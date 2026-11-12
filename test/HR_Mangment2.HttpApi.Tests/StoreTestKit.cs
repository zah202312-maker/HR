using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Shouldly;

namespace HR_Mangment2;

/// <summary>Since 2026-11-11 an item needs a SKU, a category and a unit. Each call makes fresh ones, so tests never collide.</summary>
public static class StoreTestKit
{
    public static async Task<object> ItemBodyAsync(HttpClient officer, string nameAr, string nameEn, string kind, decimal minQuantity)
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var category = await CreateAsync(officer, "/api/app/store/item-category", new { nameAr = "تصنيف " + suffix, nameEn = "Category " + suffix });
        var unit = await CreateAsync(officer, "/api/app/store/item-unit", new { nameAr = "وحدة " + suffix, nameEn = "Unit " + suffix });

        return new { nameAr, nameEn, sku = "SKU-" + suffix, categoryId = category, unitId = unit, kind, minQuantity };
    }

    private static async Task<Guid> CreateAsync(HttpClient officer, string url, object body)
    {
        var response = await officer.PostAsJsonAsync(url, body);
        var text = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.Created, text);
        return JsonDocument.Parse(text).RootElement.GetProperty("id").GetGuid();
    }
}
