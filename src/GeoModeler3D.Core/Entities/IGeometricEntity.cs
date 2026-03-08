using System.ComponentModel;
using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public interface IGeometricEntity : INotifyPropertyChanged
{
    Guid Id { get; }
    string Name { get; set; }
    EntityColor Color { get; set; }
    bool IsVisible { get; set; }
    string Layer { get; set; }
    BoundingBox3D BoundingBox { get; }

    void Transform(Matrix4x4 matrix);
    IGeometricEntity Clone();
    void Accept(IEntityVisitor visitor);
}
