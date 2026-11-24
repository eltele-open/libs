using System.Text.Json;
using System.Text.Json.Nodes;
using Eltele.GeoJson.Validation;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class TypedPropertiesTests
{
    [TestMethod]
    public void RoundTripsAFeatureWhosePropertiesAreATypedPayload()
    {
        var feature = new Feature<CityProperties>
        {
            Id = new FeatureId("oslo"),
            Geometry = new Point { Coordinates = new Position(10.7522, 59.9139) },
            Properties = new CityProperties("Oslo", 700_000),
        };

        var json = JsonSerializer.Serialize(feature);
        Json.AssertEqual(
            """
            {
                "type": "Feature",
                "id": "oslo",
                "geometry": {"type": "Point", "coordinates": [10.7522, 59.9139]},
                "properties": {"Name": "Oslo", "Population": 700000}
            }
            """,
            json);

        var roundTripped = JsonSerializer.Deserialize<Feature<CityProperties>>(json)!;
        Assert.AreEqual(new CityProperties("Oslo", 700_000), roundTripped.Properties);
    }

    [TestMethod]
    public void RoundTripsATypedGeoJsonObject()
    {
        var document = new GeoJsonObject<CityProperties>(
            new Feature<CityProperties>
            {
                Geometry = new Point { Coordinates = new Position(10.7522, 59.9139) },
                Properties = new CityProperties("Oslo", 700_000),
            });

        var json = JsonSerializer.Serialize(document);
        var roundTripped = JsonSerializer.Deserialize<GeoJsonObject<CityProperties>>(json)!;

        Assert.IsInstanceOfType<Feature<CityProperties>>(roundTripped.Value);
        Json.AssertEqual(json, JsonSerializer.Serialize(roundTripped));
    }

    [TestMethod]
    public void WritesTheTypedPayloadUnderTheActiveSerializersConventions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var feature = new Feature<CityProperties>
        {
            Geometry = new Point { Coordinates = new Position(10.7522, 59.9139) },
            Properties = new CityProperties("Oslo", 700_000),
        };

        var json = JsonSerializer.Serialize(feature, options);

        StringAssert.Contains(json, "\"properties\":{\"name\":\"Oslo\",\"population\":700000}");
        StringAssert.Contains(json, "\"type\":\"Feature\"");
    }

    [TestMethod]
    public void ReadsTypedPropertiesAsANullMember()
    {
        var feature = JsonSerializer.Deserialize<Feature<CityProperties>>(
            """{"type":"Feature","geometry":null,"properties":null}""")!;

        Assert.IsNull(feature.Properties);
    }

    [TestMethod]
    public void WritesATypedFeatureCollectionDocument()
    {
        var document = new GeoJsonObject<CityProperties>(
            new FeatureCollection<CityProperties>
            {
                Features = [new Feature<CityProperties> { Properties = new CityProperties("Oslo", 700_000) }],
            });

        var json = JsonSerializer.Serialize(document);

        StringAssert.Contains(json, "\"type\":\"FeatureCollection\"");
        Json.AssertEqual(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<GeoJsonObject<CityProperties>>(json)));
    }

    [TestMethod]
    public void WritesATypedGeometryDocument()
    {
        var document = new GeoJsonObject<CityProperties>(
            new Geometry(new Point { Coordinates = new Position(10.7522, 59.9139) }));

        Json.AssertEqual(
            """{"type":"Point","coordinates":[10.7522,59.9139]}""",
            JsonSerializer.Serialize(document));
    }

    [TestMethod]
    public void ValidatesATypedDocument()
    {
        var valid = new GeoJsonObject<CityProperties>(
            new Feature<CityProperties>
            {
                Geometry = new Point { Coordinates = new Position(10.7522, 59.9139) },
                Properties = new CityProperties("Oslo", 700_000),
            });
        var invalid = new GeoJsonObject<CityProperties>(
            new Feature<CityProperties>
            {
                Geometry = new LineString { Coordinates = [new Position(10.7522, 59.9139)] },
            });

        Assert.IsTrue(GeoJsonValidator.IsValid(valid));

        var errors = new List<ValidationError>();
        GeoJsonValidator.FindViolations(invalid, errors);
        Assert.IsTrue(errors.Any(error => error.Rule == ValidationRule.LineStringHasFewerThanTwoPositions));

        Assert.ThrowsExactly<GeoJsonValidationException>(() => GeoJsonValidator.ThrowIfInvalid(invalid));
        GeoJsonValidator.ThrowIfInvalid(valid);
    }

    [TestMethod]
    public void ReadsUntypedPropertiesAsJsonNodes()
    {
        var feature = JsonSerializer.Deserialize<Feature<JsonNode>>(
            """{"type":"Feature","geometry":null,"properties":{"a":1}}""")!;

        Assert.AreEqual(1, feature.Properties!["a"]!.GetValue<int>());
    }
}
