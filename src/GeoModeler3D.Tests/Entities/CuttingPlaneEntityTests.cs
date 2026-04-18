using System.Numerics;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Serialization;
using Xunit;

namespace GeoModeler3D.Tests.Entities;

public class CuttingPlaneEntityTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var origin = new Vector3(1, 2, 3);
        var normal = new Vector3(0, 0, 1);

        var plane = new CuttingPlaneEntity(origin, normal);

        Assert.Equal(origin, plane.Origin);
        Assert.Equal(Vector3.UnitZ, plane.Normal);
        Assert.Equal(10.0, plane.DisplayWidth);
        Assert.Equal(10.0, plane.DisplayHeight);
        Assert.Equal(0.3, plane.Opacity, 3);
        Assert.False(plane.IsCappingEnabled);
        Assert.Empty(plane.TargetEntityIds);
        Assert.Equal("CuttingPlane", plane.Name);
        Assert.Equal(ClipSide.None, plane.ClipSide);
        Assert.Equal(0.5, plane.GapDistance, 3);
    }

    [Fact]
    public void Constructor_NormalizesNormal()
    {
        var plane = new CuttingPlaneEntity(Vector3.Zero, new Vector3(0, 0, 5));

        Assert.Equal(Vector3.UnitZ, plane.Normal);
    }

    [Fact]
    public void Transform_Translation_ShiftsOrigin()
    {
        var plane = new CuttingPlaneEntity(Vector3.Zero, Vector3.UnitZ);
        plane.Transform(Matrix4x4.CreateTranslation(3, 0, 0));

        Assert.Equal(3f, plane.Origin.X, 3);
        Assert.Equal(Vector3.UnitZ, plane.Normal);
    }

    [Fact]
    public void Transform_Rotation_RotatesNormal()
    {
        var plane = new CuttingPlaneEntity(Vector3.Zero, Vector3.UnitZ);
        plane.Transform(Matrix4x4.CreateRotationX(MathF.PI / 2));

        Assert.Equal(0f, plane.Normal.Z, 3);
        Assert.Equal(-1f, plane.Normal.Y, 3);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var plane = new CuttingPlaneEntity(new Vector3(1, 2, 3), Vector3.UnitY, "MyPlane", EntityColor.Red)
        {
            DisplayWidth = 5.0,
            DisplayHeight = 8.0,
            Opacity = 0.6,
            IsCappingEnabled = true,
            ClipSide = ClipSide.Positive,
            GapDistance = 1.2
        };
        var targetId = Guid.NewGuid();
        plane.TargetEntityIds.Add(targetId);

        var clone = (CuttingPlaneEntity)plane.Clone();

        Assert.NotEqual(plane.Id, clone.Id);
        Assert.Equal(plane.Origin, clone.Origin);
        Assert.Equal(plane.Normal, clone.Normal);
        Assert.Equal(5.0, clone.DisplayWidth);
        Assert.Equal(8.0, clone.DisplayHeight);
        Assert.Equal(0.6, clone.Opacity, 3);
        Assert.True(clone.IsCappingEnabled);
        Assert.Equal(ClipSide.Positive, clone.ClipSide);
        Assert.Equal(1.2, clone.GapDistance, 3);
        Assert.Equal("MyPlane", clone.Name);
        Assert.Equal(EntityColor.Red, clone.Color);
        Assert.Single(clone.TargetEntityIds);
        Assert.Equal(targetId, clone.TargetEntityIds[0]);

        // Mutating clone's list does not affect original
        clone.TargetEntityIds.Clear();
        Assert.Single(plane.TargetEntityIds);
    }

    [Fact]
    public void BoundingBox_ContainsOrigin()
    {
        var plane = new CuttingPlaneEntity(new Vector3(5, 5, 5), Vector3.UnitZ)
        {
            DisplayWidth = 4.0,
            DisplayHeight = 4.0
        };
        var bb = plane.BoundingBox;

        Assert.True(bb.Min.X <= 5f && bb.Max.X >= 5f);
        Assert.True(bb.Min.Y <= 5f && bb.Max.Y >= 5f);
    }

    [Fact]
    public void Accept_CallsCorrectVisitorMethod()
    {
        var plane = new CuttingPlaneEntity(Vector3.Zero, Vector3.UnitZ);
        var visitor = new CuttingPlaneTestVisitor();

        plane.Accept(visitor);

        Assert.Equal(nameof(CuttingPlaneEntity), visitor.VisitedType);
    }

    [Fact]
    public void Serialize_RoundTrip_PreservesAllProperties()
    {
        var plane = new CuttingPlaneEntity(new Vector3(1, 2, 3), new Vector3(0, 1, 0), "TestPlane", EntityColor.Cyan)
        {
            DisplayWidth = 6.0,
            DisplayHeight = 8.0,
            Opacity = 0.5,
            IsCappingEnabled = true,
            ClipSide = ClipSide.Negative,
            GapDistance = 1.5
        };
        var targetId = Guid.NewGuid();
        plane.TargetEntityIds.Add(targetId);

        var visitor = new EntitySerializationVisitor();
        plane.Accept(visitor);
        var dict = visitor.Result;

        Assert.Equal("CuttingPlane", dict["type"]);
        Assert.Equal("TestPlane", dict["name"]);
        Assert.Equal(6.0, dict["displayWidth"]);
        Assert.Equal(8.0, dict["displayHeight"]);
        Assert.Equal(0.5, dict["opacity"]);
        Assert.True((bool)dict["isCappingEnabled"]!);
        Assert.Equal("Negative", dict["clipSide"]);
        Assert.Equal(1.5, dict["gapDistance"]);
        var ids = (List<string>)dict["targetEntityIds"]!;
        Assert.Single(ids);
        Assert.Equal(targetId.ToString(), ids[0]);
    }

    [Fact]
    public void ClipSide_DefaultIsNone_GapDistanceDefault()
    {
        var plane = new CuttingPlaneEntity(Vector3.Zero, Vector3.UnitZ);
        Assert.Equal(ClipSide.None, plane.ClipSide);
        Assert.Equal(0.5, plane.GapDistance, 3);
    }

    [Fact]
    public void Serialize_ClipSideAndGapDistance_Roundtrip()
    {
        var plane = new CuttingPlaneEntity(Vector3.Zero, Vector3.UnitZ)
        {
            ClipSide = ClipSide.BothWithGap,
            GapDistance = 2.0
        };

        var visitor = new EntitySerializationVisitor();
        plane.Accept(visitor);
        var dict = visitor.Result;

        Assert.Equal("BothWithGap", dict["clipSide"]);
        Assert.Equal(2.0, dict["gapDistance"]);
    }

    private class CuttingPlaneTestVisitor : IEntityVisitor
    {
        public string? VisitedType { get; private set; }
        public void Visit(PointEntity entity) => VisitedType = nameof(PointEntity);
        public void Visit(TriangleEntity entity) => VisitedType = nameof(TriangleEntity);
        public void Visit(CircleEntity entity) => VisitedType = nameof(CircleEntity);
        public void Visit(SphereEntity entity) => VisitedType = nameof(SphereEntity);
        public void Visit(CylinderEntity entity) => VisitedType = nameof(CylinderEntity);
        public void Visit(ConeEntity entity) => VisitedType = nameof(ConeEntity);
        public void Visit(TorusEntity entity) => VisitedType = nameof(TorusEntity);
        public void Visit(MeshEntity entity) => VisitedType = nameof(MeshEntity);
        public void Visit(VectorEntity entity) => VisitedType = nameof(VectorEntity);
        public void Visit(PlaneEntity entity) => VisitedType = nameof(PlaneEntity);
        public void Visit(CuttingPlaneEntity entity) => VisitedType = nameof(CuttingPlaneEntity);
        public void Visit(ContourCurveEntity entity) => VisitedType = nameof(ContourCurveEntity);
    }
}
