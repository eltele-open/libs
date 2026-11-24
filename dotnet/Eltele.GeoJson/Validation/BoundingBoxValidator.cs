namespace Eltele.GeoJson.Validation;

public static class BoundingBoxValidator
{
    public static bool HasEvenNumberOfValues(in BoundingBox boundingBox) => boundingBox.Length % 2 == 0;

    public static bool HasFourOrMoreValues(in BoundingBox boundingBox) => boundingBox.Length >= 4;

    public static bool NorthEastIsNorthOfSouthWest(in BoundingBox boundingBox)
    {
        var south = boundingBox.Values[1];
        var north = boundingBox.Values[boundingBox.Dimensions + 1];
        return north >= south;
    }

    public static bool LatitudesAreWithinThePoles(in BoundingBox boundingBox)
    {
        var south = boundingBox.Values[1];
        var north = boundingBox.Values[boundingBox.Dimensions + 1];
        return south is >= -90 and <= 90 && north is >= -90 and <= 90;
    }

    public static bool MatchesCoordinateDimensions(in BoundingBox boundingBox, int dimensions) =>
        boundingBox.Dimensions == dimensions;

    public static void FindViolations(
        in BoundingBox boundingBox,
        int? coordinateDimensions,
        List<ValidationError> errors)
    {
        var even = HasEvenNumberOfValues(boundingBox);
        if (!even)
        {
            errors.Add(new ValidationError(ValidationRule.BoundingBoxHasOddNumberOfValues));
        }

        var atLeastTwoDimensions = HasFourOrMoreValues(boundingBox);
        if (!atLeastTwoDimensions)
        {
            errors.Add(new ValidationError(ValidationRule.BoundingBoxHasFewerThanFourValues));
        }

        if (even && atLeastTwoDimensions)
        {
            if (!NorthEastIsNorthOfSouthWest(boundingBox))
            {
                errors.Add(new ValidationError(ValidationRule.BoundingBoxNorthEastIsNotNorthOfSouthWest));
            }

            if (!LatitudesAreWithinThePoles(boundingBox))
            {
                errors.Add(new ValidationError(ValidationRule.BoundingBoxLatitudeIsBeyondThePoles));
            }
        }

        if (even && coordinateDimensions is { } dimensions && !MatchesCoordinateDimensions(boundingBox, dimensions))
        {
            errors.Add(new ValidationError(ValidationRule.BoundingBoxDimensionsDifferFromCoordinates));
        }
    }
}
