using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eltele.GeoJson.Serialization;

public sealed class PositionConverter : JsonConverter<Position>
{
    public override Position Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        GeoJsonReader.ReadPosition(ref reader);

    public override void Write(Utf8JsonWriter writer, Position value, JsonSerializerOptions options) =>
        GeoJsonWriter.WritePosition(writer, value);
}

public sealed class BoundingBoxConverter : JsonConverter<BoundingBox>
{
    public override BoundingBox Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        GeoJsonReader.ReadBoundingBox(ref reader);

    public override void Write(Utf8JsonWriter writer, BoundingBox value, JsonSerializerOptions options) =>
        GeoJsonWriter.WriteValues(writer, value.Values);
}

public sealed class LinearRingConverter : JsonConverter<LinearRing>
{
    public override LinearRing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(GeoJsonReader.ReadPositions(ref reader));

    public override void Write(Utf8JsonWriter writer, LinearRing value, JsonSerializerOptions options) =>
        GeoJsonWriter.WritePositions(writer, value.Positions);
}

public sealed class FeatureIdConverter : JsonConverter<FeatureId>
{
    public override FeatureId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, FeatureId value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static FeatureId ReadCore(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.TokenType switch
        {
            JsonTokenType.String => new FeatureId(reader.GetString()!),
            JsonTokenType.Number => new FeatureId(reader.GetDouble()),
            _ => throw new JsonException(
                "The \"id\" member of a Feature is either a JSON string or a number (RFC 7946 §3.2)."),
        };
    }

    internal static void WriteCore(Utf8JsonWriter writer, FeatureId id)
    {
        switch (id.Value)
        {
            case string text:
                writer.WriteStringValue(text);
                break;
            case double number:
                writer.WriteNumberValue(number);
                break;
            default:
                throw new JsonException(
                    "The \"id\" member of a Feature is either a JSON string or a number (RFC 7946 §3.2).");
        }
    }
}
