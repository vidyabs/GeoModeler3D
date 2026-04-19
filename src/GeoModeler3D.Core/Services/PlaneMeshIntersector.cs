using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Services;

/// <summary>
/// Intersects a triangle mesh (flat vertex array, every 3 = one triangle) with a plane.
/// Returns the raw edge segments found; use ContourBuilder to stitch them into chains.
/// </summary>
public static class PlaneMeshIntersector
{
    public static List<(Vector3 A, Vector3 B)> Intersect(Plane3D plane, IReadOnlyList<Vector3> positions)
    {
        var segments = new List<(Vector3, Vector3)>(positions.Count / 6);

        int triCount = positions.Count / 3;
        for (int i = 0; i < triCount; i++)
        {
            var v0 = positions[i * 3];
            var v1 = positions[i * 3 + 1];
            var v2 = positions[i * 3 + 2];

            if (TryIntersectTriangle(plane, v0, v1, v2, out var a, out var b))
                segments.Add((a, b));
        }

        return segments;
    }

    private static bool TryIntersectTriangle(
        Plane3D plane, Vector3 v0, Vector3 v1, Vector3 v2,
        out Vector3 a, out Vector3 b)
    {
        a = b = Vector3.Zero;

        float d0 = plane.DistanceToPoint(v0);
        float d1 = plane.DistanceToPoint(v1);
        float d2 = plane.DistanceToPoint(v2);

        var pts = new List<Vector3>(2);
        AddCrossing(v0, d0, v1, d1, pts);
        AddCrossing(v1, d1, v2, d2, pts);
        AddCrossing(v2, d2, v0, d0, pts);

        if (pts.Count < 2) return false;

        a = pts[0];
        b = pts[pts.Count - 1]; // use last in case of 3 (vertex on plane)
        return (a - b).LengthSquared() > 1e-12f;
    }

    private static void AddCrossing(Vector3 vA, float dA, Vector3 vB, float dB, List<Vector3> pts)
    {
        const float tol = 1e-7f;
        bool aOnPlane = MathF.Abs(dA) <= tol;
        bool bOnPlane = MathF.Abs(dB) <= tol;

        if (aOnPlane)
        {
            pts.Add(vA);
            return;
        }
        if (bOnPlane) return; // will be picked up by the next edge starting at vB

        // Genuine crossing: different non-zero signs
        if ((dA > 0) == (dB > 0)) return;

        float t = dA / (dA - dB);
        pts.Add(Vector3.Lerp(vA, vB, t));
    }
}
