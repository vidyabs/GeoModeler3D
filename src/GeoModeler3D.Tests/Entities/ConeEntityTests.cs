using System.Numerics;
using GeoModeler3D.Core.Entities;
using Xunit;

namespace GeoModeler3D.Tests.Entities;

public class ConeEntityTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 8.0);

        Assert.Equal(Vector3.Zero, cone.BaseCenter);
        Assert.Equal(Vector3.UnitZ, cone.Axis);
        Assert.Equal(3.0, cone.BaseRadius);
        Assert.Equal(8.0, cone.Height);
    }

    [Fact]
    public void HalfAngle_IsCorrect()
    {
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 4.0);
        var expected = System.Math.Atan2(3.0, 4.0);

        Assert.Equal(expected, cone.HalfAngle, 0.0001);
    }

    [Fact]
    public void Apex_IsCorrect()
    {
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 8.0);

        Assert.Equal(new Vector3(0, 0, 8), cone.Apex);
    }

    [Fact]
    public void BoundingBox_ContainsBaseAndApex()
    {
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 8.0);
        var bb = cone.BoundingBox;

        Assert.True(bb.Contains(Vector3.Zero));
        Assert.True(bb.Contains(cone.Apex));
    }

    [Fact]
    public void Transform_Translation()
    {
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 8.0);
        cone.Transform(Matrix4x4.CreateTranslation(5, 5, 5));

        Assert.Equal(5, cone.BaseCenter.X, 0.001f);
        Assert.Equal(5, cone.BaseCenter.Y, 0.001f);
        Assert.Equal(5, cone.BaseCenter.Z, 0.001f);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var cone = new ConeEntity(new Vector3(1, 2, 3), Vector3.UnitY, 5.0, 10.0, "MyCone");
        var clone = (ConeEntity)cone.Clone();

        Assert.NotEqual(cone.Id, clone.Id);
        Assert.Equal(cone.BaseCenter, clone.BaseCenter);
        Assert.Equal(cone.BaseRadius, clone.BaseRadius);
        Assert.Equal(cone.Height, clone.Height);
        Assert.Equal("MyCone", clone.Name);
    }

    [Fact]
    public void PropertyChanged_FiresOnBaseRadiusChange()
    {
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 8.0);
        var changedProps = new List<string>();
        cone.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

        cone.BaseRadius = 5.0;

        Assert.Contains("BaseRadius", changedProps);
        Assert.Contains("HalfAngle", changedProps);
    }
}
