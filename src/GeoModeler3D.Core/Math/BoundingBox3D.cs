using System.Numerics;

namespace GeoModeler3D.Core.Math;

public readonly struct BoundingBox3D : IEquatable<BoundingBox3D>
{
    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public BoundingBox3D(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public static BoundingBox3D FromPoint(Vector3 point) => new(point, point);

    public static BoundingBox3D FromPoints(IEnumerable<Vector3> points)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var p in points)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        return new BoundingBox3D(min, max);
    }

    public Vector3 Center => (Min + Max) * 0.5f;
    public Vector3 Size => Max - Min;

    public BoundingBox3D Merge(BoundingBox3D other) =>
        new(Vector3.Min(Min, other.Min), Vector3.Max(Max, other.Max));

    public BoundingBox3D Expand(Vector3 point) =>
        new(Vector3.Min(Min, point), Vector3.Max(Max, point));

    public bool Contains(Vector3 point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y &&
        point.Z >= Min.Z && point.Z <= Max.Z;

    public bool Equals(BoundingBox3D other) => Min == other.Min && Max == other.Max;
    public override bool Equals(object? obj) => obj is BoundingBox3D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Min, Max);
    public static bool operator ==(BoundingBox3D left, BoundingBox3D right) => left.Equals(right);
    public static bool operator !=(BoundingBox3D left, BoundingBox3D right) => !left.Equals(right);
}
