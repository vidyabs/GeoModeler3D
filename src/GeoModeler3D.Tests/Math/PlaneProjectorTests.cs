using System.Numerics;
using GeoModeler3D.Core.Math;
using Xunit;

namespace GeoModeler3D.Tests.Math;

public class PlaneProjectorTests
{
    private const float Tol = 1e-4f;

    // ── Project ───────────────────────────────────────────────────────────────

    [Fact]
    public void Project_ZNormal_PointsInXY_UVEqualsXY()
    {
        // For a Z-normal plane at origin, the tangents are (1,0,0) and (0,1,0)
        // (or some permutation), so u = X, v = Y component
        var points3D = new List<Vector3>
        {
            new(1, 2, 0),
            new(-3, 4, 0),
            new(0, 0, 0)
        };

        var (pts2D, u, v) = PlaneProjector.Project(points3D, Vector3.UnitZ, Vector3.Zero);

        Assert.Equal(3, pts2D.Count);

        // Each 2D point should round-trip via Lift
        var lifted = PlaneProjector.Lift(pts2D, u, v, Vector3.Zero);
        for (int i = 0; i < points3D.Count; i++)
            AssertNearlyEqual(points3D[i], lifted[i]);
    }

    [Fact]
    public void Project_OffsetOrigin_ShiftsCoordinates()
    {
        var origin = new Vector3(5, 0, 0);
        var pts3D = new List<Vector3> { new(6, 0, 0) };

        var (pts2D, u, v) = PlaneProjector.Project(pts3D, Vector3.UnitZ, origin);

        // point is 1 unit from origin along X; should project to distance 1 in UV
        float len = MathF.Sqrt(pts2D[0].X * pts2D[0].X + pts2D[0].Y * pts2D[0].Y);
        Assert.Equal(1f, len, 3);
    }

    // ── Lift ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Lift_ZeroCoordinates_ReturnsOrigin()
    {
        var pts2D = new List<Vector2> { Vector2.Zero };
        var (_, u, v) = PlaneProjector.Project([Vector3.Zero], Vector3.UnitZ, Vector3.Zero);

        var lifted = PlaneProjector.Lift(pts2D, u, v, Vector3.Zero);

        AssertNearlyEqual(Vector3.Zero, lifted[0]);
    }

    // ── Round-trip ────────────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_ZNormalPlane_RestoresPoints()
    {
        var origin = new Vector3(1, 2, 3);
        var pts3D = new List<Vector3>
        {
            new(2, 3, 3), new(-1, 5, 3), new(4, 0, 3)
        };

        var (pts2D, u, v) = PlaneProjector.Project(pts3D, Vector3.UnitZ, origin);
        var restored = PlaneProjector.Lift(pts2D, u, v, origin);

        for (int i = 0; i < pts3D.Count; i++)
            AssertNearlyEqual(pts3D[i], restored[i]);
    }

    [Fact]
    public void RoundTrip_TiltedPlane_RestoresPoints()
    {
        // Plane with a tilted normal
        var normal = Vector3.Normalize(new Vector3(1, 1, 1));
        var origin = new Vector3(0, 0, 0);
        var pts3D = new List<Vector3>
        {
            new(1, -1, 0), new(-2, 1, 1), new(0, 0, 0)
        };

        // Project then lift each point
        var (pts2D, u, v) = PlaneProjector.Project(pts3D, normal, origin);
        var restored = PlaneProjector.Lift(pts2D, u, v, origin);

        // The restored points are the projections of the originals ONTO the plane,
        // not the originals themselves (unless the originals are already on the plane).
        // Round-trip holds only for points that lie on the plane.
        // Test the identity: re-projecting the restored points gives the same 2D.
        var (pts2D2, _, _) = PlaneProjector.Project(restored, normal, origin);
        for (int i = 0; i < pts2D.Count; i++)
        {
            Assert.Equal(pts2D[i].X, pts2D2[i].X, 3);
            Assert.Equal(pts2D[i].Y, pts2D2[i].Y, 3);
        }
    }

    [Fact]
    public void RoundTrip_PointsOnPlane_ExactRestore()
    {
        // Construct points that lie exactly on the tilted plane
        var normal = Vector3.Normalize(new Vector3(0, 1, 1));
        var origin = new Vector3(2, 0, 0);
        var (u, v) = GeoModeler3D.Core.Entities.PlaneEntity.ComputeTangents(normal);

        // Create points on the plane
        var pts3D = new List<Vector3>
        {
            origin + 2f * u + 3f * v,
            origin - 1f * u + 0.5f * v,
            origin
        };

        var (pts2D, u2, v2) = PlaneProjector.Project(pts3D, normal, origin);
        var restored = PlaneProjector.Lift(pts2D, u2, v2, origin);

        for (int i = 0; i < pts3D.Count; i++)
            AssertNearlyEqual(pts3D[i], restored[i]);
    }

    // ── helper ────────────────────────────────────────────────────────────────

    private static void AssertNearlyEqual(Vector3 expected, Vector3 actual)
    {
        Assert.Equal(expected.X, actual.X, 3);
        Assert.Equal(expected.Y, actual.Y, 3);
        Assert.Equal(expected.Z, actual.Z, 3);
    }
}
