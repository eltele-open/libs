using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Eltele.GeoJson.Serialization;

// Each geometry reads and writes its own member loop instead of sharing one: the coordinate shape differs per type,
// and a shared helper cannot take a callback that captures the ref struct Utf8JsonReader.
public sealed class PointConverter : JsonConverter<Point>
{
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static Point ReadCore(ref Utf8JsonReader reader)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var coordinates = default(Position);
        var hasCoordinates = false;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Coordinates))
            {
                reader.Read();
                coordinates = GeoJsonReader.ReadPosition(ref reader);
                hasCoordinates = true;
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Bbox))
            {
                reader.Read();
                bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Type))
            {
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.Point);
                continue;
            }

            var member = reader.GetString()!;
            reader.Read();
            (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
        }

        if (!hasCoordinates)
        {
            throw new JsonException("A Point must have a \"coordinates\" member (RFC 7946 §3.1).");
        }

        return new Point { Coordinates = coordinates, Bbox = bbox, ForeignMembers = foreignMembers };
    }

    internal static void WriteCore(Utf8JsonWriter writer, Point point)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKind.Point.Name);
        GeoJsonWriter.WriteBoundingBox(writer, point.Bbox);
        writer.WritePropertyName(GeoJsonMembers.CoordinatesText);
        GeoJsonWriter.WritePosition(writer, point.Coordinates);
        GeoJsonWriter.WriteForeignMembers(writer, point.ForeignMembers);
        writer.WriteEndObject();
    }
}

public sealed class MultiPointConverter : JsonConverter<MultiPoint>
{
    public override MultiPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, MultiPoint value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static MultiPoint ReadCore(ref Utf8JsonReader reader)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var coordinates = ImmutableArray<Position>.Empty;
        var hasCoordinates = false;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Coordinates))
            {
                reader.Read();
                coordinates = GeoJsonReader.ReadPositions(ref reader);
                hasCoordinates = true;
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Bbox))
            {
                reader.Read();
                bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Type))
            {
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.MultiPoint);
                continue;
            }

            var member = reader.GetString()!;
            reader.Read();
            (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
        }

        return hasCoordinates
            ? new MultiPoint { Coordinates = coordinates, Bbox = bbox, ForeignMembers = foreignMembers }
            : throw new JsonException("A MultiPoint must have a \"coordinates\" member (RFC 7946 §3.1).");
    }

    internal static void WriteCore(Utf8JsonWriter writer, MultiPoint multiPoint)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKind.MultiPoint.Name);
        GeoJsonWriter.WriteBoundingBox(writer, multiPoint.Bbox);
        writer.WritePropertyName(GeoJsonMembers.CoordinatesText);
        GeoJsonWriter.WritePositions(writer, multiPoint.Coordinates);
        GeoJsonWriter.WriteForeignMembers(writer, multiPoint.ForeignMembers);
        writer.WriteEndObject();
    }
}

public sealed class LineStringConverter : JsonConverter<LineString>
{
    public override LineString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, LineString value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static LineString ReadCore(ref Utf8JsonReader reader)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var coordinates = ImmutableArray<Position>.Empty;
        var hasCoordinates = false;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Coordinates))
            {
                reader.Read();
                coordinates = GeoJsonReader.ReadPositions(ref reader);
                hasCoordinates = true;
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Bbox))
            {
                reader.Read();
                bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Type))
            {
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.LineString);
                continue;
            }

            var member = reader.GetString()!;
            reader.Read();
            (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
        }

        return hasCoordinates
            ? new LineString { Coordinates = coordinates, Bbox = bbox, ForeignMembers = foreignMembers }
            : throw new JsonException("A LineString must have a \"coordinates\" member (RFC 7946 §3.1).");
    }

    internal static void WriteCore(Utf8JsonWriter writer, LineString lineString)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKind.LineString.Name);
        GeoJsonWriter.WriteBoundingBox(writer, lineString.Bbox);
        writer.WritePropertyName(GeoJsonMembers.CoordinatesText);
        GeoJsonWriter.WritePositions(writer, lineString.Coordinates);
        GeoJsonWriter.WriteForeignMembers(writer, lineString.ForeignMembers);
        writer.WriteEndObject();
    }
}

