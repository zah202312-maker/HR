using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;

namespace HR_Mangment2.Errors;

/// <summary>
/// x-server-requirements.errorBody: details are null outside development, and
/// validationErrors[].members are camelCase JSON paths.
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IExceptionToErrorInfoConverter))]
public class HrExceptionToErrorInfoConverter : IExceptionToErrorInfoConverter, ITransientDependency
{
    private readonly DefaultExceptionToErrorInfoConverter _inner;
    private readonly IHostEnvironment _environment;

    public HrExceptionToErrorInfoConverter(DefaultExceptionToErrorInfoConverter inner, IHostEnvironment environment)
    {
        _inner = inner;
        _environment = environment;
    }

    public RemoteServiceErrorInfo Convert(Exception exception, Action<Volo.Abp.AspNetCore.ExceptionHandling.AbpExceptionHandlingOptions>? options = null)
    {
        return Apply(_inner.Convert(exception, options));
    }

    public RemoteServiceErrorInfo Convert(Exception exception, bool includeSensitiveDetails)
    {
        return Apply(_inner.Convert(exception, includeSensitiveDetails));
    }

    private RemoteServiceErrorInfo Apply(RemoteServiceErrorInfo info)
    {
        if (!_environment.IsDevelopment())
        {
            info.Details = null;
        }

        if (info.ValidationErrors != null)
        {
            foreach (var error in info.ValidationErrors)
            {
                error.Members = NormalizeMembers(error.Members);
            }
        }

        return info;
    }

    public static string[] NormalizeMembers(string[]? members)
    {
        if (members == null)
        {
            return Array.Empty<string>();
        }

        return members.Select(ToJsonPath).Where(path => path.Length > 0).Distinct().ToArray();
    }

    /// <summary>Turns "$.Items[0].Name" or "Items[0].Name" into "items[0].name".</summary>
    public static string ToJsonPath(string member)
    {
        var path = member.StartsWith('$') ? member.TrimStart('$').TrimStart('.') : member;
        return string.Join('.', path.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(ToCamelCaseSegment));
    }

    private static string ToCamelCaseSegment(string segment)
    {
        return segment.Length == 0 || char.IsLower(segment[0])
            ? segment
            : char.ToLowerInvariant(segment[0]) + segment[1..];
    }
}
