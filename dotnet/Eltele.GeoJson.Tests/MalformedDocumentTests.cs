using System.Text.Json;
using Eltele.GeoJson.Validation;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class MalformedDocumentTests
{
    [TestMethod]
    [DataRow("""{"type":null}""")]
    [DataRow("""{"type":1}""")]
    [DataRow("[]")]
    [DataRow("""{"type":"Circle"}""")]
    [DataRow("""{"type":"Point","coordinates":"x"}""")]
    [DataRow("""{"type":"Point","coordinates":[1]}""")]
    [DataRow("""{"type":"Point","coordinates":[1,"x"]}""")]
    [DataRow("""{"type":"Point","bbox":"x"}""")]
    [DataRow("""{"type":"Point","bbox":[1,null]}""")]
    [DataRow("""{"type":"LineString","coordinates":[1,2]}""")]
    [DataRow("""{"type":"LineString","coordinates":5}""")]
    [DataRow("""{"type":"GeometryCollection","geometries":{}}""")]
    [DataRow("""{"type":"FeatureCollection","features":{}}""")]
    [DataRow("""{"type":"Feature","geometry":[1]}""")]
    [DataRow("""{"type":"Feature","id":true}""")]
    public void RejectsADocumentThatCannotBeRepresented(string json)
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<GeoJsonObject>(json));
    }

    [TestMethod]
    public void RejectsAGeometryWhoseTypeMemberIsNotAString()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Point>(
            """{"type":null,"coordinates":[1.0,2.0]}"""));
    }

    [TestMethod]
    public void RejectsWritingAFeatureIdThatHoldsNoValue()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Serialize(default(FeatureId)));
    }

    [TestMethod]
    public void RejectsAFeatureReadAsAGeometry()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Geometry>(
            """{"type":"Feature","geometry":null,"properties":null}"""));
    }

    [TestMethod]
    public void RejectsAGeometryReadAsAnotherGeometryType()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Point>(
            """{"type":"LineString","coordinates":[[1.0,2.0],[3.0,4.0]]}"""));
    }

    [TestMethod]
    public void RejectsWritingAGeoJsonObjectThatHoldsNoValue()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Serialize(default(GeoJsonObject)));
    }

    [TestMethod]
    public void RejectsWritingAGeometryThatHoldsNoValue()
    {
        var collection = new GeometryCollection { Geometries = [default(Geometry)] };

        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Serialize(collection));
    }

    [TestMethod]
    public void RejectsValidatingAGeometryThatHoldsNoValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            GeometryValidator.FindViolations(default(Geometry), new List<ValidationError>()));
    }

    [TestMethod]
    public void RejectsValidatingAGeoJsonObjectThatHoldsNoValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            GeoJsonValidator.FindViolations(default(GeoJsonObject), new List<ValidationError>()));
    }

    [TestMethod]
    public void RejectsValidatingATypedGeoJsonObjectThatHoldsNoValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            GeoJsonValidator.FindViolations(default(GeoJsonObject<CityProperties>), new List<ValidationError>()));
    }
}
