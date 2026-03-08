using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.Core.Operations;

/// <summary>Duplicates entities in the scene.</summary>
public class DuplicateOperation
{
    public IGeometricEntity Duplicate(IGeometricEntity entity, SceneManager scene)
    {
        var clone = entity.Clone();
        clone.Name = entity.Name + " (Copy)";
        scene.Add(clone);
        return clone;
    }
}
