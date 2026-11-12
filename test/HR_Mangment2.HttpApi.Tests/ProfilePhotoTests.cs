using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// Profile photo (decision 2026-11-11): the owner alone uploads or removes it, everyone signed in sees it next to the
/// name through photoId in the references.
/// </summary>
[Collection(HostCollection.Name)]
public class ProfilePhotoTests
{
    private const string MyPhoto = "/api/app/me/photo";

    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 1, 2, 3 };
    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 4, 5, 6 };
    private static readonly byte[] Webp = Encoding.ASCII.GetBytes("RIFF").Concat(new byte[] { 1, 0, 0, 0 }).Concat(Encoding.ASCII.GetBytes("WEBPVP8 ")).ToArray();

    private readonly HostFixture _host;

    public ProfilePhotoTests(HostFixture host)
    {
        _host = host;
    }

    private static MultipartFormDataContent Form(byte[] content, string contentType)
    {
        var part = new ByteArrayContent(content);
        part.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { part, "file", "me.img" } };
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return JsonDocument.Parse(body).RootElement;
    }

    private static async Task ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private static string? PhotoOf(JsonElement element) =>
        element.TryGetProperty("photoId", out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

    [Fact]
    public async Task The_owner_uploads_and_everyone_sees_it_next_to_the_name()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);
        var managerClient = await kit.SignInAsync(manager);
        var storeOfficer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.StoreOfficer);

        // Wrong bytes, a mismatched type and an oversized file are refused.
        await ShouldFailWithAsync(await staffClient.PostAsync(MyPhoto, Form(Encoding.UTF8.GetBytes("not an image"), "image/png")), HrErrorCodes.Document.FileTypeNotAllowed);
        await ShouldFailWithAsync(await staffClient.PostAsync(MyPhoto, Form(Jpeg, "image/png")), HrErrorCodes.Document.FileTypeNotAllowed);
        await ShouldFailWithAsync(await staffClient.PostAsync(MyPhoto, Form(Encoding.ASCII.GetBytes("GIF89a......"), "image/gif")), HrErrorCodes.Document.FileTypeNotAllowed);
        var big = new byte[1_048_577];
        Jpeg.CopyTo(big, 0);
        await ShouldFailWithAsync(await staffClient.PostAsync(MyPhoto, Form(big, "image/jpeg")), HrErrorCodes.Document.FileTooLarge);
        (await staffClient.PostAsync(MyPhoto, new MultipartFormDataContent())).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var first = (await OkAsync(await staffClient.PostAsync(MyPhoto, Form(Png, "image/png")))).GetProperty("photoId").GetString()!;
        var me = await OkAsync(await staffClient.GetAsync("/api/app/me"));
        PhotoOf(me).ShouldBe(first);
        PhotoOf(me.GetProperty("employee")).ShouldBe(first);

        // Anyone signed in reads it, the store officer included; the content never changes for an id.
        foreach (var reader in new[] { managerClient, storeOfficer, kit.Hr })
        {
            var response = await reader.GetAsync($"/api/app/profile-photo/{first}");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
            response.Headers.CacheControl!.ToString().ShouldContain("immutable");
            (await response.Content.ReadAsByteArrayAsync()).ShouldBe(Png);
        }

        (await _host.CreateClient().GetAsync($"/api/app/profile-photo/{first}")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // The references carry it: the employee file, the team and the employees list.
        PhotoOf(await OkAsync(await kit.Hr.GetAsync($"/api/app/employee/{staff.Id}"))).ShouldBe(first);
        var team = await OkAsync(await managerClient.GetAsync("/api/app/manager/my-team?maxResultCount=200"));
        PhotoOf(team.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == staff.Id)).ShouldBe(first);
        var list = await OkAsync(await kit.Hr.GetAsync($"/api/app/employee?managerEmployeeId={manager.Id}&maxResultCount=200"));
        PhotoOf(list.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == staff.Id)).ShouldBe(first);

        // A new upload is a new id and the old one is gone.
        var second = (await OkAsync(await staffClient.PostAsync(MyPhoto, Form(Webp, "image/webp")))).GetProperty("photoId").GetString()!;
        second.ShouldNotBe(first);
        (await managerClient.GetAsync($"/api/app/profile-photo/{first}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await managerClient.GetAsync($"/api/app/profile-photo/{second}")).Content.Headers.ContentType!.MediaType.ShouldBe("image/webp");

        // Removing it clears every reference; removing again is fine.
        (await staffClient.DeleteAsync(MyPhoto)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await staffClient.DeleteAsync(MyPhoto)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        PhotoOf(await OkAsync(await staffClient.GetAsync("/api/app/me"))).ShouldBeNull();
        PhotoOf(await OkAsync(await kit.Hr.GetAsync($"/api/app/employee/{staff.Id}"))).ShouldBeNull();
        (await managerClient.GetAsync($"/api/app/profile-photo/{second}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Nobody else changes it: the operations act on the caller's own account only.
        await OkAsync(await staffClient.PostAsync(MyPhoto, Form(Jpeg, "image/jpeg")));
        (await kit.Hr.DeleteAsync(MyPhoto)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        PhotoOf(await OkAsync(await staffClient.GetAsync("/api/app/me"))).ShouldNotBeNull();
    }
}
