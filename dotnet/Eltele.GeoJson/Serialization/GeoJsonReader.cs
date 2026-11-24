using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eltele.GeoJson.Serialization;

internal static class GeoJsonReader
{
    public static void ExpectStartObject(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("A GeoJSON object must be a JSON object (RFC 7946 §3).");
        }
    }

    /// <remarks>
    /// Scans a copy of the reader, so the caller keeps a reader positioned on the object it is about to read.
    /// </remarks>
    public static GeoJsonKind ReadKind(ref Utf8JsonReader reader)
    {
        ExpectStartObject(ref reader);

        var probe = reader;
        while (probe.Read() && probe.TokenType == JsonTokenType.PropertyName)
        {
            if (probe.ValueTextEquals(GeoJsonMembers.Type))
            {
                probe.Read();
                return ReadKindValue(ref probe);
            }

            probe.Read();
            probe.Skip();
        }

        throw new JsonException("A GeoJSON object must have a \"type\" member (RFC 7946 §3).");
    }

    public static void ReadAndCheckType(ref Utf8JsonReader reader, GeoJsonKind expected)
    {
        reader.Read();

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("The \"type\" member of a GeoJSON object must be a string (RFC 7946 §3).");
        }

        if (!reader.ValueTextEquals(expected.Utf8Name))
        {
            throw new JsonException(
                $"A {expected.Name} was expected here, but the \"type\" member is \"{reader.GetString()}\" (RFC 7946 §3).");
        }
    }

    public static Position ReadPosition(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("A position must be a JSON array of numbers (RFC 7946 §3.1.1).");
        }

        var ordinate = 0;
        var longitude = 0d;
        var latitude = 0d;
        double? altitude = null;
        ImmutableArray<double>.Builder? extraOrdinates = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException("A position must contain only numbers (RFC 7946 §3.1.1).");
            }

            var value = reader.GetDouble();
            switch (ordinate++)
            {
                case 0:
                    longitude = value;
                    break;
                case 1:
                    latitude = value;
                    break;
                case 2:
                    altitude = value;
                    break;
                default:
                    (extraOrdinates ??= ImmutableArray.CreateBuilder<double>()).Add(value);
                    break;
            }
        }

        if (ordinate < 2)
        {
            throw new JsonException("A position must have two or more elements (RFC 7946 §3.1.1).");
        }

        return new Position(longitude, latitude, altitude, extraOrdinates?.ToImmutable() ?? ImmutableArray<double>.Empty);
    }

    public static ImmutableArray<Position> ReadPositions(ref Utf8JsonReader reader)
    {
        ExpectCoordinatesArray(ref reader);

        var builder = ImmutableArray.CreateBuilder<Position>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            builder.Add(ReadPosition(ref reader));
        }

        return builder.ToImmutable();
    }

    public static ImmutableArray<ImmutableArray<Position>> ReadPositionArrays(ref Utf8JsonReader reader)
    {
        ExpectCoordinatesArray(ref reader);

        var builder = ImmutableArray.CreateBuilder<ImmutableArray<Position>>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            builder.Add(ReadPositions(ref reader));
        }

        return builder.ToImmutable();
    }

    public static ImmutableArray<LinearRing> ReadLinearRings(ref Utf8JsonReader reader)
    {
        ExpectCoordinatesArray(ref reader);

        var builder = ImmutableArray.CreateBuilder<LinearRing>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            builder.Add(new LinearRing(ReadPositions(ref reader)));
        }

        return builder.ToImmutable();
    }

    public static ImmutableArray<ImmutableArray<LinearRing>> ReadLinearRingArrays(ref Utf8JsonReader reader)
    {
        ExpectCoordinatesArray(ref reader);

        var builder = ImmutableArray.CreateBuilder<ImmutableArray<LinearRing>>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            builder.Add(ReadLinearRings(ref reader));
        }

        return builder.ToImmutable();
    }

    public static BoundingBox ReadBoundingBox(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("The \"bbox\" member must be a JSON array (RFC 7946 §5).");
        }

        var builder = ImmutableArray.CreateBuilder<double>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException("The \"bbox\" member must contain only numbers (RFC 7946 §5).");
            }

            builder.Add(reader.GetDouble());
        }

        return new BoundingBox(builder.ToImmutable());
    }

    public static JsonNode? ReadValue(ref Utf8JsonReader reader) => JsonNode.Parse(ref reader);

    private static void ExpectCoordinatesArray(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("The \"coordinates\" member must be a JSON array (RFC 7946 §3.1).");
        }
    }

    private static GeoJsonKind ReadKindValue(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("The \"type\" member of a GeoJSON object must be a string (RFC 7946 §3).");
        }

        foreach (var kind in GeoJsonKind.All)
        {
            if (reader.ValueTextEquals(kind.Utf8Name))
            {
                return kind;
            }
        }

        throw new JsonException($"\"{reader.GetString()}\" is not one of the nine GeoJSON types (RFC 7946 §1.4).");
    }
}
