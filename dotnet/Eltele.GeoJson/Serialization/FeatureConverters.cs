using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Eltele.GeoJson.Serialization;

public sealed class FeatureConverter<TProperties> : JsonConverter<Feature<TProperties>>
    where TProperties : class
{
    public override Feature<TProperties> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => ReadCore(ref reader, options);

    public override void Write(Utf8JsonWriter writer, Feature<TProperties> value, JsonSerializerOptions options) =>
        WriteCore(writer, value, options);

    internal static Feature<TProperties> ReadCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        FeatureId? id = null;
        Geometry? geometry = null;
        TProperties? properties = null;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Id))
            {
                reader.Read();
                id = ReadId(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Geometry))
            {
                reader.Read();
                geometry = ReadGeometry(ref reader);
                continue;
            }

            if (reader.ValueTextEquals(GeoJsonMembers.Properties))
            {
                properties = ReadProperties(ref reader, options);
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
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.Feature);
                continue;
            }

            var member = reader.GetString()!;
            switch (member)
            {
                case GeoJsonMembers.IdMember:
                    reader.Read();
                    id = ReadId(ref reader);
                    break;
                case GeoJsonMembers.GeometryMember:
                    reader.Read();
                    geometry = ReadGeometry(ref reader);
                    break;
                case GeoJsonMembers.PropertiesMember:
                    properties = ReadProperties(ref reader, options);
                    break;
                case GeoJsonMembers.BboxMember:
                    reader.Read();
                    bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                    break;
                case GeoJsonMembers.TypeMember:
                    GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.Feature);
                    break;
                default:
                    reader.Read();
                    (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
                    break;
            }
        }

        return new Feature<TProperties>
        {
            Id = id,
            Geometry = geometry,
            Properties = properties,
            Bbox = bbox,
            ForeignMembers = foreignMembers,
        };
    }

    internal static void WriteCore(
        Utf8JsonWriter writer,
        Feature<TProperties> feature,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKinds.Name(GeoJsonKind.Feature));
        GeoJsonWriter.WriteBoundingBox(writer, feature.Bbox);

        if (feature.Id is { } id && id.Value is not null)
        {
            writer.WritePropertyName(GeoJsonMembers.IdText);
            FeatureIdConverter.WriteCore(writer, id);
        }

        writer.WritePropertyName(GeoJsonMembers.GeometryText);
        if (feature.Geometry is { } geometry)
        {
            GeometryConverter.WriteCore(writer, geometry);
        }
        else
        {
            writer.WriteNullValue();
        }

        writer.WritePropertyName(GeoJsonMembers.PropertiesText);
        if (feature.Properties is { } properties)
        {
            JsonSerializer.Serialize(writer, properties, options);
        }
        else
        {
            writer.WriteNullValue();
        }

        GeoJsonWriter.WriteForeignMembers(writer, feature.ForeignMembers);
        writer.WriteEndObject();
    }

    // A conditional would take the union's struct as its natural type here and turn null into default(FeatureId),
    // which is a value without a case rather than an absent id.
    private static FeatureId? ReadId(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return FeatureIdConverter.ReadCore(ref reader);
    }

    private static Geometry? ReadGeometry(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return GeometryConverter.ReadCore(ref reader);
    }

    private static TProperties? ReadProperties(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        reader.Read();
        return reader.TokenType == JsonTokenType.Null ? null : JsonSerializer.Deserialize<TProperties>(ref reader, options);
    }
}

public sealed class FeatureCollectionConverter<TProperties> : JsonConverter<FeatureCollection<TProperties>>
    where TProperties : class
{
    public override FeatureCollection<TProperties> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => ReadCore(ref reader, options);

    public override void Write(
        Utf8JsonWriter writer,
        FeatureCollection<TProperties> value,
        JsonSerializerOptions options) => WriteCore(writer, value, options);

    internal static FeatureCollection<TProperties> ReadCore(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options)
    {
        GeoJsonReader.ExpectStartObject(ref reader);

        var features = ImmutableArray<Feature<TProperties>>.Empty;
        BoundingBox? bbox = null;
        JsonObject? foreignMembers = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(GeoJsonMembers.Features))
            {
                reader.Read();
                features = ReadFeatures(ref reader, options);
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
                GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.FeatureCollection);
                continue;
            }

            var member = reader.GetString()!;
            switch (member)
            {
                case GeoJsonMembers.FeaturesMember:
                    reader.Read();
                    features = ReadFeatures(ref reader, options);
                    break;
                case GeoJsonMembers.BboxMember:
                    reader.Read();
                    bbox = GeoJsonReader.ReadBoundingBox(ref reader);
                    break;
                case GeoJsonMembers.TypeMember:
                    GeoJsonReader.ReadAndCheckType(ref reader, GeoJsonKind.FeatureCollection);
                    break;
                default:
                    reader.Read();
                    (foreignMembers ??= new JsonObject())[member] = GeoJsonReader.ReadValue(ref reader);
                    break;
            }
        }

        return new FeatureCollection<TProperties>
        {
            Features = features,
            Bbox = bbox,
            ForeignMembers = foreignMembers,
        };
    }

    internal static void WriteCore(
        Utf8JsonWriter writer,
        FeatureCollection<TProperties> collection,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(GeoJsonMembers.TypeText, GeoJsonKinds.Name(GeoJsonKind.FeatureCollection));
        GeoJsonWriter.WriteBoundingBox(writer, collection.Bbox);
        writer.WritePropertyName(GeoJsonMembers.FeaturesText);
        writer.WriteStartArray();
        foreach (var feature in collection.Features)
        {
            FeatureConverter<TProperties>.WriteCore(writer, feature, options);
        }

        writer.WriteEndArray();
        GeoJsonWriter.WriteForeignMembers(writer, collection.ForeignMembers);
        writer.WriteEndObject();
    }

    private static ImmutableArray<Feature<TProperties>> ReadFeatures(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("The \"features\" member must be a JSON array (RFC 7946 §3.3).");
        }

        var builder = ImmutableArray.CreateBuilder<Feature<TProperties>>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            builder.Add(FeatureConverter<TProperties>.ReadCore(ref reader, options));
        }

        return builder.ToImmutable();
    }
}

