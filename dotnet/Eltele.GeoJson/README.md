# Eltele.GeoJson

RFC 7946 GeoJSON, modelled with C# 15 union types. Serialization works with a stock `System.Text.Json` — no
`JsonSerializerOptions` setup, no source generator, no converter registration.

```csharp
var document = JsonSerializer.Deserialize<GeoJsonObject>(json);   // Geometry, Feature, or FeatureCollection
if (document.Value is Feature<JsonNode> feature)
{
    Console.WriteLine(feature.Geometry);   // null for an unlocated Feature
}

var point = new Point { Coordinates = new Position(10.7522, 59.9139) };
var text = JsonSerializer.Serialize(point);   // {"type":"Point","coordinates":[10.7522,59.9139]}
```

## The model follows the spec's structure

| Spec | Type |
| --- | --- |
| §3 GeoJSON object | `union GeoJsonObject(Feature<JsonNode>, FeatureCollection<JsonNode>, Geometry)` |
| §3.1 Geometry object | `union Geometry(Point, MultiPoint, LineString, MultiLineString, Polygon, MultiPolygon, GeometryCollection)` |
| §3.1.1 Position | `Position` — longitude, latitude, optional altitude, and any further ordinates |
| §3.2 `id` | `union FeatureId(string, double)` |
| §5 `bbox` | `BoundingBox` |

Types are sealed and not inheritable, because §7 says the set of GeoJSON types is not extensible. Every member the
spec defines has a property named after it, and every unknown member is kept in `ForeignMembers` (§6.1) so a
read/write cycle does not lose data.

## Typed properties

`Feature<TProperties>` and `FeatureCollection<TProperties>` let the payload be your own type. The GeoJSON envelope
is still written by this library; `properties` is handed to whichever serializer is active, so your naming policy,
converters, and source-generated context apply to it as they would to a bare DTO.

```csharp
public sealed record City(string Name, int Population);

var feature = new Feature<City> { Properties = new City("Oslo", 700_000), Geometry = point };
var typed = JsonSerializer.Deserialize<GeoJsonObject<City>>(json);
```

`GeoJsonObject` is the same document with `properties` left as `JsonNode`, which is what you want when the shape is
not known in advance.

## Validation

Parsing is permissive: a document that the spec discourages still reads and writes, and the validators report what
is wrong. Their methods are named after the clause they enforce, and every error carries its RFC 2119 severity and
section number.

```csharp
var errors = new List<ValidationError>();
GeoJsonValidator.FindViolations(document, errors);
// or, when a valid document is a precondition:
GeoJsonValidator.ThrowIfInvalid(document);
```

Finer-grained validators exist per entity and per clause: `LinearRingValidator.IsClosed`, `FollowsRightHandRule`,
`BoundingBoxValidator.HasFourOrMoreValues`, `LineStringValidator.IsCutAtTheAntimeridian`, and so on. Messages are
built on demand from a static table, and a document that validates appends nothing, so the passing path does not
allocate.

The one rule a document cannot carry is a buffer of `coordinates` with fewer than two ordinates (§3.1.1): there is
no `Position` to build, so reading throws instead of reporting.

### Severity

`ValidationSeverity.Required` is a MUST or MUST NOT; `Recommended` is a SHOULD or SHOULD NOT. A ring that winds the
wrong way is `Required` (§3.1.6), yet the same clause tells parsers not to reject it — treat the error as advice
when the document came from somewhere you do not control.

## Serialization notes

- Converters are hand-written over `Utf8JsonReader`/`Utf8JsonWriter`. The read path does not build a
  `JsonDocument`, does not reflect, and matches the nine `type` literals against UTF-8 bytes.
- Writing a geometry allocates nothing; a test in this repository asserts that.
- `properties` and `ForeignMembers` use `System.Text.Json.Nodes`, which is part of the shared framework. The
  package has no dependencies and ships nothing but this library.
- Member order is fixed (`type`, `bbox`, then the type's own members, then foreign members). Reading accepts any
  order, duplicate members, and escaped member names.
- A typed payload on the reflection-based resolver is not trimming-safe by construction, because `TProperties` is
  resolved at runtime. With a source-generated `JsonSerializerContext` the envelope is already reflection-free.

## Not modelled

The RFC's prose about antimeridian cutting (§3.1.9), right-hand rule (§3.1.6), and GeometryCollection ergonomics
(§3.1.8) is reported by validators but never enforced by parsing or writing. §4 (WGS 84, longitude then latitude in
decimal degrees) is an assumption of the model rather than a check: nothing forbids a longitude outside
[-180, 180]. There is no geometry algebra: no area, distance, or intersection. `Eltele.GeoJson` is the format, not a
spatial engine.
