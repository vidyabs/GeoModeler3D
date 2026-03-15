using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using GeoModeler3D.Core.Commands;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.App.ViewModels;

public partial class PropertiesPanelViewModel : ObservableObject
{
    private readonly SceneManager _sceneManager;
    private readonly SelectionManager _selectionManager;
    private readonly UndoManager _undoManager;

    [ObservableProperty]
    private IGeometricEntity? _selectedEntity;

    [ObservableProperty]
    private string _entityTypeName = string.Empty;

    public PropertiesPanelViewModel(SceneManager sceneManager, SelectionManager selectionManager, UndoManager undoManager)
    {
        _sceneManager = sceneManager;
        _selectionManager = selectionManager;
        _undoManager = undoManager;
        _selectionManager.SelectionChanged += OnSelectionChanged;
    }

    /// <summary>
    /// Called by the view code-behind when the user commits an edit.
    /// propertyPath is either "PropName" (scalar/string) or "VecProp.X" (Vector3 component).
    /// </summary>
    public void CommitEdit(string? propertyPath, string? newValue)
    {
        if (SelectedEntity is null || string.IsNullOrEmpty(propertyPath) || newValue is null)
            return;

        try
        {
            if (propertyPath.Contains('.'))
                CommitVector3ComponentEdit(propertyPath, newValue);
            else if (propertyPath == "Color")
                CommitColorEdit(newValue);
            else
                CommitScalarOrStringEdit(propertyPath, newValue);
        }
        catch
        {
            // Silently ignore parse errors — the TextBox will revert on next refresh
        }
    }

    private void CommitVector3ComponentEdit(string propertyPath, string newValue)
    {
        var parts = propertyPath.Split('.'); // e.g. ["Center", "X"]
        if (parts.Length != 2) return;

        var propName = parts[0];
        var component = parts[1].ToUpperInvariant();

        var propInfo = SelectedEntity!.GetType().GetProperty(propName);
        if (propInfo?.PropertyType != typeof(Vector3)) return;

        if (!float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var newComponent))
            return;

        var oldVec = (Vector3)propInfo.GetValue(SelectedEntity)!;
        var newVec = component switch
        {
            "X" => new Vector3(newComponent, oldVec.Y, oldVec.Z),
            "Y" => new Vector3(oldVec.X, newComponent, oldVec.Z),
            "Z" => new Vector3(oldVec.X, oldVec.Y, newComponent),
            _ => oldVec
        };

        if (newVec == oldVec) return;

        var cmd = new ChangePropertyCommand<Vector3>(SelectedEntity, propName, oldVec, newVec);
        _undoManager.Execute(cmd);
    }

    private void CommitColorEdit(string newValue)
    {
        if (SelectedEntity is null) return;
        var oldColor = SelectedEntity.Color;
        var newColor = EntityColor.FromHex(newValue);
        if (newColor == oldColor) return;

        var cmd = new ChangePropertyCommand<EntityColor>(SelectedEntity, "Color", oldColor, newColor);
        _undoManager.Execute(cmd);
    }

    private void CommitScalarOrStringEdit(string propertyName, string newValue)
    {
        var propInfo = SelectedEntity!.GetType().GetProperty(propertyName)
                    ?? typeof(IGeometricEntity).GetProperty(propertyName);
        if (propInfo is null || !propInfo.CanWrite) return;

        var propType = propInfo.PropertyType;

        if (propType == typeof(double))
        {
            if (!double.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var newDouble))
                return;
            var oldDouble = (double)propInfo.GetValue(SelectedEntity)!;
            if (Math.Abs(newDouble - oldDouble) < 1e-9) return;
            var cmd = new ChangePropertyCommand<double>(SelectedEntity, propertyName, oldDouble, newDouble);
            _undoManager.Execute(cmd);
        }
        else if (propType == typeof(string))
        {
            var oldString = (string?)propInfo.GetValue(SelectedEntity) ?? string.Empty;
            if (oldString == newValue) return;
            var cmd = new ChangePropertyCommand<string>(SelectedEntity, propertyName, oldString, newValue);
            _undoManager.Execute(cmd);
        }
    }

    private void OnSelectionChanged()
    {
        if (SelectedEntity is INotifyPropertyChanged oldEntity)
            oldEntity.PropertyChanged -= OnEntityPropertyChanged;

        if (_selectionManager.SelectedIds.Count > 0)
        {
            SelectedEntity = _sceneManager.GetById(_selectionManager.SelectedIds[0]);
            EntityTypeName = SelectedEntity?.GetType().Name.Replace("Entity", "") ?? string.Empty;
        }
        else
        {
            SelectedEntity = null;
            EntityTypeName = string.Empty;
        }

        if (SelectedEntity is INotifyPropertyChanged newEntity)
            newEntity.PropertyChanged += OnEntityPropertyChanged;
    }

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedEntity));
    }
}
