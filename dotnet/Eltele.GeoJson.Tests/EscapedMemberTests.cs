using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class EscapedMemberTests
{
    [TestMethod]
    public void ReadsACoordinatesMemberWithEscapes()
    {
        var point = JsonSerializer.Deserialize<Point>(
            """{"typ\u0065":"Point","coordinat\u0065s":[1.0,2.0],"bb\u006fx":[0.0,0.0,2.0,2.0],"titl\u0065":"x"}""")!;

        Assert.AreEqual(new Position(1, 2), point.Coordinates);
        Assert.AreEqual(new BoundingBox(0, 0, 2, 2), point.Bbox);
        Assert.AreEqual("x", point.ForeignMembers!["title"]!.GetValue<string>());
    }

    [TestMethod]
    [DataRow("""{"type":"MultiPoint","coordinat\u0065s":[[1.0,2.0]]}""", "MultiPoint")]
    [DataRow("""{"type":"LineString","coordinat\u0065s":[[1.0,2.0],[3.0,4.0]]}""", "LineString")]
    [DataRow("""{"type":"MultiLineString","coordinat\u0065s":[[[1.0,2.0],[3.0,4.0]]]}""", "MultiLineString")]
    [DataRow("""{"type":"Polygon","coordinat\u0065s":[[[0.0,0.0],[1.0,0.0],[1.0,1.0],[0.0,0.0]]]}""", "Polygon")]
    [DataRow("""{"type":"MultiPolygon","coordinat\u0065s":[[[[0.0,0.0],[1.0,0.0],[1.0,1.0],[0.0,0.0]]]]}""", "MultiPolygon")]
    public void ReadsACoordinatesMemberWithEscapesOnEveryGeometry(string json, string expectedType)
    {
        var geometry = JsonSerializer.Deserialize<Geometry>(json);

        Assert.AreEqual(expectedType, geometry.Value!.GetType().Name);
    }

    [TestMethod]
    public void ReadsAGeometriesMemberWithEscapes()
    {
        var collection = JsonSerializer.Deserialize<GeometryCollection>(
            """{"type":"GeometryCollection","geometrie\u0073":[{"type":"Point","coordinates":[1.0,2.0]}]}""")!;

        Assert.AreEqual(1, collection.Geometries.Length);
    }

    [TestMethod]
    public void ReadsEscapedMembersOfAFeature()
    {
        var feature = JsonSerializer.Deserialize<Feature<JsonNode>>(
            """{"type":"Feature","i\u0064":"f1","geometr\u0079":null,"propertie\u0073":null,"bb\u006fx":[1.0,2.0,3.0,4.0]}""")!;

        Assert.AreEqual(new FeatureId("f1"), feature.Id);
        Assert.IsNull(feature.Geometry);
        Assert.IsNull(feature.Properties);
        Assert.AreEqual(new BoundingBox(1, 2, 3, 4), feature.Bbox);
    }

    [TestMethod]
    public void ReadsEscapedMembersOfAFeatureCollection()
    {
        var collection = JsonSerializer.Deserialize<FeatureCollection<JsonNode>>(
            """{"type":"FeatureCollection","feature\u0073":[],"bb\u006fx":[1.0,2.0,3.0,4.0]}""")!;

        Assert.IsTrue(collection.Features.IsEmpty);
        Assert.AreEqual(new BoundingBox(1, 2, 3, 4), collection.Bbox);
    }

    [TestMethod]
    public void RejectsAnEscapedTypeThatNamesAnotherGeometry()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Point>(
            """{"typ\u0065":"LineString","coordinates":[[1.0,2.0],[3.0,4.0]]}"""));
    }
}
