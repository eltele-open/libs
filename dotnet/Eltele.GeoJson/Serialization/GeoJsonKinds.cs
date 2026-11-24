using System.Text.Json;

namespace Eltele.GeoJson.Serialization;

internal enum GeoJsonKind
{
    Point,
    MultiPoint,
    LineString,
    MultiLineString,
    Polygon,
    MultiPolygon,
    GeometryCollection,
    Feature,
    FeatureCollection,
}

internal static class GeoJsonKinds
{
    public static readonly GeoJsonKind[] All = Enum.GetValues<GeoJsonKind>();

    public static string Name(GeoJsonKind kind) => kind switch
    {
        GeoJsonKind.Point => "Point",
        GeoJsonKind.MultiPoint => "MultiPoint",
        GeoJsonKind.LineString => "LineString",
        GeoJsonKind.MultiLineString => "MultiLineString",
        GeoJsonKind.Polygon => "Polygon",
        GeoJsonKind.MultiPolygon => "MultiPolygon",
        GeoJsonKind.GeometryCollection => "GeometryCollection",
        GeoJsonKind.Feature => "Feature",
        GeoJsonKind.FeatureCollection => "FeatureCollection",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <remarks>
    /// ValueTextEquals answers false for a string that carries escape sequences, so a caller that does not match
    /// any kind this way must fall back to comparing <see cref="Utf8JsonReader.GetString"/>.
    /// </remarks>
    public static bool Matches(ref Utf8JsonReader reader, GeoJsonKind kind) => kind switch
    {
        GeoJsonKind.Point => reader.ValueTextEquals("Point"u8),
        GeoJsonKind.MultiPoint => reader.ValueTextEquals("MultiPoint"u8),
        GeoJsonKind.LineString => reader.ValueTextEquals("LineString"u8),
        GeoJsonKind.MultiLineString => reader.ValueTextEquals("MultiLineString"u8),
        GeoJsonKind.Polygon => reader.ValueTextEquals("Polygon"u8),
        GeoJsonKind.MultiPolygon => reader.ValueTextEquals("MultiPolygon"u8),
        GeoJsonKind.GeometryCollection => reader.ValueTextEquals("GeometryCollection"u8),
        GeoJsonKind.Feature => reader.ValueTextEquals("Feature"u8),
        GeoJsonKind.FeatureCollection => reader.ValueTextEquals("FeatureCollection"u8),
        _ => false,
    };
}
