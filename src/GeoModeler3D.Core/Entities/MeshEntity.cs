using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

/// <summary>
/// A triangulated mesh imported from an external file (.stl, .obj, .wrl).
/// Stores vertices as a flat array where every three consecutive entries form one triangle.
/// </summary>
public class MeshEntity : EntityBase
{
    private Vector3[] _positions;

    /// <param name="positions">
    /// Flat vertex array: positions[i*3], positions[i*3+1], positions[i*3+2] are the
    /// three vertices of the i-th triangle. Length must be a multiple of 3.
    /// </param>
    public MeshEntity(Vector3[] positions, string name = "Mesh", EntityColor? color = null)
        : base(name, color ?? EntityColor.Cyan)
    {
        _positions = positions.Length % 3 == 0
            ? positions
            : throw new ArgumentException("Positions array length must be a multiple of 3.", nameof(positions));
    }

    /// <summary>Flat vertex array (read-only). Every three consecutive entries form one triangle.</summary>
    public IReadOnlyList<Vector3> Positions => _positions;

    /// <summary>Number of triangles in this mesh.</summary>
    public int TriangleCount => _positions.Length / 3;

    public override void Transform(Matrix4x4 matrix)
    {
        for (int i = 0; i < _positions.Length; i++)
            _positions[i] = Vector3.Transform(_positions[i], matrix);
        OnPropertyChanged(nameof(Positions));
        OnPropertyChanged(nameof(BoundingBox));
    }

    public override IGeometricEntity Clone()
    {
        var clone = new MeshEntity((Vector3[])_positions.Clone(), Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox() =>
        BoundingBox3D.FromPoints(_positions);
}
