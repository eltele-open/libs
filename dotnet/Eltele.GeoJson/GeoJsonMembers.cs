using System.Text.Json;

namespace Eltele.GeoJson;

internal static class GeoJsonMembers
{
    public const string TypeMember = "type";
    public const string CoordinatesMember = "coordinates";
    public const string GeometriesMember = "geometries";
    public const string GeometryMember = "geometry";
    public const string PropertiesMember = "properties";
    public const string FeaturesMember = "features";
    public const string BboxMember = "bbox";
    public const string IdMember = "id";

    public static ReadOnlySpan<byte> Type => "type"u8;

    public static ReadOnlySpan<byte> Coordinates => "coordinates"u8;

    public static ReadOnlySpan<byte> Geometries => "geometries"u8;

    public static ReadOnlySpan<byte> Geometry => "geometry"u8;

    public static ReadOnlySpan<byte> Properties => "properties"u8;

    public static ReadOnlySpan<byte> Features => "features"u8;

    public static ReadOnlySpan<byte> Bbox => "bbox"u8;

    public static ReadOnlySpan<byte> Id => "id"u8;

    public static readonly JsonEncodedText TypeText = JsonEncodedText.Encode(TypeMember);

    public static readonly JsonEncodedText CoordinatesText = JsonEncodedText.Encode(CoordinatesMember);

    public static readonly JsonEncodedText GeometriesText = JsonEncodedText.Encode(GeometriesMember);

    public static readonly JsonEncodedText GeometryText = JsonEncodedText.Encode(GeometryMember);

    public static readonly JsonEncodedText PropertiesText = JsonEncodedText.Encode(PropertiesMember);

    public static readonly JsonEncodedText FeaturesText = JsonEncodedText.Encode(FeaturesMember);

    public static readonly JsonEncodedText BboxText = JsonEncodedText.Encode(BboxMember);

    public static readonly JsonEncodedText IdText = JsonEncodedText.Encode(IdMember);
}
