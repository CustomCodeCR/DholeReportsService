using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dhole.Reports.Contracts.Serialization;

/// <summary>
/// Permite que los contratos reciban un JSON como objeto/arreglo o como string JSON.
/// Internamente siempre se conserva como texto JSON válido.
/// </summary>
public sealed class FlexibleJsonStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString() ?? "{}";

        using var document = JsonDocument.ParseValue(ref reader);
        return document.RootElement.GetRawText();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            document.RootElement.WriteTo(writer);
        }
        catch (JsonException)
        {
            writer.WriteStringValue(value);
        }
    }
}
