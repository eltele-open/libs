using System.Collections.Immutable;

namespace Eltele.GeoJson.Validation;

public static class LineStringValidator
{
    public static bool HasAtLeastTwoPositions(ImmutableArray<Position> positions) => positions.Length >= 2;

    public static bool IsCutAtTheAntimeridian(ImmutableArray<Position> positions) =>
        !PositionSequences.CrossesTheAntimeridian(positions);

    public static void FindViolations(ImmutableArray<Position> positions, List<ValidationError> errors)
    {
        if (!HasAtLeastTwoPositions(positions))
        {
            errors.Add(new ValidationError(ValidationRule.LineStringHasFewerThanTwoPositions));
        }

        if (!IsCutAtTheAntimeridian(positions))
        {
            errors.Add(new ValidationError(ValidationRule.GeometryCrossesTheAntimeridian));
        }

        PositionSequences.FindViolations(positions, errors);
    }

    public static void FindViolations(LineString lineString, List<ValidationError> errors)
    {
        FindViolations(lineString.Coordinates, errors);
        ObjectRules.FindViolations(
            lineString.Bbox,
            CoordinateDimensions.Of(lineString.Coordinates),
            lineString.ForeignMembers,
            ObjectRules.GeometryMembers,
            errors);
    }
}

public static class MultiLineStringValidator
{
    public static void FindViolations(MultiLineString multiLineString, List<ValidationError> errors)
    {
        foreach (var positions in multiLineString.Coordinates)
        {
            LineStringValidator.FindViolations(positions, errors);
        }

        ObjectRules.FindViolations(
            multiLineString.Bbox,
            CoordinateDimensions.Of(multiLineString.Coordinates),
            multiLineString.ForeignMembers,
            ObjectRules.GeometryMembers,
            errors);
    }
}
