using System.Numerics;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Operations;

/// <summary>Applies geometric transforms to entities (translate, rotate, scale).</summary>
public class TransformOperation
{
    public void Translate(IGeometricEntity entity, Vector3 offset)
    {
        entity.Transform(Matrix4x4.CreateTranslation(offset));
    }

    public void Rotate(IGeometricEntity entity, Vector3 axis, float angleDegrees)
    {
        // TODO: implement rotation around axis
    }

    public void Scale(IGeometricEntity entity, float factor)
    {
        // TODO: implement uniform scaling
    }
}
