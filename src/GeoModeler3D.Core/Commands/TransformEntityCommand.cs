using System.Numerics;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Commands;

public class TransformEntityCommand : IUndoableCommand
{
    private readonly IGeometricEntity _entity;
    private readonly Matrix4x4 _forwardMatrix;
    private readonly Matrix4x4 _inverseMatrix;

    public TransformEntityCommand(IGeometricEntity entity, Matrix4x4 forwardMatrix)
    {
        _entity = entity;
        _forwardMatrix = forwardMatrix;

        if (!Matrix4x4.Invert(forwardMatrix, out _inverseMatrix))
            _inverseMatrix = Matrix4x4.Identity;
    }

    public string Description => $"Transform {_entity.Name}";

    public void Execute() => _entity.Transform(_forwardMatrix);

    public void Undo() => _entity.Transform(_inverseMatrix);
}
