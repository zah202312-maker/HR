using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class LocalizationAndTenancyTests
{
    private const string Arabic = "استُخدم مفتاح منع التكرار نفسه مع محتوى مختلف.";
    private const string English = "The same idempotency key was used with different content.";

    private readonly HostFixture _host;

    public LocalizationAndTenancyTests(HostFixture host)
    {
        _host = host;
    }

    [Theory]
    [InlineData(null, Arabic)]
    [InlineData("ar", Arabic)]
    [InlineData("ar-IQ", Arabic)]
    [InlineData("en", English)]
    [InlineData("en-US", English)]
    [InlineData("fr", Arabic)]
    [InlineData("xx-YY", Arabic)]
    public async Task Error_message_follows_accept_language_and_falls_back_to_arabic(string? language, string expected)
    {
        var response = await _host.CreateClient(language).GetAsync("/test-probe/throw/rule");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("error").GetProperty("message").GetString().ShouldBe(expected);
    }

    [Fact]
    public async Task Application_configuration_is_anonymous_and_only_offers_two_languages()
    {
        var response = await _host.CreateClient().GetAsync("/api/abp/application-configuration");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var languages = json.GetProperty("localization").GetProperty("languages");
        languages.GetArrayLength().ShouldBe(2);
        json.GetProperty("localization").GetProperty("currentCulture").GetProperty("cultureName").GetString().ShouldBe("ar");
        json.GetProperty("currentUser").GetProperty("isAuthenticated").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task Tenant_comes_from_the_server_and_clients_cannot_change_it()
    {
        var client = _host.CreateClient();
        var plain = await client.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        plain.GetProperty("currentTenant").GetProperty("name").GetString().ShouldBe(HostFixture.TestTenantName);

        var spoofed = _host.CreateClient();
        spoofed.DefaultRequestHeaders.Add("__tenant", "another-company");
        var response = await spoofed.GetAsync("/api/abp/application-configuration?__tenant=another-company");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("currentTenant").GetProperty("name").GetString().ShouldBe(HostFixture.TestTenantName);
    }

    [Fact]
    public async Task Self_registration_is_off_on_the_server()
    {
        var json = await _host.CreateClient().GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");

        json.GetProperty("setting").GetProperty("values")
            .GetProperty("Abp.Account.IsSelfRegistrationEnabled").GetString()!.ToLowerInvariant().ShouldBe("false");
    }

    [Fact]
    public async Task OpenId_discovery_and_sign_in_page_respond()
    {
        var client = _host.CreateClient();

        var discovery = await client.GetAsync("/.well-known/openid-configuration");
        discovery.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await discovery.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("token_endpoint").GetString().ShouldEndWith("/connect/token");

        var login = await client.GetAsync("/Account/Login");
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
