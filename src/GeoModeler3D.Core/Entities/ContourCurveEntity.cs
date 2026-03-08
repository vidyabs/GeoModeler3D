using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class ContourCurveEntity : EntityBase
{
    private bool _isClosed;
    private ConicSectionType? _conicType;

    public ContourCurveEntity(List<Vector3> points, Guid sourcePlaneId, Guid sourceEntityId,
        string name = "Contour", EntityColor? color = null)
        : base(name, color ?? EntityColor.Yellow)
    {
        Points = points;
        SourcePlaneId = sourcePlaneId;
        SourceEntityId = sourceEntityId;
    }

    public List<Vector3> Points { get; }
    public Guid SourcePlaneId { get; }
    public Guid SourceEntityId { get; }

    public bool IsClosed
    {
        get => _isClosed;
        set => SetField(ref _isClosed, value);
    }

    public ConicSectionType? ConicType
    {
        get => _conicType;
        set => SetField(ref _conicType, value);
    }

    public override void Transform(Matrix4x4 matrix)
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] = Vector3.Transform(Points[i], matrix);
        }
        OnPropertyChanged(nameof(Points));
    }

    public override IGeometricEntity Clone()
    {
        var clone = new ContourCurveEntity(
            new List<Vector3>(Points), SourcePlaneId, SourceEntityId, Name, Color)
        {
            IsClosed = _isClosed,
            ConicType = _conicType
        };
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox()
    {
        if (Points.Count == 0)
            return new BoundingBox3D(Vector3.Zero, Vector3.Zero);
        return BoundingBox3D.FromPoints(Points);
    }
}
