using System;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;

namespace HR_Mangment2.Identity;

/// <summary>
/// The validation handler keeps private OpenIddict claims out of the principal, so the id is read from the
/// bearer token of the request. The token is already validated when an authenticated service reads this.
/// </summary>
[ExposeServices(typeof(ICurrentAccessTokenAccessor))]
public class HttpCurrentAccessTokenAccessor : ICurrentAccessTokenAccessor, ITransientDependency
{
    private const string BearerPrefix = "Bearer ";
    private const string TokenIdClaim = "oi_tkn_id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentAccessTokenAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetTokenId()
    {
        var header = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var parts = header[BearerPrefix.Length..].Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
        return document.RootElement.TryGetProperty(TokenIdClaim, out var id) ? id.GetString() : null;
    }
}
