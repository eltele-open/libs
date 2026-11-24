namespace Eltele.GeoJson.Validation;

/// <summary>RFC 2119 requirement level of the clause a rule comes from.</summary>
public enum ValidationSeverity
{
    /// <summary>A MUST or MUST NOT.</summary>
    Required,

    /// <summary>A SHOULD or SHOULD NOT.</summary>
    Recommended,
}

/// <summary>
/// One clause of RFC 7946 that a document can violate. Each member is named after the violation, and the
/// severity follows the RFC 2119 word in the clause.
/// </summary>
public enum ValidationRule
{
    PositionHasMoreThanThreeElements,
    LineStringHasFewerThanTwoPositions,
    LinearRingHasFewerThanFourPositions,
    LinearRingIsNotClosed,
    ExteriorRingIsClockwise,
    InteriorRingIsCounterClockwise,
    GeometryCrossesTheAntimeridian,
    BoundingBoxHasOddNumberOfValues,
    BoundingBoxHasFewerThanFourValues,
    BoundingBoxNorthEastIsNotNorthOfSouthWest,
    BoundingBoxLatitudeIsBeyondThePoles,
    BoundingBoxDimensionsDifferFromCoordinates,
    GeometryCollectionIsNested,
    GeometryCollectionHasOnlyOnePart,
    FeaturePropertiesIsNotAnObject,
    ForeignMemberIsTheTypeMember,
    ForeignMemberIsAMemberOfAnotherGeoJsonType,
}

public readonly record struct ValidationError(ValidationRule Rule, string? Member = null)
{
    public ValidationSeverity Severity => ValidationRules.Severity(Rule);

    public string Section => ValidationRules.Section(Rule);

    public string Message => ValidationRules.Message(Rule, Member);
}

public static class ValidationRules
{
    public static ValidationSeverity Severity(ValidationRule rule) => rule switch
    {
        ValidationRule.PositionHasMoreThanThreeElements => ValidationSeverity.Recommended,
        ValidationRule.LineStringHasFewerThanTwoPositions => ValidationSeverity.Required,
        ValidationRule.LinearRingHasFewerThanFourPositions => ValidationSeverity.Required,
        ValidationRule.LinearRingIsNotClosed => ValidationSeverity.Required,
        ValidationRule.ExteriorRingIsClockwise => ValidationSeverity.Required,
        ValidationRule.InteriorRingIsCounterClockwise => ValidationSeverity.Required,
        ValidationRule.GeometryCrossesTheAntimeridian => ValidationSeverity.Recommended,
        ValidationRule.BoundingBoxHasOddNumberOfValues => ValidationSeverity.Required,
        ValidationRule.BoundingBoxHasFewerThanFourValues => ValidationSeverity.Required,
        ValidationRule.BoundingBoxNorthEastIsNotNorthOfSouthWest => ValidationSeverity.Required,
        ValidationRule.BoundingBoxLatitudeIsBeyondThePoles => ValidationSeverity.Required,
        ValidationRule.BoundingBoxDimensionsDifferFromCoordinates => ValidationSeverity.Required,
        ValidationRule.GeometryCollectionIsNested => ValidationSeverity.Recommended,
        ValidationRule.GeometryCollectionHasOnlyOnePart => ValidationSeverity.Recommended,
        ValidationRule.FeaturePropertiesIsNotAnObject => ValidationSeverity.Required,
        ValidationRule.ForeignMemberIsTheTypeMember => ValidationSeverity.Required,
        ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType => ValidationSeverity.Required,
        _ => throw new ArgumentOutOfRangeException(nameof(rule)),
    };

