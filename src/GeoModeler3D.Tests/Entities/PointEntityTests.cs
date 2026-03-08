using System.Numerics;
using GeoModeler3D.Core.Entities;
using Xunit;

namespace GeoModeler3D.Tests.Entities;

public class PointEntityTests
{
    [Fact]
    public void Constructor_SetsPosition()
    {
        var pos = new Vector3(1, 2, 3);
        var point = new PointEntity(pos);

        Assert.Equal(pos, point.Position);
        Assert.Equal("Point", point.Name);
        Assert.True(point.IsVisible);
        Assert.Equal("Default", point.Layer);
    }

    [Fact]
    public void Clone_ReturnsNewInstanceWithSameProperties()
    {
        var point = new PointEntity(new Vector3(5, 10, 15), "P1", EntityColor.Red);
        var clone = (PointEntity)point.Clone();

        Assert.NotEqual(point.Id, clone.Id);
        Assert.Equal(point.Position, clone.Position);
        Assert.Equal(point.Name, clone.Name);
        Assert.Equal(point.Color, clone.Color);
    }

    [Fact]
    public void Transform_WithTranslation_ShiftsPosition()
    {
        var point = new PointEntity(new Vector3(1, 2, 3));
        var matrix = Matrix4x4.CreateTranslation(10, 20, 30);

        point.Transform(matrix);

        Assert.Equal(11, point.Position.X, 0.001f);
        Assert.Equal(22, point.Position.Y, 0.001f);
        Assert.Equal(33, point.Position.Z, 0.001f);
    }

    [Fact]
    public void Transform_WithIdentity_NoChange()
    {
        var original = new Vector3(1, 2, 3);
        var point = new PointEntity(original);

        point.Transform(Matrix4x4.Identity);

        Assert.Equal(original, point.Position);
    }

    [Fact]
    public void BoundingBox_IsSinglePoint()
    {
        var pos = new Vector3(5, 5, 5);
        var point = new PointEntity(pos);

        Assert.Equal(pos, point.BoundingBox.Min);
        Assert.Equal(pos, point.BoundingBox.Max);
    }

    [Fact]
    public void PropertyChanged_FiresOnPositionChange()
    {
        var point = new PointEntity(Vector3.Zero);
        string? changedProperty = null;
        point.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        point.Position = new Vector3(1, 1, 1);

        Assert.Equal("Position", changedProperty);
    }

    [Fact]
    public void PropertyChanged_DoesNotFireWhenValueUnchanged()
    {
        var point = new PointEntity(Vector3.Zero);
        bool fired = false;
        point.PropertyChanged += (_, _) => fired = true;

        point.Position = Vector3.Zero;

        Assert.False(fired);
    }
}
