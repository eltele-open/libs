using System.Buffers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Eltele.GeoJson.Validation;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class SerializationTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [TestMethod]
    public void RoundTripsTheFeatureCollectionOfSection1_5()
    {
        var document = JsonSerializer.Deserialize<GeoJsonObject>(RfcExamples.FeatureCollection);

        Assert.IsInstanceOfType<FeatureCollection<JsonNode>>(document.Value);
        Assert.IsTrue(GeoJsonValidator.IsValid(document));
        Json.AssertEqual(RfcExamples.FeatureCollection, JsonSerializer.Serialize(document));
    }

    [TestMethod]
    [DataRow(nameof(RfcExamples.Point))]
    [DataRow(nameof(RfcExamples.LineString))]
    [DataRow(nameof(RfcExamples.Polygon))]
    [DataRow(nameof(RfcExamples.PolygonWithHoles))]
    [DataRow(nameof(RfcExamples.MultiPoint))]
    [DataRow(nameof(RfcExamples.MultiLineString))]
    [DataRow(nameof(RfcExamples.MultiPolygon))]
    [DataRow(nameof(RfcExamples.GeometryCollection))]
    public void RoundTripsEveryGeometryExampleOfAppendixA(string example)
    {
        var json = (string)typeof(RfcExamples).GetField(example)!.GetValue(null)!;

        var document = JsonSerializer.Deserialize<GeoJsonObject>(json);

        Assert.IsInstanceOfType<Geometry>(document.Value);
        Assert.IsTrue(GeoJsonValidator.IsValid(document));
        Json.AssertEqual(json, JsonSerializer.Serialize(document));
    }

    [TestMethod]
    public void RoundTripsAFeatureWithABoundingBox()
    {
        var document = JsonSerializer.Deserialize<GeoJsonObject>(RfcExamples.FeatureWithBoundingBox);

        var feature = (Feature<JsonNode>)document.Value!;
        Assert.AreEqual(new BoundingBox(-10.0, -10.0, 10.0, 10.0), feature.Bbox);
        Assert.IsTrue(GeoJsonValidator.IsValid(document));
        Json.AssertEqual(RfcExamples.FeatureWithBoundingBox, JsonSerializer.Serialize(document));
    }

    [TestMethod]
    public void WritesTheSameJsonUnderDefaultAndWebOptions()
    {
        var withDefaults = JsonSerializer.Deserialize<GeoJsonObject>(RfcExamples.FeatureCollection);
        var withWeb = JsonSerializer.Deserialize<GeoJsonObject>(RfcExamples.FeatureCollection, Web);

        Json.AssertEqual(JsonSerializer.Serialize(withDefaults), JsonSerializer.Serialize(withWeb, Web));
    }

    [TestMethod]
    public void WritesTheTypeMemberFirst()
    {
        var json = JsonSerializer.Serialize(new Point { Coordinates = new Position(1, 2) });

        Assert.StartsWith("{\"type\":\"Point\"", json);
    }

    [TestMethod]
    public void PreservesForeignMembers()
    {
        const string json = """
            {"type":"Feature","id":"f1","geometry":null,"properties":null,"title":"Example Feature","n":null}
            """;

        var feature = JsonSerializer.Deserialize<Feature<JsonNode>>(json)!;

        Assert.AreEqual("Example Feature", feature.ForeignMembers!["title"]!.GetValue<string>());
        Assert.IsTrue(feature.ForeignMembers.ContainsKey("n"));
        Json.AssertEqual(json, JsonSerializer.Serialize(feature));
    }

    [TestMethod]
    public void ReadsMembersInAnyOrder()
    {
        var geometry = JsonSerializer.Deserialize<Geometry>("""{"coordinates":[1.5,2.5],"type":"Point"}""");

        var point = (Point)geometry.Value!;
        Assert.AreEqual(new Position(1.5, 2.5), point.Coordinates);
    }

    [TestMethod]
    public void ReadsAnEscapedTypeMember()
    {
        var geometry = JsonSerializer.Deserialize<Geometry>("""{"typ\u0065":"Point","coordinates":[1.0,2.0]}""");

        Assert.IsInstanceOfType<Point>(geometry.Value);
    }

    [TestMethod]
    public void ReadsAPositionWithAltitudeAndFurtherOrdinates()
    {
        var point = JsonSerializer.Deserialize<Point>("""{"type":"Point","coordinates":[1.0,2.0,3.0,4.0]}""")!;

        Assert.AreEqual(new Position(1, 2, 3, [4.0]), point.Coordinates);
        Json.AssertEqual("""{"type":"Point","coordinates":[1.0,2.0,3.0,4.0]}""", JsonSerializer.Serialize(point));
    }

    [TestMethod]
    public void RejectsATypeOutsideTheNineGeoJsonTypes()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<GeoJsonObject>("""{"type":"Circle"}"""));
    }

    [TestMethod]
    public void RejectsAGeoJsonObjectWithoutATypeMember()
    {
        Assert.ThrowsExactly<JsonException>(() =>
            JsonSerializer.Deserialize<GeoJsonObject>("""{"coordinates":[1.0,2.0]}"""));
    }

    [TestMethod]
    public void RejectsAGeometryWithoutACoordinatesMember()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Geometry>("""{"type":"Point"}"""));
    }

    [TestMethod]
    public void RejectsAFeatureIdThatIsNotAStringOrNumber()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Feature<JsonNode>>(
            """{"type":"Feature","id":{"x":1},"geometry":null,"properties":null}"""));
    }

    [TestMethod]
    public void ReadsAFeatureIdThatIsJsonNullAsAbsent()
    {
        var feature = JsonSerializer.Deserialize<Feature<JsonNode>>(
            """{"type":"Feature","id":null,"geometry":null,"properties":null}""")!;

        Assert.IsNull(feature.Id);
    }

    [TestMethod]
    public void RoundTripsANumericFeatureId()
    {
        var feature = new Feature<JsonNode> { Id = new FeatureId(42) };

        Json.AssertEqual(
            """{"type":"Feature","id":42,"geometry":null,"properties":null}""",
            JsonSerializer.Serialize(feature));
        Assert.AreEqual(new FeatureId(42), JsonSerializer.Deserialize<Feature<JsonNode>>(
            """{"type":"Feature","id":42,"geometry":null,"properties":null}""")!.Id);
    }

    [TestMethod]
    public void OmitsAFeatureIdThatHoldsNoValue()
    {
        var feature = new Feature<JsonNode> { Id = default(FeatureId) };

        Assert.AreEqual(
            """{"type":"Feature","geometry":null,"properties":null}""",
            JsonSerializer.Serialize(feature));
    }

    [TestMethod]
    public void ReadsAnEmptyFeatureCollection()
    {
        var collection = JsonSerializer.Deserialize<FeatureCollection<JsonNode>>(
            """{"type":"FeatureCollection","features":[]}""")!;

        Assert.IsTrue(collection.Features.IsEmpty);
        Json.AssertEqual(
            """{"type":"FeatureCollection","features":[]}""",
            JsonSerializer.Serialize(collection));
    }

    [TestMethod]
    public void HoldsArbitraryJsonInUntypedProperties()
    {
        var feature = JsonSerializer.Deserialize<Feature<JsonNode>>(
            """{"type":"Feature","geometry":null,"properties":{"a":[1,{"b":null}]}}""")!;

        Assert.IsInstanceOfType<JsonObject>(feature.Properties);
        Json.AssertEqual(
            """{"type":"Feature","geometry":null,"properties":{"a":[1,{"b":null}]}}""",
            JsonSerializer.Serialize(feature));
    }

    [TestMethod]
    public void WritesAPointWithoutAllocating()
    {
        var point = new Point
        {
            Coordinates = new Position(10.7522, 59.9139),
            Bbox = new BoundingBox(10.0, 59.0, 11.0, 60.0),
        };

        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        JsonSerializer.Serialize(writer, point);
        writer.Flush();
        writer.Reset(buffer);
        buffer.ResetWrittenCount();

        var before = GC.GetAllocatedBytesForCurrentThread();
        JsonSerializer.Serialize(writer, point);
        writer.Flush();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0, allocated);
    }
}
