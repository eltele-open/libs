using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Eltele.GeoJson.Serialization;

namespace Eltele.GeoJson;

public static class GeoJsonMediaType
{
    public const string ApplicationGeoJson = "application/geo+json";
}

/// <summary>
/// A GeoJSON object whose <c>properties</c> hold arbitrary JSON, for documents of unknown shape.
/// </summary>
[JsonConverter(typeof(GeoJsonObjectConverter))]
public union GeoJsonObject(Feature<JsonNode>, FeatureCollection<JsonNode>, Geometry);

/// <summary>
/// A GeoJSON object whose <c>properties</c> are read and written by the active serializer as
/// <typeparamref name="TProperties"/>.
/// </summary>
[JsonConverter(typeof(GeoJsonObjectConverter<>))]
public union GeoJsonObject<TProperties>(Feature<TProperties>, FeatureCollection<TProperties>, Geometry)
    where TProperties : class;