public sealed class MultiLineStringConverter : JsonConverter<MultiLineString>
{
    public override MultiLineString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, MultiLineString value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static MultiLineString ReadCore(ref Utf8JsonReader reader)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var coordinates = ImmutableArray<ImmutableArray<Position>>.Empty;
        var hasCoordinates = false;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Coordinates))
            {
                reader.Read();
                coordinates = GeoJsonReader.ReadPositionArrays(ref reader);
                hasCoordinates = true;
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Bbox))
            {
                reader.Read();
                bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Type))
            {
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.MultiLineString);
                continue;
            }

            var member = reader.GetString()!;
            reader.Read();
            (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
        }

        return hasCoordinates
            ? new MultiLineString { Coordinates = coordinates, Bbox = bbox, ForeignMembers = foreignMembers }
            : throw new JsonException("A MultiLineString must have a \"coordinates\" member (RFC 7946 §3.1).");
    }

    internal static void WriteCore(Utf8JsonWriter writer, MultiLineString multiLineString)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKind.MultiLineString.Name);
        GeoJsonWriter.WriteBoundingBox(writer, multiLineString.Bbox);
        writer.WritePropertyName(GeoJsonMembers.CoordinatesText);
        writer.WriteStartArray();
        foreach (var positions in multiLineString.Coordinates)
        {
            GeoJsonWriter.WritePositions(writer, positions);
        }

        writer.WriteEndArray();
        GeoJsonWriter.WriteForeignMembers(writer, multiLineString.ForeignMembers);
        writer.WriteEndObject();
    }
}

public sealed class PolygonConverter : JsonConverter<Polygon>
{
    public override Polygon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, Polygon value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static Polygon ReadCore(ref Utf8JsonReader reader)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var coordinates = ImmutableArray<LinearRing>.Empty;
        var hasCoordinates = false;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Coordinates))
            {
                reader.Read();
                coordinates = GeoJsonReader.ReadLinearRings(ref reader);
                hasCoordinates = true;
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Bbox))
            {
                reader.Read();
                bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Type))
            {
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.Polygon);
                continue;
            }

            var member = reader.GetString()!;
            reader.Read();
            (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
        }

        return hasCoordinates
            ? new Polygon { Coordinates = coordinates, Bbox = bbox, ForeignMembers = foreignMembers }
            : throw new JsonException("A Polygon must have a \"coordinates\" member (RFC 7946 §3.1).");
    }

    internal static void WriteCore(Utf8JsonWriter writer, Polygon polygon)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKind.Polygon.Name);
        GeoJsonWriter.WriteBoundingBox(writer, polygon.Bbox);
        writer.WritePropertyName(GeoJsonMembers.CoordinatesText);
        writer.WriteStartArray();
        foreach (var ring in polygon.Coordinates)
        {
            GeoJsonWriter.WritePositions(writer, ring.Positions);
        }

        writer.WriteEndArray();
        GeoJsonWriter.WriteForeignMembers(writer, polygon.ForeignMembers);
        writer.WriteEndObject();
    }
}

public sealed class MultiPolygonConverter : JsonConverter<MultiPolygon>
{
    public override MultiPolygon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, MultiPolygon value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static MultiPolygon ReadCore(ref Utf8JsonReader reader)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var coordinates = ImmutableArray<ImmutableArray<LinearRing>>.Empty;
        var hasCoordinates = false;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Coordinates))
            {
                reader.Read();
                coordinates = GeoJsonReader.ReadLinearRingArrays(ref reader);
                hasCoordinates = true;
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Bbox))
            {
                reader.Read();
                bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Type))
            {
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.MultiPolygon);
                continue;
            }

            var member = reader.GetString()!;
            reader.Read();
            (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
        }

        return hasCoordinates
            ? new MultiPolygon { Coordinates = coordinates, Bbox = bbox, ForeignMembers = foreignMembers }
            : throw new JsonException("A MultiPolygon must have a \"coordinates\" member (RFC 7946 §3.1).");
    }

    internal static void WriteCore(Utf8JsonWriter writer, MultiPolygon multiPolygon)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKind.MultiPolygon.Name);
        GeoJsonWriter.WriteBoundingBox(writer, multiPolygon.Bbox);
        writer.WritePropertyName(GeoJsonMembers.CoordinatesText);
        writer.WriteStartArray();
        foreach (var rings in multiPolygon.Coordinates)
        {
            writer.WriteStartArray();
            foreach (var ring in rings)
            {
                GeoJsonWriter.WritePositions(writer, ring.Positions);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
        GeoJsonWriter.WriteForeignMembers(writer, multiPolygon.ForeignMembers);
        writer.WriteEndObject();
    }
}

