using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class ValueTypeTests
{
    [TestMethod]
    public void PositionsWithTheSameOrdinatesAreEqual()
    {
        var left = new Position(1, 2, 3, [4.0, 5.0]);
        var right = new Position(1, 2, 3, [4.0, 5.0]);

        Assert.AreEqual(left, right);
        Assert.IsTrue(left == right);
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }

    [TestMethod]
    public void PositionsThatDifferInAnOrdinateAreNotEqual()
    {
        Assert.AreNotEqual(new Position(1, 2), new Position(1, 2, 0));
        Assert.AreNotEqual(new Position(1, 2, 3), new Position(1, 2, 3, [4.0]));
        Assert.IsTrue(new Position(1, 2) != new Position(1, 3));
    }

    [TestMethod]
    public void PositionsCountTheirOrdinates()
    {
        Assert.AreEqual(2, new Position(1, 2).Count);
        Assert.AreEqual(3, new Position(1, 2, 3).Count);
        Assert.AreEqual(5, new Position(1, 2, 3, [4.0, 5.0]).Count);
        Assert.AreEqual(2, default(Position).Count);
        Assert.IsTrue(default(Position).ExtraOrdinates.IsEmpty);
    }

    [TestMethod]
    public void FormatsAPositionAsAnArray()
    {
        Assert.AreEqual("[1, 2]", new Position(1, 2).ToString());
        Assert.AreEqual("[1, 2, 3]", new Position(1, 2, 3).ToString());
        Assert.AreEqual("[1, 2, 3, 4]", new Position(1, 2, 3, [4.0]).ToString());
    }

    [TestMethod]
    public void BoundingBoxesWithTheSameValuesAreEqual()
    {
        var left = new BoundingBox(1, 2, 3, 4);
        var right = new BoundingBox(ImmutableArray.Create(1.0, 2.0, 3.0, 4.0));

        Assert.AreEqual(left, right);
        Assert.IsTrue(left == right);
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        Assert.IsTrue(left != new BoundingBox(1, 2, 3, 5));
    }

    [TestMethod]
    public void BoundingBoxReportsItsLengthAndDimensions()
    {
        var fromValues = new BoundingBox(1, 2, 3, 4, 5, 6);

        Assert.AreEqual(6, fromValues.Length);
        Assert.AreEqual(3, fromValues.Dimensions);
        Assert.AreEqual(0, default(BoundingBox).Length);
        Assert.IsTrue(default(BoundingBox).Values.IsEmpty);
    }

    [TestMethod]
    public void LinearRingsWithTheSamePositionsAreEqual()
    {
        Position[] positions = [new(0, 0), new(1, 0), new(1, 1), new(0, 0)];
        LinearRing fromArray = positions;
        LinearRing fromImmutable = ImmutableArray.Create(positions);

        Assert.AreEqual(fromArray, fromImmutable);
        Assert.IsTrue(fromArray == fromImmutable);
        Assert.AreEqual(fromArray.GetHashCode(), fromImmutable.GetHashCode());
        Assert.IsTrue(fromArray != new LinearRing([new Position(0, 0)]));
        Assert.IsTrue(default(LinearRing).Positions.IsEmpty);
    }

    [TestMethod]
    public void ValueTypesCompareThroughObject()
    {
        Assert.IsTrue(new Position(1, 2).Equals((object)new Position(1, 2)));
        Assert.IsFalse(new Position(1, 2).Equals((object)new Position(1, 3)));
        Assert.IsFalse(new Position(1, 2).Equals(null));
        Assert.IsTrue(new BoundingBox(1, 2, 3, 4).Equals((object)new BoundingBox(1, 2, 3, 4)));
        Assert.IsTrue(new LinearRing([new Position(1, 2)]).Equals((object)new LinearRing([new Position(1, 2)])));
    }

    [TestMethod]
    public void ConvertsADoubleArrayToABoundingBox()
    {
        BoundingBox boundingBox = new double[] { 1, 2, 3, 4 };

        Assert.AreEqual(2, boundingBox.Dimensions);
    }

    [TestMethod]
    public void EveryTypeMemberHoldsItsSpecLiteral()
    {
        Assert.AreEqual("Point", new Point().Type);
        Assert.AreEqual("MultiPoint", new MultiPoint().Type);
        Assert.AreEqual("LineString", new LineString().Type);
        Assert.AreEqual("MultiLineString", new MultiLineString().Type);
        Assert.AreEqual("Polygon", new Polygon().Type);
        Assert.AreEqual("MultiPolygon", new MultiPolygon().Type);
        Assert.AreEqual("GeometryCollection", new GeometryCollection().Type);
        Assert.AreEqual("Feature", new Feature<JsonNode>().Type);
        Assert.AreEqual("FeatureCollection", new FeatureCollection<JsonNode>().Type);
    }

    [TestMethod]
    public void RoundTripsAPositionOnItsOwn()
    {
        var position = new Position(1.5, 2.5, 3.5, [4.5]);

        Assert.AreEqual("[1.5,2.5,3.5,4.5]", JsonSerializer.Serialize(position));
        Assert.AreEqual(position, JsonSerializer.Deserialize<Position>("[1.5,2.5,3.5,4.5]"));
    }

    [TestMethod]
    public void RoundTripsABoundingBoxOnItsOwn()
    {
        var boundingBox = new BoundingBox(1, 2, 3, 4, 5, 6);

        Assert.AreEqual("[1,2,3,4,5,6]", JsonSerializer.Serialize(boundingBox));
        Assert.AreEqual(boundingBox, JsonSerializer.Deserialize<BoundingBox>("[1,2,3,4,5,6]"));
    }

    [TestMethod]
    public void RoundTripsAFeatureIdOnItsOwn()
    {
        Assert.AreEqual("\"f1\"", JsonSerializer.Serialize(new FeatureId("f1")));
        Assert.AreEqual("42", JsonSerializer.Serialize(new FeatureId(42)));
        Assert.AreEqual(new FeatureId("f1"), JsonSerializer.Deserialize<FeatureId>("\"f1\""));
    }

    [TestMethod]
    public void SerializesAGeometryOnItsOwn()
    {
        var geometry = new Geometry(new Point { Coordinates = new Position(1, 2) });

        Assert.AreEqual("""{"type":"Point","coordinates":[1,2]}""", JsonSerializer.Serialize(geometry));
    }

    [TestMethod]
    public void RoundTripsALinearRingOnItsOwn()
    {
        var ring = new LinearRing([new Position(0, 0), new Position(1, 0), new Position(0, 0)]);

        Assert.AreEqual("[[0,0],[1,0],[0,0]]", JsonSerializer.Serialize(ring));
        Assert.AreEqual(ring, JsonSerializer.Deserialize<LinearRing>("[[0,0],[1,0],[0,0]]"));
    }
}
