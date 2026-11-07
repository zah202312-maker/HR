using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shouldly;

namespace HR_Mangment2.Infrastructure;

public record TokenPair(string AccessToken, string? RefreshToken);

/// <summary>Signs in through the real browser flow: authorize, sign-in page, code exchange with PKCE.</summary>
public static class TestAuth
{
    public const string ClientId = "HR_Mangment2_Flutter";
    public const string RedirectUri = "com.hrsystem.app://oauth/callback";

    public static async Task<HttpResponseMessage> StartSignInAsync(HttpClient browser, string challenge)
    {
        var url = "/connect/authorize?client_id=" + ClientId +
                  "&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
                  "&response_type=code&scope=" + Uri.EscapeDataString("openid profile offline_access HR_Mangment2") +
                  "&code_challenge=" + challenge + "&code_challenge_method=S256";
        return await browser.GetAsync(url);
    }

    public static async Task<TokenPair> SignInAsync(HostFixture host, string userName, string password, string? totpSecret = null)
    {
        var browser = host.CreateClient();
        var verifier = Base64Url(RandomNumberGenerator.GetBytes(32));
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

        var response = await StartSignInAsync(browser, challenge);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        var loginUrl = response.Headers.Location!.ToString();

        var page = await browser.GetStringAsync(loginUrl);
        response = await browser.PostAsync(loginUrl, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["LoginInput.UserNameOrEmailAddress"] = userName,
            ["LoginInput.Password"] = password,
            ["LoginInput.RememberMe"] = "false",
            ["Action"] = "Login",
            ["__RequestVerificationToken"] = Antiforgery(page)
        }));
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect, "sign-in of " + userName + " failed");

        var next = response.Headers.Location!.ToString();
        if (next.Contains("LoginTotp", StringComparison.OrdinalIgnoreCase))
        {
            totpSecret.ShouldNotBeNull();
            var totpPage = await browser.GetStringAsync(next);
            response = await browser.PostAsync(next, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Code"] = Totp.Compute(totpSecret!),
                ["__RequestVerificationToken"] = Antiforgery(totpPage)
            }));
            response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            next = response.Headers.Location!.ToString();
        }

        response = await browser.GetAsync(next);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        var callback = response.Headers.Location!.ToString();
        callback.ShouldStartWith(RedirectUri);
        var code = Uri.UnescapeDataString(Regex.Match(callback, "[?&]code=([^&]+)").Groups[1].Value);

        var token = await host.CreateClient().PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = ClientId,
            ["code"] = code,
            ["redirect_uri"] = RedirectUri,
            ["code_verifier"] = verifier
        }));
        token.StatusCode.ShouldBe(HttpStatusCode.OK, await token.Content.ReadAsStringAsync());

        var json = await token.Content.ReadFromJsonAsync<JsonElement>();
        return new TokenPair(
            json.GetProperty("access_token").GetString()!,
            json.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() : null);
    }

    public static HttpClient ClientFor(HostFixture host, string accessToken, string? language = null)
    {
        var client = host.CreateClient(language);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private static string Antiforgery(string html) =>
        Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

/// <summary>RFC 6238 codes, as an authenticator app computes them.</summary>
public static class Totp
{
    public static string Compute(string base32Secret)
    {
        var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var data = BitConverter.GetBytes(counter);
        Array.Reverse(data);

        using var hmac = new HMACSHA1(FromBase32(base32Secret));
        var hash = hmac.ComputeHash(data);
        var offset = hash[^1] & 0x0F;
        var value = ((hash[offset] & 0x7F) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (value % 1_000_000).ToString("D6");
    }

    private static byte[] FromBase32(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = 0;
        var buffer = 0;
        var output = new List<byte>();
        foreach (var c in input.TrimEnd('=').ToUpperInvariant())
        {
            buffer = (buffer << 5) | alphabet.IndexOf(c);
            bits += 5;
            if (bits >= 8)
            {
                output.Add((byte)(buffer >> (bits - 8)));
                bits -= 8;
            }
        }

        return output.ToArray();
    }
}
