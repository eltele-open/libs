using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Eltele.GeoJson.Serialization;

namespace Eltele.GeoJson;

/// <summary>The <c>id</c> of a Feature: either a JSON string or a number (RFC 7946 §3.2).</summary>
[JsonConverter(typeof(FeatureIdConverter))]
public union FeatureId(string, double);

[JsonConverter(typeof(FeatureConverter<>))]
public sealed class Feature<TProperties>
    where TProperties : class
{
    public string Type => "Feature";

    public FeatureId? Id { get; init; }

    /// <remarks>Null for an unlocated Feature; the member itself is always written.</remarks>
    public Geometry? Geometry { get; init; }

    /// <remarks>Null means a JSON null value, which §3.2 allows alongside an object.</remarks>
    public TProperties? Properties { get; init; }

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}
