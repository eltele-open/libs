using System.Collections.Immutable;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Eltele.GeoJson.Serialization;

namespace Eltele.GeoJson;

[JsonConverter(typeof(PointConverter))]
public sealed class Point
{
    public string Type => "Point";

    public Position Coordinates { get; init; }

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}

[JsonConverter(typeof(MultiPointConverter))]
public sealed class MultiPoint
{
    public string Type => "MultiPoint";

    public ImmutableArray<Position> Coordinates { get; init; } = [];

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}

[JsonConverter(typeof(LineStringConverter))]
public sealed class LineString
{
    public string Type => "LineString";

    public ImmutableArray<Position> Coordinates { get; init; } = [];

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}

[JsonConverter(typeof(MultiLineStringConverter))]
public sealed class MultiLineString
{
    public string Type => "MultiLineString";

    public ImmutableArray<ImmutableArray<Position>> Coordinates { get; init; } = [];

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}

[JsonConverter(typeof(PolygonConverter))]
public sealed class Polygon
{
    public string Type => "Polygon";

    /// <remarks>The first ring is the exterior ring, the rest are interior rings (RFC 7946 §3.1.6).</remarks>
    public ImmutableArray<LinearRing> Coordinates { get; init; } = [];

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}

[JsonConverter(typeof(MultiPolygonConverter))]
public sealed class MultiPolygon
{
    public string Type => "MultiPolygon";

    public ImmutableArray<ImmutableArray<LinearRing>> Coordinates { get; init; } = [];

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}

[JsonConverter(typeof(GeometryCollectionConverter))]
public sealed class GeometryCollection
{
    public string Type => "GeometryCollection";

    public ImmutableArray<Geometry> Geometries { get; init; } = [];

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}

[JsonConverter(typeof(GeometryConverter))]
public union Geometry(Point, MultiPoint, LineString, MultiLineString, Polygon, MultiPolygon, GeometryCollection);
