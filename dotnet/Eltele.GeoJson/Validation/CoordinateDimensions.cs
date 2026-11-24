using System.Collections.Immutable;

namespace Eltele.GeoJson.Validation;

/// <summary>
/// The n of RFC 7946 §5: how many ordinates the contained positions carry. Mixed arities report the widest
/// position, since that is the dimension a bbox has to describe.
/// </summary>
public static class CoordinateDimensions
{
    public static int? Of(Geometry geometry) => geometry.Value switch
    {
        Point point => point.Coordinates.Count,
        MultiPoint multiPoint => Of(multiPoint.Coordinates),
        LineString lineString => Of(lineString.Coordinates),
        MultiLineString multiLineString => Of(multiLineString.Coordinates),
        Polygon polygon => Of(polygon.Coordinates),
        MultiPolygon multiPolygon => Of(multiPolygon.Coordinates),
        GeometryCollection collection => Of(collection.Geometries),
        _ => null,
    };

    public static int? Of(ImmutableArray<Position> positions)
    {
        int? dimensions = null;
        foreach (var position in positions)
        {
            if (dimensions is null || position.Count > dimensions)
            {
                dimensions = position.Count;
            }
        }

        return dimensions;
    }

    public static int? Of(ImmutableArray<ImmutableArray<Position>> positionArrays)
    {
        int? dimensions = null;
        foreach (var positions in positionArrays)
        {
            dimensions = Widest(dimensions, Of(positions));
        }

        return dimensions;
    }

    public static int? Of(ImmutableArray<LinearRing> rings)
    {
        int? dimensions = null;
        foreach (var ring in rings)
        {
            dimensions = Widest(dimensions, Of(ring.Positions));
        }

        return dimensions;
    }

    public static int? Of(ImmutableArray<ImmutableArray<LinearRing>> ringArrays)
    {
        int? dimensions = null;
        foreach (var rings in ringArrays)
        {
            dimensions = Widest(dimensions, Of(rings));
        }

        return dimensions;
    }

    public static int? Of(ImmutableArray<Geometry> geometries)
    {
        int? dimensions = null;
        foreach (var geometry in geometries)
        {
            dimensions = Widest(dimensions, Of(geometry));
        }

        return dimensions;
    }

    private static int? Widest(int? left, int? right) =>
        left is null ? right : right is null ? left : Math.Max(left.Value, right.Value);
}
