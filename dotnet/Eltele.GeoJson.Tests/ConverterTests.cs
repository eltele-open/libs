using System.Text.Json;
using System.Text.Json.Nodes;
using Eltele.GeoJson.Validation;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class ConverterTests
{
    private const string PointJson =
        """{"type":"Point","coordinates":[1.0,2.0],"bbox":[0.0,0.0,2.0,2.0],"title":"x"}""";

    private const string MultiPointJson =
        """{"type":"MultiPoint","coordinates":[[1.0,2.0]],"bbox":[0.0,0.0,2.0,2.0],"title":"x"}""";

    private const string LineStringJson =
        """{"type":"LineString","coordinates":[[1.0,2.0],[3.0,4.0]],"bbox":[0.0,0.0,4.0,5.0],"title":"x"}""";

    private const string MultiLineStringJson =
        """{"type":"MultiLineString","coordinates":[[[1.0,2.0],[3.0,4.0]]],"bbox":[0.0,0.0,4.0,5.0],"title":"x"}""";

    private const string PolygonJson =
        """{"type":"Polygon","coordinates":[[[0.0,0.0],[1.0,0.0],[1.0,1.0],[0.0,0.0]]],"bbox":[0.0,0.0,1.0,1.0],"title":"x"}""";

    private const string MultiPolygonJson =
        """{"type":"MultiPolygon","coordinates":[[[[0.0,0.0],[1.0,0.0],[1.0,1.0],[0.0,0.0]]]],"bbox":[0.0,0.0,1.0,1.0],"title":"x"}""";

    private const string GeometryCollectionJson =
        """{"type":"GeometryCollection","geometries":[{"type":"Point","coordinates":[1.0,2.0]}],"bbox":[0.0,0.0,2.0,2.0],"title":"x"}""";

    [TestMethod]
    [DataRow(typeof(Point), PointJson)]
    [DataRow(typeof(MultiPoint), MultiPointJson)]
    [DataRow(typeof(LineString), LineStringJson)]
    [DataRow(typeof(MultiLineString), MultiLineStringJson)]
    [DataRow(typeof(Polygon), PolygonJson)]
    [DataRow(typeof(MultiPolygon), MultiPolygonJson)]
    [DataRow(typeof(GeometryCollection), GeometryCollectionJson)]
    public void RoundTripsEveryGeometryTypeOnItsOwn(Type type, string json)
    {
        var geometry = JsonSerializer.Deserialize(json, type);

        Json.AssertEqual(json, JsonSerializer.Serialize(geometry, type));
    }

    [TestMethod]
    [DataRow(typeof(MultiPoint))]
    [DataRow(typeof(LineString))]
    [DataRow(typeof(MultiLineString))]
    [DataRow(typeof(Polygon))]
    [DataRow(typeof(MultiPolygon))]
    public void RejectsAGeometryWithoutACoordinatesMember(Type type)
    {
        var json = $$"""{"type":"{{type.Name}}"}""";

        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize(json, type));
    }

    [TestMethod]
    public void PreservesForeignMembersOfAFeatureCollection()
    {
        var collection = JsonSerializer.Deserialize<FeatureCollection<JsonNode>>(
            """{"type":"FeatureCollection","features":[],"title":"x"}""")!;

        Assert.AreEqual("x", collection.ForeignMembers!["title"]!.GetValue<string>());
        Json.AssertEqual(
            """{"type":"FeatureCollection","features":[],"title":"x"}""",
            JsonSerializer.Serialize(collection));
    }

    [TestMethod]
    public void RoundTripsATypedFeatureCollectionDocument()
    {
        var json = JsonSerializer.Serialize(new GeoJsonObject<CityProperties>(
            new FeatureCollection<CityProperties>
            {
                Features = [new Feature<CityProperties> { Properties = new CityProperties("Oslo", 700_000) }],
            }));

        var document = JsonSerializer.Deserialize<GeoJsonObject<CityProperties>>(json)!;

        Assert.IsInstanceOfType<FeatureCollection<CityProperties>>(document.Value);
    }

    [TestMethod]
    public void RoundTripsATypedGeometryDocument()
    {
        var document = JsonSerializer.Deserialize<GeoJsonObject<CityProperties>>(
            """{"type":"Point","coordinates":[1.0,2.0]}""")!;

        Assert.IsInstanceOfType<Geometry>(document.Value);
        Json.AssertEqual("""{"type":"Point","coordinates":[1.0,2.0]}""", JsonSerializer.Serialize(document));
    }

    [TestMethod]
    public void RejectsWritingATypedGeoJsonObjectThatHoldsNoValue()
    {
        Assert.ThrowsExactly<JsonException>(() =>
            JsonSerializer.Serialize(default(GeoJsonObject<CityProperties>)));
    }

    [TestMethod]
    public void ValidatesTypedFeatureCollectionAndGeometryDocuments()
    {
        var collection = new GeoJsonObject<CityProperties>(
            new FeatureCollection<CityProperties>
            {
                Features =
                [
                    new Feature<CityProperties>
                    {
                        Geometry = new LineString { Coordinates = [new Position(1, 2)] },
                    },
                ],
            });
        var geometry = new GeoJsonObject<CityProperties>(
            new Geometry(new LineString { Coordinates = [new Position(1, 2)] }));

        var collectionErrors = new List<ValidationError>();
        GeoJsonValidator.FindViolations(collection, collectionErrors);
        var geometryErrors = new List<ValidationError>();
        GeoJsonValidator.FindViolations(geometry, geometryErrors);

        Assert.IsFalse(GeoJsonValidator.IsValid(collection));
        Assert.IsFalse(GeoJsonValidator.IsValid(geometry));
        Assert.AreEqual(1, collectionErrors.Count);
        Assert.AreEqual(ValidationRule.LineStringHasFewerThanTwoPositions, geometryErrors[0].Rule);
    }
}
