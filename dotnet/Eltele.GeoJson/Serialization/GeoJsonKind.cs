using System.Text;
using System.Text.Json;

namespace Eltele.GeoJson.Serialization;

/// <remarks>
/// Closed rather than an enum, so a switch over the nine types is exhaustive without a fallback arm — §1.4 and §7
/// fix the set. The UTF-8 form of each name is precomputed, so matching a document's "type" allocates nothing.
/// </remarks>
internal closed class GeoJsonKind
{
    private readonly byte[] _utf8Name;

    private protected GeoJsonKind(string name)
    {
        Name = name;
        _utf8Name = Encoding.UTF8.GetBytes(name);
    }

    public static GeoJsonKind Point { get; } = new PointKind();

    public static GeoJsonKind MultiPoint { get; } = new MultiPointKind();

    public static GeoJsonKind LineString { get; } = new LineStringKind();

    public static GeoJsonKind MultiLineString { get; } = new MultiLineStringKind();

    public static GeoJsonKind Polygon { get; } = new PolygonKind();

    public static GeoJsonKind MultiPolygon { get; } = new MultiPolygonKind();

    public static GeoJsonKind GeometryCollection { get; } = new GeometryCollectionKind();

    public static GeoJsonKind Feature { get; } = new FeatureKind();

    public static GeoJsonKind FeatureCollection { get; } = new FeatureCollectionKind();

    public static IReadOnlyList<GeoJsonKind> All { get; } =
    [
        Point,
        MultiPoint,
        LineString,
        MultiLineString,
        Polygon,
        MultiPolygon,
        GeometryCollection,
        Feature,
        FeatureCollection,
    ];

    public string Name { get; }

    public ReadOnlySpan<byte> Utf8Name => _utf8Name;
}

internal sealed class PointKind : GeoJsonKind
{
    public PointKind()
        : base("Point")
    {
    }
}

internal sealed class MultiPointKind : GeoJsonKind
{
    public MultiPointKind()
        : base("MultiPoint")
    {
    }
}

internal sealed class LineStringKind : GeoJsonKind
{
    public LineStringKind()
        : base("LineString")
    {
    }
}

internal sealed class MultiLineStringKind : GeoJsonKind
{
    public MultiLineStringKind()
        : base("MultiLineString")
    {
    }
}

internal sealed class PolygonKind : GeoJsonKind
{
    public PolygonKind()
        : base("Polygon")
    {
    }
}

internal sealed class MultiPolygonKind : GeoJsonKind
{
    public MultiPolygonKind()
        : base("MultiPolygon")
    {
    }
}

internal sealed class GeometryCollectionKind : GeoJsonKind
{
    public GeometryCollectionKind()
        : base("GeometryCollection")
    {
    }
}

internal sealed class FeatureKind : GeoJsonKind
{
    public FeatureKind()
        : base("Feature")
    {
    }
}

internal sealed class FeatureCollectionKind : GeoJsonKind
{
    public FeatureCollectionKind()
        : base("FeatureCollection")
    {
    }
}
