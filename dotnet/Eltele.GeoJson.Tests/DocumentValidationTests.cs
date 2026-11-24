using System.Text.Json;
using System.Text.Json.Nodes;
using Eltele.GeoJson.Validation;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class DocumentValidationTests
{
    [TestMethod]
    [DataRow(nameof(RfcExamples.FeatureCollection))]
    [DataRow(nameof(RfcExamples.FeatureWithBoundingBox))]
    [DataRow(nameof(RfcExamples.Point))]
    [DataRow(nameof(RfcExamples.LineString))]
    [DataRow(nameof(RfcExamples.Polygon))]
    [DataRow(nameof(RfcExamples.PolygonWithHoles))]
    [DataRow(nameof(RfcExamples.MultiPoint))]
    [DataRow(nameof(RfcExamples.MultiLineString))]
    [DataRow(nameof(RfcExamples.MultiPolygon))]
    [DataRow(nameof(RfcExamples.GeometryCollection))]
    public void EveryRfcExampleIsValid(string example)
    {
        var json = (string)typeof(RfcExamples).GetField(example)!.GetValue(null)!;
        var document = JsonSerializer.Deserialize<GeoJsonObject>(json);

        var errors = new List<ValidationError>();
        GeoJsonValidator.FindViolations(document, errors);

        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(error => error.Message)));
    }

    [TestMethod]
    public void FindsViolationsInsideAFeatureCollection()
    {
        var document = new GeoJsonObject(new FeatureCollection<JsonNode>
        {
            Features =
            [
                new Feature<JsonNode>
                {
                    Geometry = new LineString { Coordinates = [new Position(10, 60)] },
                },
            ],
        });

        var errors = new List<ValidationError>();
        GeoJsonValidator.FindViolations(document, errors);

        Assert.IsTrue(errors.Any(error => error.Rule == ValidationRule.LineStringHasFewerThanTwoPositions));
    }

    [TestMethod]
    public void ThrowIfInvalidCarriesTheErrors()
    {
        var document = new GeoJsonObject(new FeatureCollection<JsonNode>
        {
            Features =
            [
                new Feature<JsonNode>
                {
                    Geometry = new LineString { Coordinates = [new Position(10, 60)] },
                },
            ],
        });

        var exception = Assert.ThrowsExactly<GeoJsonValidationException>(
            () => GeoJsonValidator.ThrowIfInvalid(document));

        Assert.IsTrue(exception.Errors.Any(error => error.Rule == ValidationRule.LineStringHasFewerThanTwoPositions));
        StringAssert.Contains(exception.Message, "RFC 7946");
    }

    [TestMethod]
    public void ThrowIfInvalidAcceptsAValidDocument()
    {
        var document = JsonSerializer.Deserialize<GeoJsonObject>(RfcExamples.FeatureCollection);

        GeoJsonValidator.ThrowIfInvalid(document);
    }

    [TestMethod]
    public void MessagesQuoteTheClauseAndTheOffendingMember()
    {
        var error = new ValidationError(ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType, "coordinates");

        StringAssert.Contains(error.Message, "§7.1");
        StringAssert.Contains(error.Message, "\"coordinates\"");
        Assert.AreEqual("7.1", error.Section);
        Assert.AreEqual(ValidationSeverity.Required, error.Severity);
    }
}
