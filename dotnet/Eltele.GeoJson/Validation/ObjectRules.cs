using System.Text.Json.Nodes;

namespace Eltele.GeoJson.Validation;

internal static class ObjectRules
{
    public static string[] FeatureMembers { get; } =
        [GeoJsonMembers.CoordinatesMember, GeoJsonMembers.GeometriesMember, GeoJsonMembers.FeaturesMember];

    public static string[] FeatureCollectionMembers { get; } =
    [
        GeoJsonMembers.CoordinatesMember,
        GeoJsonMembers.GeometriesMember,
        GeoJsonMembers.GeometryMember,
        GeoJsonMembers.PropertiesMember,
    ];

    public static string[] GeometryMembers { get; } =
        [GeoJsonMembers.GeometryMember, GeoJsonMembers.PropertiesMember, GeoJsonMembers.FeaturesMember];

    public static void FindViolations(
        BoundingBox? bbox,
        int? coordinateDimensions,
        JsonObject? foreignMembers,
        string[] membersOfOtherTypes,
        List<ValidationError> errors)
    {
        if (bbox is { } bounds)
        {
            BoundingBoxValidator.FindViolations(bounds, coordinateDimensions, errors);
        }

        if (foreignMembers is null)
        {
            return;
        }

        foreach (var member in foreignMembers)
        {
            if (string.Equals(member.Key, GeoJsonMembers.TypeMember, StringComparison.Ordinal))
            {
                errors.Add(new ValidationError(ValidationRule.ForeignMemberIsTheTypeMember, member.Key));
                continue;
            }

            foreach (var reserved in membersOfOtherTypes)
            {
                if (string.Equals(member.Key, reserved, StringComparison.Ordinal))
                {
                    errors.Add(new ValidationError(ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType, member.Key));
                    break;
                }
            }
        }
    }
}
