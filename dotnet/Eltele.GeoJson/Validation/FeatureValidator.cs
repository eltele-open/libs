using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eltele.GeoJson.Validation;

public static class FeatureValidator
{
    public static bool PropertiesIsAnObjectOrNull<TProperties>(Feature<TProperties> feature)
        where TProperties : class =>
        feature.Properties is not JsonNode node || node is JsonObject;

    public static void FindViolations<TProperties>(Feature<TProperties> feature, List<ValidationError> errors)
        where TProperties : class
    {
        if (!PropertiesIsAnObjectOrNull(feature))
        {
            errors.Add(new ValidationError(ValidationRule.FeaturePropertiesIsNotAnObject));
        }

        var dimensions = feature.Geometry is { } geometry ? CoordinateDimensions.Of(geometry) : null;
        ObjectRules.FindViolations(
            feature.Bbox,
            dimensions,
            feature.ForeignMembers,
            ObjectRules.FeatureMembers,
            errors);

        if (feature.Geometry is { } value)
        {
            GeometryValidator.FindViolations(value, errors);
        }
    }
}

public static class FeatureCollectionValidator
{
    public static void FindViolations<TProperties>(
        FeatureCollection<TProperties> collection,
        List<ValidationError> errors)
        where TProperties : class
    {
        int? dimensions = null;
        foreach (var feature in collection.Features)
        {
            if (feature.Geometry is { } geometry && CoordinateDimensions.Of(geometry) is { } value)
            {
                dimensions = dimensions is null ? value : Math.Max(dimensions.Value, value);
            }
        }

        ObjectRules.FindViolations(
            collection.Bbox,
            dimensions,
            collection.ForeignMembers,
            ObjectRules.FeatureCollectionMembers,
            errors);

        foreach (var feature in collection.Features)
        {
            FeatureValidator.FindViolations(feature, errors);
        }
    }
}

public static class GeoJsonValidator
{
    public static bool IsValid(GeoJsonObject document) => IsValid(document, out _);

    public static bool IsValid<TProperties>(GeoJsonObject<TProperties> document)
        where TProperties : class => IsValid(document, out _);

    public static void FindViolations(GeoJsonObject document, List<ValidationError> errors)
    {
        switch (document.Value)
        {
            case Feature<JsonNode> feature:
                FeatureValidator.FindViolations(feature, errors);
                break;
            case FeatureCollection<JsonNode> collection:
                FeatureCollectionValidator.FindViolations(collection, errors);
                break;
            case Geometry geometry:
                GeometryValidator.FindViolations(geometry, errors);
                break;
            default:
                throw new ArgumentException("The GeoJSON object holds no value.", nameof(document));
        }
    }

    public static void FindViolations<TProperties>(
        GeoJsonObject<TProperties> document,
        List<ValidationError> errors)
        where TProperties : class
    {
        switch (document.Value)
        {
            case Feature<TProperties> feature:
                FeatureValidator.FindViolations(feature, errors);
                break;
            case FeatureCollection<TProperties> collection:
                FeatureCollectionValidator.FindViolations(collection, errors);
                break;
            case Geometry geometry:
                GeometryValidator.FindViolations(geometry, errors);
                break;
            default:
                throw new ArgumentException("The GeoJSON object holds no value.", nameof(document));
        }
    }

    public static void ThrowIfInvalid(GeoJsonObject document)
    {
        if (!IsValid(document, out var errors))
        {
            throw new GeoJsonValidationException(errors);
        }
    }

    public static void ThrowIfInvalid<TProperties>(GeoJsonObject<TProperties> document)
        where TProperties : class
    {
        if (!IsValid(document, out var errors))
        {
            throw new GeoJsonValidationException(errors);
        }
    }

    private static bool IsValid(GeoJsonObject document, out List<ValidationError> errors)
    {
        errors = [];
        FindViolations(document, errors);
        return errors.Count == 0;
    }

    private static bool IsValid<TProperties>(GeoJsonObject<TProperties> document, out List<ValidationError> errors)
        where TProperties : class
    {
        errors = [];
        FindViolations(document, errors);
        return errors.Count == 0;
    }
}

public sealed class GeoJsonValidationException : JsonException
{
    public GeoJsonValidationException(IReadOnlyList<ValidationError> errors)
        : base(Describe(errors)) => Errors = errors;

    public IReadOnlyList<ValidationError> Errors { get; }

    private static string Describe(IReadOnlyList<ValidationError> errors) => errors.Count == 1
        ? $"The GeoJSON document violates RFC 7946: {errors[0].Message}"
        : $"The GeoJSON document violates {errors.Count} RFC 7946 clauses; first: {errors[0].Message}";
}