public sealed class GeometryCollectionConverter : JsonConverter<GeometryCollection>
{
    public override GeometryCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, GeometryCollection value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static GeometryCollection ReadCore(ref Utf8JsonReader reader)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var geometries = ImmutableArray<Geometry>.Empty;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Geometries))
            {
                reader.Read();
                geometries = ReadGeometries(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Bbox))
            {
                reader.Read();
                bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Type))
            {
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.GeometryCollection);
                continue;
            }

            var member = reader.GetString()!;
            reader.Read();
            (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
        }

        return new GeometryCollection { Geometries = geometries, Bbox = bbox, ForeignMembers = foreignMembers };
    }

    internal static void WriteCore(Utf8JsonWriter writer, GeometryCollection collection)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKind.GeometryCollection.Name);
        GeoJsonWriter.WriteBoundingBox(writer, collection.Bbox);
        writer.WritePropertyName(GeoJsonMembers.GeometriesText);
        writer.WriteStartArray();
        foreach (var geometry in collection.Geometries)
        {
            GeometryConverter.WriteCore(writer, geometry);
        }

        writer.WriteEndArray();
        GeoJsonWriter.WriteForeignMembers(writer, collection.ForeignMembers);
        writer.WriteEndObject();
    }

    private static ImmutableArray<Geometry> ReadGeometries(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("The \"geometries\" member must be a JSON array (RFC 7946 §3.1.8).");
        }

        var builder = ImmutableArray.CreateBuilder<Geometry>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            builder.Add(GeometryConverter.ReadCore(ref reader));
        }

        return builder.ToImmutable();
    }
}

public sealed class GeometryConverter : JsonConverter<Geometry>
{
    public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader);

    public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options) =>
        WriteCore(writer, value);

    internal static Geometry ReadCore(ref Utf8JsonReader reader) => GeoJsonReader.ReadKind(ref reader) switch
    {
        PointKind => new Geometry(PointConverter.ReadCore(ref reader)),
        MultiPointKind => new Geometry(MultiPointConverter.ReadCore(ref reader)),
        LineStringKind => new Geometry(LineStringConverter.ReadCore(ref reader)),
        MultiLineStringKind => new Geometry(MultiLineStringConverter.ReadCore(ref reader)),
        PolygonKind => new Geometry(PolygonConverter.ReadCore(ref reader)),
        MultiPolygonKind => new Geometry(MultiPolygonConverter.ReadCore(ref reader)),
        GeometryCollectionKind => new Geometry(GeometryCollectionConverter.ReadCore(ref reader)),
        FeatureKind or FeatureCollectionKind => throw new JsonException(
            "A Geometry must have one of the seven geometry types (RFC 7946 §3.1)."),
    };

    internal static void WriteCore(Utf8JsonWriter writer, Geometry geometry)
    {
        switch (geometry.Value)
        {
            case Point point:
                PointConverter.WriteCore(writer, point);
                break;
            case MultiPoint multiPoint:
                MultiPointConverter.WriteCore(writer, multiPoint);
                break;
            case LineString lineString:
                LineStringConverter.WriteCore(writer, lineString);
                break;
            case MultiLineString multiLineString:
                MultiLineStringConverter.WriteCore(writer, multiLineString);
                break;
            case Polygon polygon:
                PolygonConverter.WriteCore(writer, polygon);
                break;
            case MultiPolygon multiPolygon:
                MultiPolygonConverter.WriteCore(writer, multiPolygon);
                break;
            case GeometryCollection collection:
                GeometryCollectionConverter.WriteCore(writer, collection);
                break;
            default:
                throw new JsonException("The Geometry holds no value.");
        }
    }
}
