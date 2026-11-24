using System.Collections.Immutable;
using System.Text.Json.Nodes;
using Eltele.GeoJson.Validation;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class ValidatorTests
{
    [TestMethod]
    public void EveryRuleHasASeveritySectionAndMessage()
    {
        foreach (var rule in Enum.GetValues<ValidationRule>())
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(ValidationRules.Section(rule)), rule.ToString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(ValidationRules.Message(rule, null)), rule.ToString());
            Assert.IsTrue(Enum.IsDefined(ValidationRules.Severity(rule)), rule.ToString());
        }
    }

    [TestMethod]
    public void RejectsARuleThatIsNotDefined()
    {
        var rule = (ValidationRule)999;

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ValidationRules.Severity(rule));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ValidationRules.Section(rule));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ValidationRules.Message(rule, null));
    }

    [TestMethod]
    public void ReportsAPositionWithMoreThanThreeElementsAsARecommendation()
    {
        var errors = Validate(errors => PositionValidator.FindViolations(new Position(1, 2, 3, [4.0]), errors));

        AssertRule(errors, ValidationRule.PositionHasMoreThanThreeElements);
        Assert.AreEqual(ValidationSeverity.Recommended, errors[0].Severity);
        Assert.AreEqual("3.1.1", errors[0].Section);
    }

    [TestMethod]
    public void ReportsALineStringWithFewerThanTwoPositions()
    {
        var lineString = new LineString { Coordinates = [new Position(1, 2)] };

        AssertRule(Validate(errors => LineStringValidator.FindViolations(lineString, errors)),
            ValidationRule.LineStringHasFewerThanTwoPositions);
    }

    [TestMethod]
    public void ReportsALinearRingWithFewerThanFourPositions()
    {
        var ring = Ring((0, 0), (1, 0), (0, 0));

        AssertRule(Validate(errors => LinearRingValidator.FindViolations(ring, RingKind.Exterior, errors)),
            ValidationRule.LinearRingHasFewerThanFourPositions);
    }

    [TestMethod]
    public void ReportsALinearRingThatIsNotClosed()
    {
        var ring = Ring((0, 0), (1, 0), (1, 1), (0, 1));

        AssertRule(Validate(errors => LinearRingValidator.FindViolations(ring, RingKind.Exterior, errors)),
            ValidationRule.LinearRingIsNotClosed);
    }

    [TestMethod]
    public void ReportsARingThatCrossesTheAntimeridian()
    {
        var ring = Ring((170, 0), (-170, 0), (-170, 10), (170, 10), (170, 0));

        AssertRule(Validate(errors => LinearRingValidator.FindViolations(ring, RingKind.Exterior, errors)),
            ValidationRule.GeometryCrossesTheAntimeridian);
    }

    [TestMethod]
    public void AcceptsARingTooShortToHaveAWinding()
    {
        var ring = Ring((0, 0), (0, 0));

        var errors = Validate(errors => LinearRingValidator.FindViolations(ring, RingKind.Exterior, errors));

        Assert.IsFalse(errors.Any(error => error.Rule == ValidationRule.ExteriorRingIsClockwise));
    }

    [TestMethod]
    public void ReportsAClockwiseExteriorRing()
    {
        var ring = Ring((0, 0), (0, 1), (1, 1), (1, 0), (0, 0));

        AssertRule(Validate(errors => LinearRingValidator.FindViolations(ring, RingKind.Exterior, errors)),
            ValidationRule.ExteriorRingIsClockwise);
    }

    [TestMethod]
    public void ReportsACounterclockwiseInteriorRing()
    {
        var ring = Ring((0.2, 0.2), (0.8, 0.2), (0.8, 0.8), (0.2, 0.8), (0.2, 0.2));

        AssertRule(Validate(errors => LinearRingValidator.FindViolations(ring, RingKind.Interior, errors)),
            ValidationRule.InteriorRingIsCounterClockwise);
    }

    [TestMethod]
    public void ReportsAPolygonWhoseRingsFollowTheRightHandRule()
    {
        var exterior = Ring((100, 0), (101, 0), (101, 1), (100, 1), (100, 0));
        var hole = Ring((100.8, 0.8), (100.8, 0.2), (100.2, 0.2), (100.2, 0.8), (100.8, 0.8));

        var errors = Validate(errors => PolygonValidator.FindViolations(
            new Polygon { Coordinates = [exterior, hole] },
            errors));

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void ReportsALineStringThatCrossesTheAntimeridian()
    {
        var lineString = new LineString { Coordinates = [new Position(170, 0), new Position(-170, 0)] };

        AssertRule(Validate(errors => LineStringValidator.FindViolations(lineString, errors)),
            ValidationRule.GeometryCrossesTheAntimeridian);
    }

    [TestMethod]
    public void AcceptsTheCutMultiLineStringOfSection3_1_9()
    {
        var multiLineString = new MultiLineString
        {
            Coordinates =
            [
                [new Position(170, 45), new Position(180, 45)],
                [new Position(-180, 45), new Position(-170, 45)],
            ],
        };

        Assert.IsEmpty(Validate(errors => MultiLineStringValidator.FindViolations(multiLineString, errors)));
    }

    [TestMethod]
    public void ReportsABoundingBoxWithAnOddNumberOfValues()
    {
        AssertRule(Validate(errors => BoundingBoxValidator.FindViolations(new BoundingBox(1, 2, 3, 4, 5), null, errors)),
            ValidationRule.BoundingBoxHasOddNumberOfValues);
    }

    [TestMethod]
    public void ReportsABoundingBoxWithFewerThanFourValues()
    {
        AssertRule(Validate(errors => BoundingBoxValidator.FindViolations(new BoundingBox(1, 2), null, errors)),
            ValidationRule.BoundingBoxHasFewerThanFourValues);
    }

    [TestMethod]
    public void ReportsABoundingBoxWhoseNorthEastCornerIsSouthOfTheSouthWestCorner()
    {
        AssertRule(Validate(errors => BoundingBoxValidator.FindViolations(new BoundingBox(10, 20, 0, 10), null, errors)),
            ValidationRule.BoundingBoxNorthEastIsNotNorthOfSouthWest);
    }

    [TestMethod]
    public void ReportsABoundingBoxLatitudeBeyondThePoles()
    {
        AssertRule(Validate(errors => BoundingBoxValidator.FindViolations(new BoundingBox(0, -95, 10, 10), null, errors)),
            ValidationRule.BoundingBoxLatitudeIsBeyondThePoles);
    }

    [TestMethod]
    public void AcceptsTheBoundingBoxOfASphericalCapOverTheNorthPole()
    {
        Assert.IsEmpty(Validate(errors => BoundingBoxValidator.FindViolations(new BoundingBox(-180, 50, 180, 90), 2, errors)));
    }

    [TestMethod]
    public void ReportsABoundingBoxWithADifferentNumberOfDimensionsThanItsCoordinates()
    {
        var point = new Point { Coordinates = new Position(1, 2), Bbox = new BoundingBox(0, 1, 2, 3, 4, 5) };

        AssertRule(Validate(errors => PointValidator.FindViolations(point, errors)),
            ValidationRule.BoundingBoxDimensionsDifferFromCoordinates);
    }

    [TestMethod]
    public void ReportsANestedGeometryCollectionAsARecommendation()
    {
        var nested = new GeometryCollection
        {
            Geometries = [new Geometry(new Point { Coordinates = new Position(1, 2) }), new Geometry(new Point { Coordinates = new Position(3, 4) })],
        };
        var collection = new GeometryCollection
        {
            Geometries = [new Geometry(nested), new Geometry(new Point { Coordinates = new Position(5, 6) })],
        };

        var errors = Validate(errors => GeometryCollectionValidator.FindViolations(collection, errors));

        AssertRule(errors, ValidationRule.GeometryCollectionIsNested);
        Assert.AreEqual(ValidationSeverity.Recommended, errors[0].Severity);
    }

    [TestMethod]
    public void ReportsAGeometryCollectionWithASinglePart()
    {
        var collection = new GeometryCollection
        {
            Geometries = [new Geometry(new Point { Coordinates = new Position(1, 2) })],
        };

        AssertRule(Validate(errors => GeometryCollectionValidator.FindViolations(collection, errors)),
            ValidationRule.GeometryCollectionHasOnlyOnePart);
    }

    [TestMethod]
    public void AcceptsAnEmptyGeometryCollection()
    {
        Assert.IsEmpty(Validate(errors => GeometryCollectionValidator.FindViolations(
            new GeometryCollection(),
            errors)));
    }

    [TestMethod]
    public void ReportsFeaturePropertiesThatAreNotAnObject()
    {
        var feature = new Feature<JsonNode> { Properties = JsonValue.Create(1) };

        AssertRule(Validate(errors => FeatureValidator.FindViolations(feature, errors)),
            ValidationRule.FeaturePropertiesIsNotAnObject);
    }

    [TestMethod]
    public void AcceptsTypedFeatureProperties()
    {
        var feature = new Feature<CityProperties> { Properties = new CityProperties("Oslo", 700_000) };

        Assert.IsEmpty(Validate(errors => FeatureValidator.FindViolations(feature, errors)));
    }

    [TestMethod]
    public void ReportsAForeignMemberNamedType()
    {
        var point = new Point
        {
            Coordinates = new Position(1, 2),
            ForeignMembers = new JsonObject { ["type"] = "Circle" },
        };

        var errors = Validate(errors => PointValidator.FindViolations(point, errors));

        AssertRule(errors, ValidationRule.ForeignMemberIsTheTypeMember);
        Assert.AreEqual("type", errors[0].Member);
    }

    [TestMethod]
    [DataRow("geometry")]
    [DataRow("properties")]
    [DataRow("features")]
    public void ReportsAForeignMemberThatAnotherGeoJsonTypeDefines(string member)
    {
        var point = new Point
        {
            Coordinates = new Position(1, 2),
            ForeignMembers = new JsonObject { [member] = "x" },
        };

        var errors = Validate(errors => PointValidator.FindViolations(point, errors));

        AssertRule(errors, ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType);
        Assert.AreEqual(member, errors[0].Member);
    }

    [TestMethod]
    public void ReportsAFeatureThatCarriesCoordinates()
    {
        var feature = new Feature<JsonNode>
        {
            ForeignMembers = new JsonObject { ["coordinates"] = new JsonArray(1, 2) },
        };

        AssertRule(Validate(errors => FeatureValidator.FindViolations(feature, errors)),
            ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType);
    }

    [TestMethod]
    public void ReportsAFeatureCollectionThatCarriesProperties()
    {
        var collection = new FeatureCollection<JsonNode>
        {
            ForeignMembers = new JsonObject { ["properties"] = new JsonObject() },
        };

        AssertRule(Validate(errors => FeatureCollectionValidator.FindViolations(collection, errors)),
            ValidationRule.ForeignMemberIsAMemberOfAnotherGeoJsonType);
    }

    [TestMethod]
    public void AcceptsAForeignMemberThatBelongsToNoGeoJsonType()
    {
        var point = new Point
        {
            Coordinates = new Position(1, 2),
            ForeignMembers = new JsonObject { ["title"] = "Example Feature" },
        };

        Assert.IsEmpty(Validate(errors => PointValidator.FindViolations(point, errors)));
    }

    private static List<ValidationError> Validate(Action<List<ValidationError>> findViolations)
    {
        var errors = new List<ValidationError>();
        findViolations(errors);
        return errors;
    }

    private static void AssertRule(IReadOnlyCollection<ValidationError> errors, ValidationRule rule) =>
        Assert.IsTrue(
            errors.Any(error => error.Rule == rule),
            $"Expected {rule}, but found [{string.Join(", ", errors.Select(error => error.Rule))}]");

    private static LinearRing Ring(params (double Longitude, double Latitude)[] positions) =>
        new(positions.Select(position => new Position(position.Longitude, position.Latitude)).ToImmutableArray());
}
