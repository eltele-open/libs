using Eltele.GeoJson.Validation;

namespace Eltele.GeoJson.Tests;

[TestClass]
public sealed class CoordinateDimensionsTests
{
    private static readonly Position Two = new(1, 2);
    private static readonly Position Three = new(1, 2, 3);

    [TestMethod]
    public void ReadsTheWidestPositionOfAGeometry()
    {
        Assert.AreEqual(2, CoordinateDimensions.Of(new Geometry(new Point { Coordinates = Two })));
        Assert.AreEqual(3, CoordinateDimensions.Of(new Geometry(new Point { Coordinates = Three })));
        Assert.AreEqual(3, CoordinateDimensions.Of(new Geometry(new MultiPoint { Coordinates = [Two, Three] })));
        Assert.AreEqual(2, CoordinateDimensions.Of(new Geometry(new LineString { Coordinates = [Two, Two] })));
        Assert.AreEqual(3, CoordinateDimensions.Of(new Geometry(
            new MultiLineString { Coordinates = [[Two, Two], [Three, Three]] })));
        Assert.AreEqual(3, CoordinateDimensions.Of(new Geometry(new Polygon
        {
            Coordinates = [new LinearRing([Two, Two, Three, Two])],
        })));
        Assert.AreEqual(3, CoordinateDimensions.Of(new Geometry(new MultiPolygon
        {
            Coordinates = [[new LinearRing([Two, Two, Three, Two])]],
        })));
    }

    [TestMethod]
    public void ReadsTheWidestPositionOfAGeometryCollection()
    {
        var collection = new Geometry(new GeometryCollection
        {
            Geometries =
            [
                new Geometry(new Point { Coordinates = Two }),
                new Geometry(new Point { Coordinates = Three }),
            ],
        });

        Assert.AreEqual(3, CoordinateDimensions.Of(collection));
    }

    [TestMethod]
    public void ReportsNoDimensionsForAGeometryWithoutPositions()
    {
        Assert.IsNull(CoordinateDimensions.Of(default(Geometry)));
        Assert.IsNull(CoordinateDimensions.Of(new Geometry(new MultiPoint())));
        Assert.IsNull(CoordinateDimensions.Of(new Geometry(new GeometryCollection())));
    }
}
