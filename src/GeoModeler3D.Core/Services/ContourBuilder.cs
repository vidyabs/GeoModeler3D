using System.Numerics;

namespace GeoModeler3D.Core.Services;

/// <summary>
/// Stitches a flat list of (A, B) line segments into ordered polyline chains.
/// </summary>
public static class ContourBuilder
{
    private const float DefaultTolerance = 1e-4f;

    /// <summary>
    /// Returns one record per connected chain.
    /// IsClosed = true when the last point reconnects to the first within tolerance.
    /// </summary>
    public static List<(List<Vector3> Points, bool IsClosed)> Build(
        List<(Vector3 A, Vector3 B)> segments,
        float tolerance = DefaultTolerance)
    {
        var result = new List<(List<Vector3>, bool)>();
        if (segments.Count == 0) return result;

        var used = new bool[segments.Count];
        float tol2 = tolerance * tolerance;

        while (true)
        {
            int startIdx = FindUnused(used);
            if (startIdx < 0) break;

            var chain = new List<Vector3> { segments[startIdx].A, segments[startIdx].B };
            used[startIdx] = true;

            // Extend forward from chain end
            while (true)
            {
                int next = FindNext(segments, used, chain[chain.Count - 1], tol2);
                if (next < 0) break;
                used[next] = true;
                var (sA, sB) = segments[next];
                chain.Add((sA - chain[chain.Count - 1]).LengthSquared() < tol2 ? sB : sA);
            }

            // Extend backward from chain start
            while (true)
            {
                int prev = FindNext(segments, used, chain[0], tol2);
                if (prev < 0) break;
                used[prev] = true;
                var (pA, pB) = segments[prev];
                chain.Insert(0, (pA - chain[0]).LengthSquared() < tol2 ? pB : pA);
            }

            bool isClosed = (chain[chain.Count - 1] - chain[0]).LengthSquared() < tol2;
            if (isClosed && chain.Count > 1)
                chain.RemoveAt(chain.Count - 1);

            if (chain.Count >= 2)
                result.Add((chain, isClosed));
        }

        return result;
    }

    private static int FindUnused(bool[] used)
    {
        for (int i = 0; i < used.Length; i++)
            if (!used[i]) return i;
        return -1;
    }

    private static int FindNext(List<(Vector3 A, Vector3 B)> segments, bool[] used, Vector3 tip, float tol2)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (used[i]) continue;
            var (a, b) = segments[i];
            if ((a - tip).LengthSquared() < tol2 || (b - tip).LengthSquared() < tol2)
                return i;
        }
        return -1;
    }
}
