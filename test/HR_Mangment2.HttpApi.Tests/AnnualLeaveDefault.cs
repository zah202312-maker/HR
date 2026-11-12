using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace HR_Mangment2;

/// <summary>
/// Decision 2026-11-10: nobody sets an employee's entitlement by hand. A leave year opens with the company default
/// (system-settings/annual-leave) on the first read of the balance, so a test picks the number by setting the default
/// just before that read. The tests share one collection and run one at a time, so the shared default is safe.
/// </summary>
public static class AnnualLeaveDefault
{
    public const string Url = "/api/app/system-settings/annual-leave";

    public static async Task SetAsync(HttpClient hr, decimal days)
    {
        var settings = await hr.GetFromJsonAsync<JsonElement>(Url);
        (await hr.PutAsJsonAsync(Url, new { defaultEntitlementDays = days, concurrencyStamp = settings.GetProperty("concurrencyStamp").GetString() }))
            .EnsureSuccessStatusCode();
    }

    /// <summary>Sets the default, then reads the balance, which opens the employee's year with it.</summary>
    public static async Task<JsonElement> OpenWithAsync(HttpClient hr, Guid employeeId, decimal days)
    {
        await SetAsync(hr, days);
        var response = await hr.GetAsync($"/api/app/leave-balance/employee/{employeeId}");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Reading the balance failed with {(int)response.StatusCode}: {body}");
        }

        return JsonDocument.Parse(body).RootElement;
    }
}
