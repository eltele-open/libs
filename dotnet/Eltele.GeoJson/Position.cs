using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Eltele.GeoJson.Serialization;

namespace Eltele.GeoJson;

[JsonConverter(typeof(PositionConverter))]
public readonly struct Position : IEquatable<Position>
{
    private readonly ImmutableArray<double> _extraOrdinates;

    public Position(double longitude, double latitude)
        : this(longitude, latitude, null, ImmutableArray<double>.Empty)
    {
    }

    public Position(double longitude, double latitude, double altitude)
        : this(longitude, latitude, altitude, ImmutableArray<double>.Empty)
    {
    }

    public Position(double longitude, double latitude, double? altitude, ImmutableArray<double> extraOrdinates = default)
    {
        Longitude = longitude;
        Latitude = latitude;
        Altitude = altitude;
        _extraOrdinates = extraOrdinates.IsDefault ? ImmutableArray<double>.Empty : extraOrdinates;
    }

    public double Longitude { get; }

    public double Latitude { get; }

    public double? Altitude { get; }

    /// <remarks>
    /// RFC 7946 §3.1.1 says positions SHOULD NOT extend beyond three elements but that a parser MAY ignore
    /// additional ones. Keeping them makes a read/write cycle lossless; <see cref="Validation.PositionValidator"/>
    /// reports them.
    /// </remarks>
    public ImmutableArray<double> ExtraOrdinates => _extraOrdinates.IsDefault ? ImmutableArray<double>.Empty : _extraOrdinates;

    public int Count => 2 + (Altitude.HasValue ? 1 : 0) + ExtraOrdinates.Length;

    public bool Equals(Position other) =>
        Longitude.Equals(other.Longitude)
        && Latitude.Equals(other.Latitude)
        && Nullable.Equals(Altitude, other.Altitude)
        && ExtraOrdinates.AsSpan().SequenceEqual(other.ExtraOrdinates.AsSpan());

    public override bool Equals(object? obj) => obj is Position other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Longitude, Latitude, Altitude, ExtraOrdinates.Length);

    public static bool operator ==(Position left, Position right) => left.Equals(right);

    public static bool operator !=(Position left, Position right) => !left.Equals(right);

    public override string ToString()
    {
        var text = new StringBuilder("[").Append(Longitude.ToString(CultureInfo.InvariantCulture));
        text.Append(", ").Append(Latitude.ToString(CultureInfo.InvariantCulture));
        if (Altitude is { } altitude)
        {
            text.Append(", ").Append(altitude.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var ordinate in ExtraOrdinates)
        {
            text.Append(", ").Append(ordinate.ToString(CultureInfo.InvariantCulture));
        }

        return text.Append(']').ToString();
    }
}
