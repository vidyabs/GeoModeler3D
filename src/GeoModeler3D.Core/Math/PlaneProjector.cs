using System.Numerics;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Math;

/// <summary>
/// Projects 3-D points onto a plane's local (u, v) coordinate frame and back.
/// Uses the same tangent basis as <see cref="PlaneEntity.ComputeTangents"/>.
/// </summary>
public static class PlaneProjector
{
    /// <summary>
    /// Projects <paramref name="points3D"/> onto the 2-D (u, v) frame of the plane
    /// defined by <paramref name="planeNormal"/> and <paramref name="planeOrigin"/>.
    /// </summary>
    /// <returns>
    /// A tuple of:
    /// <list type="bullet">
    ///   <item><term>Points2D</term> — projected coordinates</item>
    ///   <item><term>U</term> — first tangent vector (world space)</item>
    ///   <item><term>V</term> — second tangent vector (world space)</item>
    /// </list>
    /// The (U, V) pair is needed to lift the points back to 3-D.
    /// </returns>
    public static (List<Vector2> Points2D, Vector3 U, Vector3 V) Project(
        IList<Vector3> points3D, Vector3 planeNormal, Vector3 planeOrigin)
    {
        var (u, v) = PlaneEntity.ComputeTangents(planeNormal);

        var result = new List<Vector2>(points3D.Count);
        foreach (var p in points3D)
        {
            var offset = p - planeOrigin;
            result.Add(new Vector2(Vector3.Dot(offset, u), Vector3.Dot(offset, v)));
        }

        return (result, u, v);
    }

    /// <summary>
    /// Lifts 2-D projected coordinates back to 3-D world space using the tangent
    /// frame (U, V) and <paramref name="planeOrigin"/> returned by <see cref="Project"/>.
    /// </summary>
    public static List<Vector3> Lift(
        IList<Vector2> points2D, Vector3 u, Vector3 v, Vector3 planeOrigin)
    {
        var result = new List<Vector3>(points2D.Count);
        foreach (var p in points2D)
            result.Add(planeOrigin + p.X * u + p.Y * v);
        return result;
    }
}
