using System.Text.Json;
using System.Text.Json.Serialization;

namespace HR_Mangment2.Json;

public static class HrJsonConfiguration
{
    /// <summary>
    /// x-server-requirements.enums and .dates. The converters go first, because ABP registers its own
    /// enum converter that accepts integers, and the first match wins.
    /// </summary>
    public static void Apply(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Insert(0, new HrTimeOnlyJsonConverter());
        options.Converters.Insert(0, new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    }
}
