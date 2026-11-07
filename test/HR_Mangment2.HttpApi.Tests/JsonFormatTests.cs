using System.Linq;
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
public class JsonFormatTests
{
    private const string ValidBody = """
        {"status":"onHold","date":"2026-03-15","time":"08:30","at":"2026-03-15T06:10:00Z","amount":12.5}
        """;

    private readonly HostFixture _host;

    public JsonFormatTests(HostFixture host)
    {
        _host = host;
    }

    private async Task<HttpResponseMessage> PostAsync(string path, string json)
    {
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await _host.CreateClient().PostAsync(path, content);
    }

    [Fact]
    public async Task Output_uses_contract_formats()
    {
        var json = await _host.CreateClient().GetFromJsonAsync<JsonElement>("/test-probe/format");

        json.GetProperty("status").GetString().ShouldBe("onHold");
        json.GetProperty("date").GetString().ShouldBe("2026-03-15");
        json.GetProperty("time").GetString().ShouldBe("08:30");
        json.GetProperty("at").GetString().ShouldBe("2026-03-15T06:10:00Z");
        json.GetProperty("amount").ValueKind.ShouldBe(JsonValueKind.Number);
        json.GetProperty("note").ValueKind.ShouldBe(JsonValueKind.Null);
        json.GetProperty("optionalTime").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Valid_input_round_trips()
    {
        var response = await PostAsync("/test-probe/echo", ValidBody);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("status").GetString().ShouldBe("onHold");
        json.GetProperty("time").GetString().ShouldBe("08:30");
    }

    [Theory]
    [InlineData("""{"status":1}""", "status")]
    [InlineData("""{"status":"unknownValue"}""", "status")]
    [InlineData("""{"time":"8:30:00"}""", "time")]
    [InlineData("""{"date":"15/03/2026"}""", "date")]
    public async Task Invalid_formats_return_400_with_the_json_path(string body, string expectedMember)
    {
        var response = await PostAsync("/test-probe/echo", body);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var error = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error");
        var members = error.GetProperty("validationErrors").EnumerateArray()
            .SelectMany(e => e.GetProperty("members").EnumerateArray().Select(m => m.GetString()))
            .ToList();
        members.ShouldContain(expectedMember);
    }

    [Fact]
    public async Task Paging_rejects_more_than_200_and_defaults_to_20()
    {
        var client = _host.CreateClient();

        (await client.GetAsync("/test-probe/page?maxResultCount=201")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await client.GetAsync("/test-probe/page?maxResultCount=200")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var json = await client.GetFromJsonAsync<JsonElement>("/test-probe/page");
        json.GetProperty("maxResultCount").GetInt32().ShouldBe(20);
    }
}
