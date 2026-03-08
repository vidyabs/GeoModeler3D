using System.Numerics;
using GeoModeler3D.Core.Entities;
using Xunit;

namespace GeoModeler3D.Tests.Entities;

public class SphereEntityTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var sphere = new SphereEntity(new Vector3(1, 2, 3), 5.0);

        Assert.Equal(new Vector3(1, 2, 3), sphere.Center);
        Assert.Equal(5.0, sphere.Radius);
        Assert.Equal("Sphere", sphere.Name);
    }

    [Fact]
    public void BoundingBox_IsCorrect()
    {
        var sphere = new SphereEntity(new Vector3(0, 0, 0), 2.0);
        var bb = sphere.BoundingBox;

        Assert.Equal(new Vector3(-2, -2, -2), bb.Min);
        Assert.Equal(new Vector3(2, 2, 2), bb.Max);
    }

    [Fact]
    public void BoundingBox_WithOffset_IsCorrect()
    {
        var sphere = new SphereEntity(new Vector3(5, 5, 5), 1.0);
        var bb = sphere.BoundingBox;

        Assert.Equal(new Vector3(4, 4, 4), bb.Min);
        Assert.Equal(new Vector3(6, 6, 6), bb.Max);
    }

    [Fact]
    public void Transform_Translation_ShiftsCenter()
    {
        var sphere = new SphereEntity(new Vector3(0, 0, 0), 3.0);
        sphere.Transform(Matrix4x4.CreateTranslation(10, 0, 0));

        Assert.Equal(10, sphere.Center.X, 0.001f);
        Assert.Equal(0, sphere.Center.Y, 0.001f);
        Assert.Equal(3.0, sphere.Radius);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var sphere = new SphereEntity(new Vector3(1, 2, 3), 4.0, "MySphere", EntityColor.Green);
        var clone = (SphereEntity)sphere.Clone();

        Assert.NotEqual(sphere.Id, clone.Id);
        Assert.Equal(sphere.Center, clone.Center);
        Assert.Equal(sphere.Radius, clone.Radius);
        Assert.Equal("MySphere", clone.Name);
        Assert.Equal(EntityColor.Green, clone.Color);

        clone.Center = new Vector3(99, 99, 99);
        Assert.Equal(new Vector3(1, 2, 3), sphere.Center);
    }

    [Fact]
    public void PropertyChanged_FiresOnRadiusChange()
    {
        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        var changedProps = new List<string>();
        sphere.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

        sphere.Radius = 5.0;

        Assert.Contains("Radius", changedProps);
    }

    [Fact]
    public void Accept_CallsCorrectVisitorMethod()
    {
        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        var visitor = new TestVisitor();

        sphere.Accept(visitor);

        Assert.Equal(nameof(SphereEntity), visitor.VisitedType);
    }

    private class TestVisitor : IEntityVisitor
    {
        public string? VisitedType { get; private set; }
        public void Visit(PointEntity entity) => VisitedType = nameof(PointEntity);
        public void Visit(TriangleEntity entity) => VisitedType = nameof(TriangleEntity);
        public void Visit(CircleEntity entity) => VisitedType = nameof(CircleEntity);
        public void Visit(SphereEntity entity) => VisitedType = nameof(SphereEntity);
        public void Visit(CylinderEntity entity) => VisitedType = nameof(CylinderEntity);
        public void Visit(ConeEntity entity) => VisitedType = nameof(ConeEntity);
        public void Visit(TorusEntity entity) => VisitedType = nameof(TorusEntity);
        public void Visit(CuttingPlaneEntity entity) => VisitedType = nameof(CuttingPlaneEntity);
        public void Visit(ContourCurveEntity entity) => VisitedType = nameof(ContourCurveEntity);
    }
}
