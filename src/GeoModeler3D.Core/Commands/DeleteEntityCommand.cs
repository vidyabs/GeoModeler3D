using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.Core.Commands;

public class DeleteEntityCommand : IUndoableCommand
{
    private readonly SceneManager _scene;
    private readonly IGeometricEntity _entity;
    private readonly int _originalIndex;

    public DeleteEntityCommand(SceneManager scene, IGeometricEntity entity)
    {
        _scene = scene;
        _entity = entity;
        _originalIndex = scene.IndexOf(entity.Id);
    }

    public string Description => $"Delete {_entity.Name}";

    public void Execute() => _scene.Remove(_entity.Id);

    public void Undo() => _scene.Insert(_originalIndex, _entity);
}
