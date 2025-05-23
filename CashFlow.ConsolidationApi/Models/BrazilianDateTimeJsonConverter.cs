using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CashFlow.ConsolidationApi.Models;

public class BrazilianDateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string _format = "dd/MM/yyyy";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateStr = reader.GetString();

        if (DateTime.TryParseExact(dateStr, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        throw new JsonException($"Data inválida. Use o formato {_format}.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
    }
}