    public static string Section(ValidationRule rule) => rule switch
    {
        ValidationRule.PositionHasMoreThanThreeElements => "3.1.1",
        ValidationRule.LineStringHasFewerThanTwoPositions => "3.1.4",
        ValidationRule.LinearRingHasFewerThanFourPositions => "3.1.6",
        ValidationRule.LinearRingIsNotClosed => "3.1.6",
        ValidationRule.ExteriorRingIsClockwise => "3.1.6",
        ValidationRule.InteriorRingIsCounterClockwise => "3.1.6",
        ValidationRule.GeometryCrossesTheAntimeridian => "3.1.9",
        ValidationRule.BoundingBoxHasOddNumberOfValues => "5",
        ValidationRule.BoundingBoxHasFewerThanFourValues => "5",
        ValidationRule.BoundingBoxNorthEastIsNotNorthOfSouthWest => "5.2",
        ValidationRule.BoundingBoxLatitudeIsBeyondThePoles => "5.3",
        ValidationRule.BoundingBoxDimensionsDifferFromCoordinates => "5",
        ValidationRule.GeometryCollectionIsNested => "3.1.8",
        ValidationRule.GeometryCollectionHasOnlyOnePart => "3.1.8",
        ValidationRule.FeaturePropertiesIsNotAnObject => "3.2",
        ValidationRule.ForeignMemberIsTheTypeMember => "3",
        ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType => "7.1",
        _ => throw new ArgumentOutOfRangeException(nameof(rule)),
    };

    public static string Message(ValidationRule rule, string? member)
    {
        var text = rule switch
        {
            ValidationRule.PositionHasMoreThanThreeElements =>
                "A position SHOULD NOT extend beyond three elements (§3.1.1).",
            ValidationRule.LineStringHasFewerThanTwoPositions =>
                "A LineString has an array of two or more positions (§3.1.4).",
            ValidationRule.LinearRingHasFewerThanFourPositions =>
                "A linear ring is a closed LineString with four or more positions (§3.1.6).",
            ValidationRule.LinearRingIsNotClosed =>
                "The first and last positions of a linear ring MUST contain identical values (§3.1.6).",
            ValidationRule.ExteriorRingIsClockwise =>
                "An exterior ring MUST follow the right-hand rule, so it is counterclockwise (§3.1.6).",
            ValidationRule.InteriorRingIsCounterClockwise =>
                "A hole MUST follow the right-hand rule, so it is clockwise (§3.1.6).",
            ValidationRule.GeometryCrossesTheAntimeridian =>
                "A geometry that crosses the antimeridian SHOULD be cut in two (§3.1.9).",
            ValidationRule.BoundingBoxHasOddNumberOfValues =>
                "The bbox member MUST be an array of length 2*n (§5).",
            ValidationRule.BoundingBoxHasFewerThanFourValues =>
                "A bbox has at least two axes, so its array has four or more values (§5).",
            ValidationRule.BoundingBoxNorthEastIsNotNorthOfSouthWest =>
                "The latitude of the northeast corner is always greater than the latitude of the southwest corner (§5.2).",
            ValidationRule.BoundingBoxLatitudeIsBeyondThePoles =>
                "Implementers MUST NOT use latitude values beyond 90 or below -90 (§5.3).",
            ValidationRule.BoundingBoxDimensionsDifferFromCoordinates =>
                "n is the number of dimensions represented in the contained geometries (§5).",
            ValidationRule.GeometryCollectionIsNested =>
                "Implementations SHOULD avoid nested GeometryCollections (§3.1.8).",
            ValidationRule.GeometryCollectionHasOnlyOnePart =>
                "Implementations SHOULD avoid a GeometryCollection composed of a single part (§3.1.8).",
            ValidationRule.FeaturePropertiesIsNotAnObject =>
                "The value of the properties member is an object or a JSON null value (§3.2).",
            ValidationRule.ForeignMemberIsTheTypeMember =>
                "The value of the type member MUST be one of the GeoJSON types (§3).",
            ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType =>
                "A GeoJSON object MUST NOT contain a member that another GeoJSON type defines (§7.1).",
                _ => throw new ArgumentOutOfRangeException(nameof(rule)),
        };

        return member is null ? text : $"{text} Offending member: \"{member}\".";
    }
}