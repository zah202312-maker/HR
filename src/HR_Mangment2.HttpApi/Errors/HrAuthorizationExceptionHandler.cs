using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Users;

namespace HR_Mangment2.Errors;

/// <summary>
/// On API paths the framework must not start an authentication challenge, so 401 and 403
/// come back in the RemoteServiceErrorResponse format. Sign-in pages keep the default behaviour.
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAbpAuthorizationExceptionHandler))]
public class HrAuthorizationExceptionHandler : IAbpAuthorizationExceptionHandler, ITransientDependency
{
    private readonly DefaultAbpAuthorizationExceptionHandler _inner;
    private readonly IExceptionToErrorInfoConverter _converter;
    private readonly ICurrentUser _currentUser;
    private readonly IOptions<JsonOptions> _jsonOptions;

    public HrAuthorizationExceptionHandler(
        DefaultAbpAuthorizationExceptionHandler inner,
        IExceptionToErrorInfoConverter converter,
        ICurrentUser currentUser,
        IOptions<JsonOptions> jsonOptions)
    {
        _inner = inner;
        _converter = converter;
        _currentUser = currentUser;
        _jsonOptions = jsonOptions;
    }

    public async Task HandleAsync(AbpAuthorizationException exception, HttpContext httpContext)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
        {
            await _inner.HandleAsync(exception, httpContext);
            return;
        }

        httpContext.Response.StatusCode = _currentUser.IsAuthenticated
            ? StatusCodes.Status403Forbidden
            : StatusCodes.Status401Unauthorized;
        httpContext.Response.Headers[AbpHttpConsts.AbpErrorFormat] = "true";

        await httpContext.Response.WriteAsJsonAsync(
            new RemoteServiceErrorResponse(_converter.Convert(exception)),
            _jsonOptions.Value.JsonSerializerOptions);
    }
}
