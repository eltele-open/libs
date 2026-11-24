# Eltele.GeoJson

RFC 7946 GeoJSON with C# 15 union types. Serialization needs no `JsonSerializerOptions` setup, no converter
registration, and no source generator.

```csharp
var document = JsonSerializer.Deserialize<GeoJsonObject>(json);   // Geometry, Feature, or FeatureCollection
if (document.Value is Feature<JsonNode> feature)
{
    Console.WriteLine(feature.Geometry);   // null when the Feature is unlocated
}

var point = new Point { Coordinates = new Position(10.7522, 59.9139) };
var text = JsonSerializer.Serialize(point);   // {"type":"Point","coordinates":[10.7522,59.9139]}
```

## Model

| Spec | Type |
| --- | --- |
| §3 GeoJSON object | `union GeoJsonObject(Feature<JsonNode>, FeatureCollection<JsonNode>, Geometry)` |
| §3.1 Geometry object | `union Geometry(Point, MultiPoint, LineString, MultiLineString, Polygon, MultiPolygon, GeometryCollection)` |
| §3.1.1 Position | `Position`: longitude, latitude, optional altitude, further ordinates |
| §3.2 `id` | `union FeatureId(string, double)` |
| §5 `bbox` | `BoundingBox` |

Members carry the spec's names. Unknown members are kept in `ForeignMembers` (§6.1) and written back. Types are
sealed, since §7 fixes the set of GeoJSON types.

## Typed properties

`Feature<TProperties>` and `FeatureCollection<TProperties>` take a payload type for `properties`. This library
writes the GeoJSON envelope; `properties` goes through the active serializer, so the naming policy, converters and
source-generated context of the consuming app apply to it.

```csharp
public sealed record City(string Name, int Population);

var feature = new Feature<City> { Properties = new City("Oslo", 700_000), Geometry = point };
var document = JsonSerializer.Deserialize<GeoJsonObject<City>>(json);
```

`GeoJsonObject` leaves `properties` as `JsonNode`, for documents whose shape is unknown.

## Validation

Parsing is permissive: a document the spec discourages still reads and writes, and the validators report it. Each
validator method is named after the clause it enforces, and each error carries the RFC 2119 severity and section.

```csharp
var errors = new List<ValidationError>();
GeoJsonValidator.FindViolations(document, errors);
GeoJsonValidator.ThrowIfInvalid(document);
```

Per-entity validators append to the same list: `LinearRingValidator.IsClosed`, `FollowsRightHandRule`,
`LineStringValidator.IsCutAtTheAntimeridian`, `BoundingBoxValidator.HasFourOrMoreValues`. A document that
validates appends nothing, and messages are built on demand from a static table.

Two violations have no representation in the model and throw while reading: `coordinates` with fewer than two
ordinates (§3.1.1), and an `id` that is neither a string nor a number (§3.2). A JSON null `id` reads as absent.

### Severity

`Required` is a MUST or MUST NOT, `Recommended` a SHOULD or SHOULD NOT. A ring that winds the wrong way is
`Required` (§3.1.6), and the same clause tells parsers not to reject it, so treat that error as advice for
documents you did not produce.

## Serialization

- Converters read and write `Utf8JsonReader`/`Utf8JsonWriter` directly: no `JsonDocument`, no reflection, and the
  nine `type` literals matched as UTF-8 bytes.
- Writing a geometry allocates nothing; a test asserts it.
- `properties` and `ForeignMembers` use `System.Text.Json.Nodes` from the shared framework, so the package has no
  dependencies.
- Output order is `type`, `bbox`, the type's own members, then foreign members. Reading accepts any order,
  duplicate members, and escaped member names.
- A typed `properties` payload under the reflection resolver is not trim-safe, because `TProperties` is resolved
  at run time. A source-generated `JsonSerializerContext` keeps the envelope reflection-free.

## Out of scope

Validators report §3.1.6 winding, §3.1.8 GeometryCollection ergonomics and §3.1.9 antimeridian cutting; parsing
and writing never enforce them. §4 (WGS 84, longitude then latitude in decimal degrees) is assumed, so a longitude
outside [-180, 180] passes. There is no geometry algebra.
