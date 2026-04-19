using System.Numerics;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Services;
using Xunit;

namespace GeoModeler3D.Tests.Services;

public class ContourExtractionServiceTests
{
    private readonly ContourExtractionService _svc = new();

    private static CuttingPlaneEntity MakePlane(Vector3 origin, Vector3 normal, Guid targetId)
    {
        var plane = new CuttingPlaneEntity(origin, normal);
        plane.TargetEntityIds.Add(targetId);
        return plane;
    }

    // ── Sphere ────────────────────────────────────────────────────────────────

    [Fact]
    public void Sphere_PlaneThrough_ReturnsCircle()
    {
        var sphere = new SphereEntity(Vector3.Zero, 5.0);
        var plane = MakePlane(new Vector3(0, 0, 3), Vector3.UnitZ, sphere.Id);

        var contours = _svc.Extract(plane, sphere);

        Assert.Single(contours);
        Assert.True(contours[0].IsClosed);
        // Circle radius = sqrt(25 - 9) = 4
        var pts = contours[0].Points;
        Assert.True(pts.Count > 0);
        float expectedR = 4f;
        foreach (var p in pts)
            Assert.Equal(expectedR, MathF.Sqrt(p.X * p.X + p.Y * p.Y), 2);
    }

    [Fact]
    public void Sphere_PlaneMissing_ReturnsEmpty()
    {
        var sphere = new SphereEntity(Vector3.Zero, 3.0);
        var plane = MakePlane(new Vector3(0, 0, 10), Vector3.UnitZ, sphere.Id);

        var contours = _svc.Extract(plane, sphere);

        Assert.Empty(contours);
    }

    [Fact]
    public void Sphere_PlaneThroughCenter_ReturnsGreatCircle()
    {
        var sphere = new SphereEntity(new Vector3(1, 2, 3), 5.0);
        var plane = MakePlane(new Vector3(1, 2, 3), Vector3.UnitZ, sphere.Id);

        var contours = _svc.Extract(plane, sphere);

        Assert.Single(contours);
        // All points should be at radius ≈ 5 from (1,2,3) projected onto the plane
        foreach (var p in contours[0].Points)
        {
            float dx = p.X - 1f, dy = p.Y - 2f;
            Assert.Equal(5f, MathF.Sqrt(dx * dx + dy * dy), 2);
        }
    }

    // ── Cylinder ──────────────────────────────────────────────────────────────

    [Fact]
    public void Cylinder_PerpendicularPlane_ReturnsClosedCircle()
    {
        // Cylinder along Z axis, plane perpendicular → circle
        var cyl = new CylinderEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 10.0);
        var plane = MakePlane(new Vector3(0, 0, 5), Vector3.UnitZ, cyl.Id);

        var contours = _svc.Extract(plane, cyl);

        Assert.Single(contours);
        Assert.True(contours[0].IsClosed);
        foreach (var p in contours[0].Points)
            Assert.Equal(3f, MathF.Sqrt(p.X * p.X + p.Y * p.Y), 2);
    }

    [Fact]
    public void Cylinder_PlaneMissesCylinder_ReturnsEmpty()
    {
        // Plane is above the cylinder entirely
        var cyl = new CylinderEntity(Vector3.Zero, Vector3.UnitZ, 2.0, 5.0);
        var plane = MakePlane(new Vector3(0, 0, 20), Vector3.UnitZ, cyl.Id);

        var contours = _svc.Extract(plane, cyl);

        Assert.Empty(contours);
    }

    // ── Cone ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Cone_PerpendicularPlane_ReturnsClosedContour()
    {
        // Cone along Z, plane perpendicular at mid-height → circle at half base radius
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 4.0, 8.0);
        var plane = MakePlane(new Vector3(0, 0, 4), Vector3.UnitZ, cone.Id);

        var contours = _svc.Extract(plane, cone);

        Assert.Single(contours);
        Assert.True(contours[0].IsClosed);
        // At h=4 out of 8, radius = 4*(1 - 4/8) = 2
        foreach (var p in contours[0].Points)
            Assert.Equal(2f, MathF.Sqrt(p.X * p.X + p.Y * p.Y), 2);
    }

    // ── Mesh ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Mesh_SingleTriangleCrossed_ReturnsContour()
    {
        // Triangle: one vertex above z=0, two below
        var positions = new Vector3[]
        {
            new(0, 0, 1), new(2, 0, -1), new(-2, 0, -1)
        };
        var mesh = new MeshEntity(positions);
        var plane = MakePlane(Vector3.Zero, Vector3.UnitZ, mesh.Id);

        var contours = _svc.Extract(plane, mesh);

        Assert.Single(contours);
        Assert.Equal(2, contours[0].Points.Count);
    }

    [Fact]
    public void Mesh_NoIntersection_ReturnsEmpty()
    {
        var positions = new Vector3[]
        {
            new(0, 0, 5), new(1, 0, 5), new(0, 1, 5)
        };
        var mesh = new MeshEntity(positions);
        var plane = MakePlane(Vector3.Zero, Vector3.UnitZ, mesh.Id);

        var contours = _svc.Extract(plane, mesh);

        Assert.Empty(contours);
    }

    // ── Unsupported entity type ────────────────────────────────────────────────

    [Fact]
    public void UnsupportedEntityType_ReturnsEmpty()
    {
        var point = new PointEntity(Vector3.Zero);
        var plane = MakePlane(Vector3.Zero, Vector3.UnitZ, point.Id);

        var contours = _svc.Extract(plane, point);

        Assert.Empty(contours);
    }
}