public sealed class GeoJsonObjectConverter : JsonConverter<GeoJsonObject>
{
    public override GeoJsonObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadCore(ref reader, options);

    public override void Write(Utf8JsonWriter writer, GeoJsonObject value, JsonSerializerOptions options) =>
        WriteCore(writer, value, options);

    internal static GeoJsonObject ReadCore(ref Utf8JsonReader reader, JsonSerializerOptions options) =>
        GeoJsonReader.ReadKind(ref reader) switch
        {
            GeoJsonKind.Feature => new GeoJsonObject(FeatureConverter<JsonNode>.ReadCore(ref reader, options)),
            GeoJsonKind.FeatureCollection => new GeoJsonObject(
                FeatureCollectionConverter<JsonNode>.ReadCore(ref reader, options)),
            GeoJsonKind.Point
                or GeoJsonKind.MultiPoint
                or GeoJsonKind.LineString
                or GeoJsonKind.MultiLineString
                or GeoJsonKind.Polygon
                or GeoJsonKind.MultiPolygon
                or GeoJsonKind.GeometryCollection => new GeoJsonObject(GeometryConverter.ReadCore(ref reader)),
            _ => throw new JsonException("A GeoJSON object is a Geometry, a Feature, or a FeatureCollection (RFC 7946 §3)."),
        };

    internal static void WriteCore(Utf8JsonWriter writer, GeoJsonObject value, JsonSerializerOptions options)
    {
        switch (value.Value)
        {
            case Feature<JsonNode> feature:
                FeatureConverter<JsonNode>.WriteCore(writer, feature, options);
                break;
            case FeatureCollection<JsonNode> collection:
                FeatureCollectionConverter<JsonNode>.WriteCore(writer, collection, options);
                break;
            case Geometry geometry:
                GeometryConverter.WriteCore(writer, geometry);
                break;
            default:
                throw new JsonException("The GeoJSON object holds no value.");
        }
    }
}

public sealed class GeoJsonObjectConverter<TProperties> : JsonConverter<GeoJsonObject<TProperties>>
    where TProperties : class
{
    public override GeoJsonObject<TProperties> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => GeoJsonReader.ReadKind(ref reader) switch
    {
        GeoJsonKind.Feature => new GeoJsonObject<TProperties>(
            FeatureConverter<TProperties>.ReadCore(ref reader, options)),
        GeoJsonKind.FeatureCollection => new GeoJsonObject<TProperties>(
            FeatureCollectionConverter<TProperties>.ReadCore(ref reader, options)),
        GeoJsonKind.Point
            or GeoJsonKind.MultiPoint
            or GeoJsonKind.LineString
            or GeoJsonKind.MultiLineString
            or GeoJsonKind.Polygon
            or GeoJsonKind.MultiPolygon
            or GeoJsonKind.GeometryCollection => new GeoJsonObject<TProperties>(GeometryConverter.ReadCore(ref reader)),
        _ => throw new JsonException("A GeoJSON object is a Geometry, a Feature, or a FeatureCollection (RFC 7946 §3)."),
    };

    public override void Write(
        Utf8JsonWriter writer,
        GeoJsonObject<TProperties> value,
        JsonSerializerOptions options)
    {
        switch (value.Value)
        {
            case Feature<TProperties> feature:
                FeatureConverter<TProperties>.WriteCore(writer, feature, options);
                break;
            case FeatureCollection<TProperties> collection:
                FeatureCollectionConverter<TProperties>.WriteCore(writer, collection, options);
                break;
            case Geometry geometry:
                GeometryConverter.WriteCore(writer, geometry);
                break;
            default:
                throw new JsonException("The GeoJSON object holds no value.");
        }
    }
}
