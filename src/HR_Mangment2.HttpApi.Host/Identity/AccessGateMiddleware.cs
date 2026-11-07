using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Users;

namespace HR_Mangment2.Identity;

/// <summary>
/// x-global-rules.access-gates. It runs on every API request, reads the account from the database and
/// answers 403 before the rules of any operation. Non-API paths such as sign-in pages are not affected.
/// </summary>
public class AccessGateMiddleware : IMiddleware, ITransientDependency
{
    private static readonly (string Method, string Path)[] AlwaysAllowed =
    {
        ("GET", "/api/abp/application-configuration"),
        ("GET", "/api/app/me"),
        ("POST", "/api/app/account/change-password"),

        // exemptOperations: the device key authorizes this path, not a user.
        ("POST", "/api/attendance-devices/punches")
    };

    private static readonly (string Method, string Path)[] AllowedDuringMfaGate =
    {
        ("GET", "/api/app/account/mfa"),
        ("POST", "/api/app/account/mfa/enroll"),
        ("POST", "/api/app/account/mfa/confirm")
    };

    private readonly ICurrentUser _currentUser;
    private readonly AccountAccessEvaluator _evaluator;
    private readonly IExceptionToErrorInfoConverter _errorConverter;
    private readonly IOptions<JsonOptions> _jsonOptions;

    public AccessGateMiddleware(
        ICurrentUser currentUser,
        AccountAccessEvaluator evaluator,
        IExceptionToErrorInfoConverter errorConverter,
        IOptions<JsonOptions> jsonOptions)
    {
        _currentUser = currentUser;
        _evaluator = evaluator;
        _errorConverter = errorConverter;
        _jsonOptions = jsonOptions;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = (context.Request.Path.Value ?? string.Empty).TrimEnd('/');

        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
            !_currentUser.IsAuthenticated ||
            _currentUser.Id == null ||
            Matches(AlwaysAllowed, context.Request.Method, path))
        {
            await next(context);
            return;
        }

        var state = await _evaluator.GetAsync(_currentUser.Id.Value);

        if (!state.Exists || !state.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var blockedBy = state.Gate switch
        {
            AccessGate.PasswordChangeRequired => HrErrorCodes.Identity.PasswordChangeRequired,
            AccessGate.MfaEnrollmentRequired when !Matches(AllowedDuringMfaGate, context.Request.Method, path) => HrErrorCodes.Identity.MfaEnrollmentRequired,
            _ => null
        };

        if (blockedBy == null)
        {
            await next(context);
            return;
        }

        var info = _errorConverter.Convert(new BusinessException(blockedBy));
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.Headers[AbpHttpConsts.AbpErrorFormat] = "true";
        await context.Response.WriteAsJsonAsync(new RemoteServiceErrorResponse(info), _jsonOptions.Value.JsonSerializerOptions);
    }

    private static bool Matches(IEnumerable<(string Method, string Path)> list, string method, string path) =>
        list.Any(item => item.Method == method && string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase));
}
