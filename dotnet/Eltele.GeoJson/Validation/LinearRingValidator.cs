using System.Collections.Immutable;

namespace Eltele.GeoJson.Validation;

/// <summary>Which side of a surface a linear ring bounds.</summary>
public enum RingKind
{
    Exterior,
    Interior,
}

public static class LinearRingValidator
{
    public static bool HasAtLeastFourPositions(in LinearRing ring) => ring.Positions.Length >= 4;

    public static bool IsClosed(in LinearRing ring)
    {
        var positions = ring.Positions;
        return !positions.IsEmpty && positions[0].Equals(positions[positions.Length - 1]);
    }

    /// <remarks>
    /// §3.1.6 also tells parsers not to reject a ring that breaks the rule, so a consumer of foreign data should
    /// read this error as advice rather than as a reason to skip the ring.
    /// </remarks>
    public static bool FollowsRightHandRule(in LinearRing ring, RingKind kind)
    {
        var area = SignedArea(ring.Positions);
        if (area == 0)
        {
            return true;
        }

        return kind == RingKind.Exterior ? area > 0 : area < 0;
    }

    public static void FindViolations(in LinearRing ring, RingKind kind, List<ValidationError> errors)
    {
        if (!HasAtLeastFourPositions(ring))
        {
            errors.Add(new ValidationError(ValidationRule.LinearRingHasFewerThanFourPositions));
        }

        if (!IsClosed(ring))
        {
            errors.Add(new ValidationError(ValidationRule.LinearRingIsNotClosed));
        }

        if (!FollowsRightHandRule(ring, kind))
        {
            errors.Add(new ValidationError(
                kind == RingKind.Exterior
                    ? ValidationRule.ExteriorRingIsClockwise
                    : ValidationRule.InteriorRingIsCounterClockwise));
        }

        if (PositionSequences.CrossesTheAntimeridian(ring.Positions))
        {
            errors.Add(new ValidationError(ValidationRule.GeometryCrossesTheAntimeridian));
        }

        PositionSequences.FindViolations(ring.Positions, errors);
    }

    /// <remarks>Shoelace formula; positive when the ring winds counterclockwise in east/north coordinates.</remarks>
    internal static double SignedArea(ImmutableArray<Position> positions)
    {
        if (positions.Length < 3)
        {
            return 0;
        }

        var sum = 0d;
        for (var index = 0; index < positions.Length; index++)
        {
            var current = positions[index];
            var next = positions[(index + 1) % positions.Length];
            sum += (current.Longitude * next.Latitude) - (next.Longitude * current.Latitude);
        }

        return sum / 2;
    }
}

public static class PolygonValidator
{
    public static void FindViolations(ImmutableArray<LinearRing> rings, List<ValidationError> errors)
    {
        for (var index = 0; index < rings.Length; index++)
        {
            LinearRingValidator.FindViolations(
                rings[index],
                index == 0 ? RingKind.Exterior : RingKind.Interior,
                errors);
        }
    }

    public static void FindViolations(Polygon polygon, List<ValidationError> errors)
    {
        FindViolations(polygon.Coordinates, errors);
        ObjectRules.FindViolations(
            polygon.Bbox,
            CoordinateDimensions.Of(polygon.Coordinates),
            polygon.ForeignMembers,
            ObjectRules.GeometryMembers,
            errors);
    }

    public static void FindViolations(MultiPolygon multiPolygon, List<ValidationError> errors)
    {
        foreach (var rings in multiPolygon.Coordinates)
        {
            FindViolations(rings, errors);
        }

        ObjectRules.FindViolations(
            multiPolygon.Bbox,
            CoordinateDimensions.Of(multiPolygon.Coordinates),
            multiPolygon.ForeignMembers,
            ObjectRules.GeometryMembers,
            errors);
    }
}
