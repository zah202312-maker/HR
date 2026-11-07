using System;
using System.Data.Common;
using System.Net;
using HR_Mangment2.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;

namespace HR_Mangment2.Errors;

/// <summary>
/// x-server-requirements.errorStatus: Hr:* codes return 422, except the two access gates, which return 403.
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IHttpExceptionStatusCodeFinder))]
public class HrHttpExceptionStatusCodeFinder : DefaultHttpExceptionStatusCodeFinder
{
    private const string UniqueViolationSqlState = "23505";

    public HrHttpExceptionStatusCodeFinder(IOptions<AbpExceptionHttpStatusCodeOptions> options)
        : base(options)
    {
    }

    public override HttpStatusCode GetStatusCode(HttpContext httpContext, Exception exception)
    {
        if (exception is IHasErrorCode { Code: { } code } && code.StartsWith(HrErrorCodes.Prefix, StringComparison.Ordinal))
        {
            return code is HrErrorCodes.Identity.PasswordChangeRequired or HrErrorCodes.Identity.MfaEnrollmentRequired
                ? HttpStatusCode.Forbidden
                : HttpStatusCode.UnprocessableEntity;
        }

        // Two racing requests can pass the duplicate check; the unique index then answers as "already exists".
        if (exception.InnerException is DbException { SqlState: UniqueViolationSqlState })
        {
            return HttpStatusCode.Conflict;
        }

        return base.GetStatusCode(httpContext, exception);
    }
}
