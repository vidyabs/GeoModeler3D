using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class TriangleEntity : EntityBase
{
    private Vector3 _vertex0;
    private Vector3 _vertex1;
    private Vector3 _vertex2;

    public TriangleEntity(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
        string name = "Triangle", EntityColor? color = null)
        : base(name, color ?? EntityColor.Cyan)
    {
        _vertex0 = vertex0;
        _vertex1 = vertex1;
        _vertex2 = vertex2;
    }

    public Vector3 Vertex0
    {
        get => _vertex0;
        set { SetField(ref _vertex0, value); OnPropertyChanged(nameof(Normal)); OnPropertyChanged(nameof(Area)); }
    }

    public Vector3 Vertex1
    {
        get => _vertex1;
        set { SetField(ref _vertex1, value); OnPropertyChanged(nameof(Normal)); OnPropertyChanged(nameof(Area)); }
    }

    public Vector3 Vertex2
    {
        get => _vertex2;
        set { SetField(ref _vertex2, value); OnPropertyChanged(nameof(Normal)); OnPropertyChanged(nameof(Area)); }
    }

    public Vector3 Normal => GeometryUtils.ComputeTriangleNormal(_vertex0, _vertex1, _vertex2);
    public float Area => GeometryUtils.ComputeTriangleArea(_vertex0, _vertex1, _vertex2);

    public override void Transform(Matrix4x4 matrix)
    {
        _vertex0 = Vector3.Transform(_vertex0, matrix);
        _vertex1 = Vector3.Transform(_vertex1, matrix);
        _vertex2 = Vector3.Transform(_vertex2, matrix);
        OnPropertyChanged(nameof(Vertex0));
        OnPropertyChanged(nameof(Vertex1));
        OnPropertyChanged(nameof(Vertex2));
        OnPropertyChanged(nameof(Normal));
        OnPropertyChanged(nameof(Area));
    }

    public override IGeometricEntity Clone()
    {
        var clone = new TriangleEntity(_vertex0, _vertex1, _vertex2, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox() =>
        BoundingBox3D.FromPoints([_vertex0, _vertex1, _vertex2]);
}
