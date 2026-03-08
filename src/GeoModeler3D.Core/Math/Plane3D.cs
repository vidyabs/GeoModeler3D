using System.Numerics;

namespace GeoModeler3D.Core.Math;

public readonly struct Plane3D : IEquatable<Plane3D>
{
    public Vector3 Origin { get; }
    public Vector3 Normal { get; }

    public Plane3D(Vector3 origin, Vector3 normal)
    {
        Origin = origin;
        Normal = Vector3.Normalize(normal);
    }

    public float DistanceToPoint(Vector3 point) =>
        Vector3.Dot(point - Origin, Normal);

    public PlaneSide ClassifySide(Vector3 point)
    {
        var dist = DistanceToPoint(point);
        if (dist > MathConstants.Tolerance) return PlaneSide.Positive;
        if (dist < -MathConstants.Tolerance) return PlaneSide.Negative;
        return PlaneSide.OnPlane;
    }

    public Vector3? LineIntersection(Vector3 lineStart, Vector3 lineEnd)
    {
        var direction = lineEnd - lineStart;
        var denom = Vector3.Dot(Normal, direction);

        if (System.Math.Abs(denom) < MathConstants.Tolerance)
            return null;

        var t = Vector3.Dot(Origin - lineStart, Normal) / denom;
        if (t < 0 || t > 1)
            return null;

        return lineStart + t * direction;
    }

    public bool Equals(Plane3D other) => Origin == other.Origin && Normal == other.Normal;
    public override bool Equals(object? obj) => obj is Plane3D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Origin, Normal);
    public static bool operator ==(Plane3D left, Plane3D right) => left.Equals(right);
    public static bool operator !=(Plane3D left, Plane3D right) => !left.Equals(right);
}

public enum PlaneSide
{
    Positive,
    Negative,
    OnPlane
}
