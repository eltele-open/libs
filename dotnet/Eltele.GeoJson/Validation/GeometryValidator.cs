using System.Collections.Immutable;

namespace Eltele.GeoJson.Validation;

public static class PointValidator
{
    public static void FindViolations(Point point, List<ValidationError> errors)
    {
        PositionValidator.FindViolations(point.Coordinates, errors);
        ObjectRules.FindViolations(
            point.Bbox,
            point.Coordinates.Count,
            point.ForeignMembers,
            ObjectRules.GeometryMembers,
            errors);
    }
}

public static class MultiPointValidator
{
    public static void FindViolations(MultiPoint multiPoint, List<ValidationError> errors)
    {
        PositionSequences.FindViolations(multiPoint.Coordinates, errors);
        ObjectRules.FindViolations(
            multiPoint.Bbox,
            CoordinateDimensions.Of(multiPoint.Coordinates),
            multiPoint.ForeignMembers,
            ObjectRules.GeometryMembers,
            errors);
    }
}

public static class GeometryCollectionValidator
{
    public static bool AvoidsNesting(ImmutableArray<Geometry> geometries)
    {
        foreach (var geometry in geometries)
        {
            if (geometry.Value is GeometryCollection)
            {
                return false;
            }
        }

        return true;
    }

    public static bool AvoidsASinglePart(ImmutableArray<Geometry> geometries) => geometries.Length != 1;

    public static void FindViolations(GeometryCollection collection, List<ValidationError> errors)
    {
        if (!AvoidsNesting(collection.Geometries))
        {
            errors.Add(new ValidationError(ValidationRule.GeometryCollectionIsNested));
        }

        if (!AvoidsASinglePart(collection.Geometries))
        {
            errors.Add(new ValidationError(ValidationRule.GeometryCollectionHasOnlyOnePart));
        }

        foreach (var geometry in collection.Geometries)
        {
            GeometryValidator.FindViolations(geometry, errors);
        }

        ObjectRules.FindViolations(
            collection.Bbox,
            CoordinateDimensions.Of(collection.Geometries),
            collection.ForeignMembers,
            ObjectRules.GeometryMembers,
            errors);
    }
}

public static class GeometryValidator
{
    public static void FindViolations(Geometry geometry, List<ValidationError> errors)
    {
        switch (geometry.Value)
        {
            case Point point:
                PointValidator.FindViolations(point, errors);
                break;
            case MultiPoint multiPoint:
                MultiPointValidator.FindViolations(multiPoint, errors);
                break;
            case LineString lineString:
                LineStringValidator.FindViolations(lineString, errors);
                break;
            case MultiLineString multiLineString:
                MultiLineStringValidator.FindViolations(multiLineString, errors);
                break;
            case Polygon polygon:
                PolygonValidator.FindViolations(polygon, errors);
                break;
            case MultiPolygon multiPolygon:
                PolygonValidator.FindViolations(multiPolygon, errors);
                break;
            case GeometryCollection collection:
                GeometryCollectionValidator.FindViolations(collection, errors);
                break;
            default:
                throw new ArgumentException("The Geometry holds no value.", nameof(geometry));
        }
    }
}
