using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.Core.Commands;

public class CreateEntityCommand : IUndoableCommand
{
    private readonly SceneManager _scene;
    private readonly IGeometricEntity _entity;

    public CreateEntityCommand(SceneManager scene, IGeometricEntity entity)
    {
        _scene = scene;
        _entity = entity;
    }

    public string Description => $"Create {_entity.Name}";

    public void Execute() => _scene.Add(_entity);

    public void Undo() => _scene.Remove(_entity.Id);
}
