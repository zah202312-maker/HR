using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class ErrorContractTests
{
    private readonly HostFixture _host;

    public ErrorContractTests(HostFixture host)
    {
        _host = host;
    }

    private async Task<(HttpStatusCode Status, JsonElement Error)> ThrowAsync(string kind)
    {
        var response = await _host.CreateClient("en").GetAsync("/test-probe/throw/" + kind);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (response.StatusCode, body.GetProperty("error"));
    }

    [Fact]
    public async Task Hr_business_codes_return_422_with_the_stable_code()
    {
        var (status, error) = await ThrowAsync("rule");

        status.ShouldBe((HttpStatusCode)422);
        error.GetProperty("code").GetString().ShouldBe(HrErrorCodes.Common.IdempotencyKeyReused);
    }

    [Theory]
    [InlineData("password-gate", HrErrorCodes.Identity.PasswordChangeRequired)]
    [InlineData("mfa-gate", HrErrorCodes.Identity.MfaEnrollmentRequired)]
    public async Task Access_gate_codes_return_403(string kind, string code)
    {
        var (status, error) = await ThrowAsync(kind);

        status.ShouldBe(HttpStatusCode.Forbidden);
        error.GetProperty("code").GetString().ShouldBe(code);
    }

    [Theory]
    [InlineData("concurrency", HttpStatusCode.Conflict)]
    [InlineData("not-found", HttpStatusCode.NotFound)]
    [InlineData("other-business", HttpStatusCode.Forbidden)]
    public async Task Other_exceptions_keep_their_status(string kind, HttpStatusCode expected)
    {
        var (status, _) = await ThrowAsync(kind);

        status.ShouldBe(expected);
    }

    [Fact]
    public async Task Details_are_null_outside_development()
    {
        var (_, error) = await ThrowAsync("rule");

        error.GetProperty("details").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Unhandled_exceptions_reveal_nothing()
    {
        var response = await _host.CreateClient("en").GetAsync("/test-probe/throw/unhandled");
        var text = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        text.ShouldNotContain("secret internal message");
        text.ShouldNotContain("InvalidOperationException");
        text.ShouldNotContain("at HR_Mangment2");
    }

    [Fact]
    public async Task Fluent_validation_returns_400_with_camelCase_members()
    {
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var response = await _host.CreateClient("en").PostAsync("/test-probe/fluent", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var error = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error");
        var members = error.GetProperty("validationErrors").EnumerateArray()
            .SelectMany(e => e.GetProperty("members").EnumerateArray().Select(m => m.GetString()))
            .ToList();
        members.ShouldContain("displayName");
    }

    [Fact]
    public async Task Nested_data_annotation_members_are_camelCase_paths()
    {
        using var content = new StringContent("""{"address":{}}""", System.Text.Encoding.UTF8, "application/json");
        var response = await _host.CreateClient("en").PostAsync("/test-probe/echo", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var error = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error");
        var members = error.GetProperty("validationErrors").EnumerateArray()
            .SelectMany(e => e.GetProperty("members").EnumerateArray().Select(m => m.GetString()))
            .ToList();
        members.ShouldContain("address.cityName");
    }

    [Fact]
    public void Member_paths_are_normalized()
    {
        HrExceptionToErrorInfoConverter.ToJsonPath("$.Items[0].DisplayName").ShouldBe("items[0].displayName");
        HrExceptionToErrorInfoConverter.ToJsonPath("Address.CityName").ShouldBe("address.cityName");
        HrExceptionToErrorInfoConverter.ToJsonPath("status").ShouldBe("status");
    }
}
