using System.Numerics;

namespace GeoModeler3D.Core.Math;

public static class GeometryUtils
{
    public static float AngleBetweenVectors(Vector3 a, Vector3 b)
    {
        var dot = Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b));
        dot = System.Math.Clamp(dot, -1f, 1f);
        return MathF.Acos(dot);
    }

    public static Vector3 ProjectPointOntoPlane(Vector3 point, Plane3D plane)
    {
        var dist = plane.DistanceToPoint(point);
        return point - dist * plane.Normal;
    }

    public static Vector3? LinePlaneIntersection(Vector3 lineStart, Vector3 lineDirection, Plane3D plane)
    {
        var denom = Vector3.Dot(plane.Normal, lineDirection);
        if (System.Math.Abs(denom) < MathConstants.Tolerance)
            return null;

        var t = Vector3.Dot(plane.Origin - lineStart, plane.Normal) / denom;
        return lineStart + t * lineDirection;
    }

    public static Vector3 ComputeTriangleNormal(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var edge1 = v1 - v0;
        var edge2 = v2 - v0;
        return Vector3.Normalize(Vector3.Cross(edge1, edge2));
    }

    public static float ComputeTriangleArea(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var cross = Vector3.Cross(v1 - v0, v2 - v0);
        return cross.Length() * 0.5f;
    }
}
