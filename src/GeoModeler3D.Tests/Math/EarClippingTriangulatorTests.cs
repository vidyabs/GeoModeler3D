using System.Numerics;
using GeoModeler3D.Core.Math;
using Xunit;

namespace GeoModeler3D.Tests.Math;

public class EarClippingTriangulatorTests
{
    // ── degenerate / edge cases ───────────────────────────────────────────────

    [Fact]
    public void Empty_ReturnsEmpty()
    {
        var result = EarClippingTriangulator.Triangulate([]);
        Assert.Empty(result);
    }

    [Fact]
    public void TwoPoints_ReturnsEmpty()
    {
        var result = EarClippingTriangulator.Triangulate(
            [new Vector2(0, 0), new Vector2(1, 0)]);
        Assert.Empty(result);
    }

    // ── single triangle ───────────────────────────────────────────────────────

    [Fact]
    public void Triangle_CCW_ReturnsOneTriangle()
    {
        // CCW triangle
        var poly = new List<Vector2>
        {
            new(0, 0), new(1, 0), new(0, 1)
        };
        var indices = EarClippingTriangulator.Triangulate(poly);

        Assert.Equal(3, indices.Count);
        AssertValidTriangulation(poly, indices, expectedTriCount: 1);
    }

    [Fact]
    public void Triangle_CW_ReturnsOneTriangle()
    {
        // CW triangle — triangulator should normalise winding internally
        var poly = new List<Vector2>
        {
            new(0, 0), new(0, 1), new(1, 0)
        };
        var indices = EarClippingTriangulator.Triangulate(poly);

        Assert.Equal(3, indices.Count);
    }

    // ── convex polygons ───────────────────────────────────────────────────────

    [Fact]
    public void ConvexSquare_ReturnsTwoTriangles()
    {
        var poly = new List<Vector2>
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };
        var indices = EarClippingTriangulator.Triangulate(poly);

        Assert.Equal(6, indices.Count);
        AssertValidTriangulation(poly, indices, expectedTriCount: 2);
    }

    [Fact]
    public void ConvexPentagon_ReturnsThreeTriangles()
    {
        // Regular pentagon (CCW)
        var poly = new List<Vector2>();
        for (int i = 0; i < 5; i++)
        {
            float angle = 2 * MathF.PI * i / 5;
            poly.Add(new Vector2(MathF.Cos(angle), MathF.Sin(angle)));
        }

        var indices = EarClippingTriangulator.Triangulate(poly);

        Assert.Equal(9, indices.Count);
        AssertValidTriangulation(poly, indices, expectedTriCount: 3);
    }

    [Fact]
    public void ConvexHexagon_ReturnsFourTriangles()
    {
        var poly = new List<Vector2>();
        for (int i = 0; i < 6; i++)
        {
            float angle = 2 * MathF.PI * i / 6;
            poly.Add(new Vector2(MathF.Cos(angle), MathF.Sin(angle)));
        }

        var indices = EarClippingTriangulator.Triangulate(poly);

        Assert.Equal(12, indices.Count);
        AssertValidTriangulation(poly, indices, expectedTriCount: 4);
    }

    // ── concave polygon ───────────────────────────────────────────────────────

    [Fact]
    public void ConcaveLShape_TriangulatesWithoutError()
    {
        // L-shape (CCW)
        //   (0,2)─(1,2)
        //     │       │
        //   (0,1)─(1,1)─(2,1)
        //     │               │
        //   (0,0)──────(2,0)
        var poly = new List<Vector2>
        {
            new(0, 0), new(2, 0), new(2, 1),
            new(1, 1), new(1, 2), new(0, 2)
        };

        var indices = EarClippingTriangulator.Triangulate(poly);

        // n=6 → 4 triangles, 12 indices
        Assert.Equal(12, indices.Count);
        AssertValidTriangulation(poly, indices, expectedTriCount: 4);
    }

    // ── circle-approximation (as used by contour curves) ─────────────────────

    [Fact]
    public void Circle64Points_TriangulatesCompletely()
    {
        int n = 64;
        var poly = new List<Vector2>(n);
        for (int i = 0; i < n; i++)
        {
            float angle = 2 * MathF.PI * i / n;
            poly.Add(new Vector2(MathF.Cos(angle), MathF.Sin(angle)));
        }

        var indices = EarClippingTriangulator.Triangulate(poly);

        Assert.Equal((n - 2) * 3, indices.Count);
        AssertValidTriangulation(poly, indices, expectedTriCount: n - 2);
    }

    // ── invariant: all produced indices are in bounds ─────────────────────────

    private static void AssertValidTriangulation(
        IList<Vector2> polygon, IList<int> indices, int expectedTriCount)
    {
        Assert.Equal(expectedTriCount * 3, indices.Count);
        foreach (var idx in indices)
            Assert.InRange(idx, 0, polygon.Count - 1);
    }
}
