using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HR_Mangment2.Json;

/// <summary>Time-only values in the contract are "HH:mm".</summary>
public class HrTimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;

        if (TimeOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            return time;
        }

        throw new JsonException($"The time must use the {Format} format.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
