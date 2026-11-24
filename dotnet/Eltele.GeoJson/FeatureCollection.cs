using System.Collections.Immutable;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Eltele.GeoJson.Serialization;

namespace Eltele.GeoJson;

[JsonConverter(typeof(FeatureCollectionConverter<>))]
public sealed class FeatureCollection<TProperties>
    where TProperties : class
{
    public string Type => "FeatureCollection";

    /// <remarks>May be empty (RFC 7946 §3.3); ordered, not a set.</remarks>
    public ImmutableArray<Feature<TProperties>> Features { get; init; } = [];

    public BoundingBox? Bbox { get; init; }

    public JsonObject? ForeignMembers { get; init; }
}
