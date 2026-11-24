using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Eltele.GeoJson.Serialization;

namespace Eltele.GeoJson;

/// <summary>
/// A linear ring (RFC 7946 §3.1.6): a closed LineString with four or more positions, the first and last
/// identical. Validation of closure and winding is left to <see cref="Validation.LinearRingValidator"/> so a
/// document read from a non-conforming producer stays readable.
/// </summary>
[JsonConverter(typeof(LinearRingConverter))]
public readonly struct LinearRing : IEquatable<LinearRing>
{
    private readonly ImmutableArray<Position> _positions;

    public LinearRing(ImmutableArray<Position> positions) =>
        _positions = positions.IsDefault ? ImmutableArray<Position>.Empty : positions;

    public LinearRing(params Position[] positions) => _positions = ImmutableArray.Create(positions);

    public ImmutableArray<Position> Positions => _positions.IsDefault ? ImmutableArray<Position>.Empty : _positions;

    public static implicit operator LinearRing(Position[] positions) => new(positions);

    public static implicit operator LinearRing(ImmutableArray<Position> positions) => new(positions);

    public bool Equals(LinearRing other) => Positions.AsSpan().SequenceEqual(other.Positions.AsSpan());

    public override bool Equals(object? obj) => obj is LinearRing other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var position in Positions)
        {
            hash.Add(position);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(LinearRing left, LinearRing right) => left.Equals(right);

    public static bool operator !=(LinearRing left, LinearRing right) => !left.Equals(right);
}
