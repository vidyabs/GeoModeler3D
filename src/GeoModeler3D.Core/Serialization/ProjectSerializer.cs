using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.Core.Serialization;

/// <summary>Serializes and deserializes entire project files.</summary>
public class ProjectSerializer
{
    public void Save(SceneManager scene, string filePath)
    {
        // TODO: serialize all entities to JSON and write to file
    }

    public void Load(SceneManager scene, string filePath)
    {
        // TODO: read file, deserialize entities, populate scene
    }
}
