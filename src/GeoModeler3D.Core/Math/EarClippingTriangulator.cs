using System.Numerics;

namespace GeoModeler3D.Core.Math;

/// <summary>
/// Triangulates a simple (non-self-intersecting) 2-D polygon using the ear-clipping algorithm.
/// Handles both convex and simple concave polygons. Does not support holes.
/// </summary>
public static class EarClippingTriangulator
{
    /// <summary>
    /// Returns a flat list of triangle indices (every three ints = one CCW triangle)
    /// into the original <paramref name="polygon"/> array.
    /// Returns an empty list if <paramref name="polygon"/> has fewer than 3 points.
    /// </summary>
    public static List<int> Triangulate(IList<Vector2> polygon)
    {
        var result = new List<int>();
        int n = polygon.Count;
        if (n < 3) return result;

        // Degenerate: single triangle
        if (n == 3)
        {
            result.AddRange([0, 1, 2]);
            return result;
        }

        // Working index list; we'll remove ears one by one
        var indices = Enumerable.Range(0, n).ToList();

        // Ensure CCW winding — if signed area is negative the polygon is CW, reverse it.
        if (SignedArea(polygon) < 0f)
            indices.Reverse();

        // Guard against degenerate input (all collinear, etc.)
        int maxIterations = n * n;
        int iteration = 0;

        while (indices.Count > 3 && iteration++ < maxIterations)
        {
            bool earFound = false;

            for (int i = 0; i < indices.Count; i++)
            {
                int prevIdx = indices[(i - 1 + indices.Count) % indices.Count];
                int currIdx = indices[i];
                int nextIdx = indices[(i + 1) % indices.Count];

                if (!IsEar(polygon, indices, prevIdx, currIdx, nextIdx)) continue;

                result.Add(prevIdx);
                result.Add(currIdx);
                result.Add(nextIdx);
                indices.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound) break; // polygon is degenerate
        }

        // Add the final triangle
        if (indices.Count == 3)
        {
            result.Add(indices[0]);
            result.Add(indices[1]);
            result.Add(indices[2]);
        }

        return result;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>Positive for CCW, negative for CW.</summary>
    private static float SignedArea(IList<Vector2> poly)
    {
        float area = 0f;
        int n = poly.Count;
        for (int i = 0; i < n; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % n];
            area += a.X * b.Y - b.X * a.Y;
        }
        return area * 0.5f;
    }

    /// <summary>Returns true if vertex <paramref name="curr"/> is an ear of the polygon.</summary>
    private static bool IsEar(IList<Vector2> poly, List<int> indices,
        int prev, int curr, int next)
    {
        var a = poly[prev];
        var b = poly[curr];
        var c = poly[next];

        // Triangle must be convex (positive cross product for CCW winding)
        if (Cross2D(b - a, c - b) <= 0f) return false;

        // No other polygon vertex may lie strictly inside the triangle
        foreach (int idx in indices)
        {
            if (idx == prev || idx == curr || idx == next) continue;
            if (PointInTriangle(poly[idx], a, b, c)) return false;
        }

        return true;
    }

    /// <summary>2-D cross product (z-component of 3-D cross).</summary>
    private static float Cross2D(Vector2 u, Vector2 v) => u.X * v.Y - u.Y * v.X;

    /// <summary>
    /// Returns true if <paramref name="p"/> is inside or on the boundary of triangle (a, b, c).
    /// The triangle is assumed CCW.
    /// </summary>
    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross2D(p - a, b - a);
        float d2 = Cross2D(p - b, c - b);
        float d3 = Cross2D(p - c, a - c);

        bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
        bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;

        return !(hasNeg && hasPos); // all same sign (including zero) → inside or on edge
    }
}
