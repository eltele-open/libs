using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eltele.GeoJson.Serialization;

internal static class GeoJsonWriter
{
    public static void WritePosition(Utf8JsonWriter writer, Position position)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(position.Longitude);
        writer.WriteNumberValue(position.Latitude);
        if (position.Altitude is { } altitude)
        {
            writer.WriteNumberValue(altitude);
        }

        foreach (var ordinate in position.ExtraOrdinates)
        {
            writer.WriteNumberValue(ordinate);
        }

        writer.WriteEndArray();
    }

    public static void WritePositions(Utf8JsonWriter writer, ImmutableArray<Position> positions)
    {
        writer.WriteStartArray();
        foreach (var position in positions)
        {
            WritePosition(writer, position);
        }

        writer.WriteEndArray();
    }

    public static void WriteValues(Utf8JsonWriter writer, ImmutableArray<double> values)
    {
        writer.WriteStartArray();
        foreach (var value in values)
        {
            writer.WriteNumberValue(value);
        }

        writer.WriteEndArray();
    }

    public static void WriteBoundingBox(Utf8JsonWriter writer, BoundingBox? boundingBox)
    {
        if (boundingBox is not { } value)
        {
            return;
        }

        writer.WritePropertyName(GeoJsonMembers.BboxText);
        WriteValues(writer, value.Values);
    }

    public static void WriteForeignMembers(Utf8JsonWriter writer, JsonObject? foreignMembers)
    {
        if (foreignMembers is null)
        {
            return;
        }

        foreach (var member in foreignMembers)
        {
            if (member.Value is null)
            {
                writer.WriteNull(member.Key);
                continue;
            }

            writer.WritePropertyName(member.Key);
            member.Value.WriteTo(writer);
        }
    }
}
