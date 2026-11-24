using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Eltele.GeoJson.Serialization;

namespace Eltele.GeoJson;

/// <summary>
/// The <c>bbox</c> member: 2*n ordinates, all axes of the south-westerly corner followed by all axes of the
/// north-easterly corner (RFC 7946 §5).
/// </summary>
[JsonConverter(typeof(BoundingBoxConverter))]
public readonly struct BoundingBox : IEquatable<BoundingBox>
{
    private readonly ImmutableArray<double> _values;

    public BoundingBox(params ImmutableArray<double> values) =>
        _values = values.IsDefault ? ImmutableArray<double>.Empty : values;

    public ImmutableArray<double> Values => _values.IsDefault ? ImmutableArray<double>.Empty : _values;

    public int Length => Values.Length;

    public int Dimensions => Values.Length / 2;

    public static implicit operator BoundingBox(double[] values) => new(ImmutableArray.Create(values));

    public bool Equals(BoundingBox other) => Values.AsSpan().SequenceEqual(other.Values.AsSpan());

    public override bool Equals(object? obj) => obj is BoundingBox other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in Values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(BoundingBox left, BoundingBox right) => left.Equals(right);

    public static bool operator !=(BoundingBox left, BoundingBox right) => !left.Equals(right);
}
