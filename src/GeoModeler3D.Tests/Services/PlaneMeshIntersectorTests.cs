using System.Numerics;
using GeoModeler3D.Core.Math;
using GeoModeler3D.Core.Services;
using Xunit;

namespace GeoModeler3D.Tests.Services;

public class PlaneMeshIntersectorTests
{
    private static Plane3D HorizontalPlane(float z) =>
        new(new Vector3(0, 0, z), Vector3.UnitZ);

    [Fact]
    public void SingleTriangle_CrossesPlane_ReturnsOneSegment()
    {
        // Triangle straddles z=0: one vertex above, two below
        var positions = new Vector3[]
        {
            new(0, 0, 1), new(1, 0, -1), new(-1, 0, -1)
        };
        var plane = HorizontalPlane(0);

        var segs = PlaneMeshIntersector.Intersect(plane, positions);

        Assert.Single(segs);
        Assert.Equal(0, segs[0].A.Z, 3);
        Assert.Equal(0, segs[0].B.Z, 3);
    }

    [Fact]
    public void TriangleEntirelyAbovePlane_ReturnsNoSegments()
    {
        var positions = new Vector3[]
        {
            new(0, 0, 1), new(1, 0, 2), new(-1, 0, 3)
        };
        var segs = PlaneMeshIntersector.Intersect(HorizontalPlane(0), positions);
        Assert.Empty(segs);
    }

    [Fact]
    public void TriangleEntirelyBelowPlane_ReturnsNoSegments()
    {
        var positions = new Vector3[]
        {
            new(0, 0, -1), new(1, 0, -2), new(-1, 0, -3)
        };
        var segs = PlaneMeshIntersector.Intersect(HorizontalPlane(0), positions);
        Assert.Empty(segs);
    }

    [Fact]
    public void TwoTriangles_BothCross_ReturnsTwoSegments()
    {
        var positions = new Vector3[]
        {
            // Triangle 1
            new(0, 0, 1), new(1, 0, -1), new(-1, 0, -1),
            // Triangle 2 (shifted in Y)
            new(0, 2, 1), new(1, 2, -1), new(-1, 2, -1)
        };
        var segs = PlaneMeshIntersector.Intersect(HorizontalPlane(0), positions);
        Assert.Equal(2, segs.Count);
    }

    [Fact]
    public void TriangleEdgeOnPlane_ReturnsDegenerate_NotCrash()
    {
        // An edge exactly on z=0 — should produce at most one segment, not crash
        var positions = new Vector3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1)
        };
        // Just verify it does not throw
        var segs = PlaneMeshIntersector.Intersect(HorizontalPlane(0), positions);
        Assert.True(segs.Count >= 0);
    }
}
