using System.Reflection;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Commands;

public class ChangePropertyCommand<T> : IUndoableCommand
{
    private readonly IGeometricEntity _entity;
    private readonly string _propertyName;
    private readonly T _oldValue;
    private readonly T _newValue;
    private readonly PropertyInfo _propertyInfo;

    public ChangePropertyCommand(IGeometricEntity entity, string propertyName, T oldValue, T newValue)
    {
        _entity = entity;
        _propertyName = propertyName;
        _oldValue = oldValue;
        _newValue = newValue;
        _propertyInfo = entity.GetType().GetProperty(propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {entity.GetType().Name}");
    }

    public string Description => $"Change {_propertyName} of {_entity.Name}";

    public void Execute() => _propertyInfo.SetValue(_entity, _newValue);

    public void Undo() => _propertyInfo.SetValue(_entity, _oldValue);
}
