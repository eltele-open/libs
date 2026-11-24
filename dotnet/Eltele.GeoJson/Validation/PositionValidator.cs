using System.Collections.Immutable;

namespace Eltele.GeoJson.Validation;

public static class PositionValidator
{
    public static bool HasNoMoreThanThreeElements(in Position position) => position.ExtraOrdinates.IsEmpty;

    public static void FindViolations(in Position position, List<ValidationError> errors)
    {
        if (!HasNoMoreThanThreeElements(position))
        {
            errors.Add(new ValidationError(ValidationRule.PositionHasMoreThanThreeElements));
        }
    }
}

internal static class PositionSequences
{
    public static bool CrossesTheAntimeridian(ImmutableArray<Position> positions)
    {
        for (var index = 1; index < positions.Length; index++)
        {
            if (Math.Abs(positions[index].Longitude - positions[index - 1].Longitude) > 180)
            {
                return true;
            }
        }

        return false;
    }

    public static void FindViolations(ImmutableArray<Position> positions, List<ValidationError> errors)
    {
        foreach (var position in positions)
        {
            PositionValidator.FindViolations(position, errors);
        }
    }
}
