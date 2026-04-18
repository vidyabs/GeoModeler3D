using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeoModeler3D.Core.Commands;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Import;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Core.Serialization;
using GeoModeler3D.App.Services;

namespace GeoModeler3D.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SceneManager _sceneManager;
    private readonly SelectionManager _selectionManager;
    private readonly UndoManager _undoManager;
    private readonly IDialogService _dialogService;
    private readonly IFileDialogService _fileDialogService;
    private readonly ProjectSerializer _projectSerializer;
    private string? _currentFilePath;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private string _title = "GeoModeler3D";

    public MainViewModel(
        SceneManager sceneManager,
        SelectionManager selectionManager,
        UndoManager undoManager,
        IDialogService dialogService,
        IFileDialogService fileDialogService,
        ProjectSerializer projectSerializer)
    {
        _sceneManager = sceneManager;
        _selectionManager = selectionManager;
        _undoManager = undoManager;
        _dialogService = dialogService;
        _fileDialogService = fileDialogService;
        _projectSerializer = projectSerializer;

        _undoManager.StackChanged += OnUndoStackChanged;
        _selectionManager.SelectionChanged += OnSelectionChanged;
        _sceneManager.EntityAdded += _ => OnPropertyChanged(nameof(EntityCount));
        _sceneManager.EntityRemoved += _ => OnPropertyChanged(nameof(EntityCount));
    }

    public SceneManager SceneManager => _sceneManager;
    public SelectionManager SelectionManager => _selectionManager;
    public UndoManager UndoManager => _undoManager;

    public ObservableCollection<IGeometricEntity> Entities => _sceneManager.Entities;
    public int EntityCount => _sceneManager.Entities.Count;

    public bool CanUndo => _undoManager.CanUndo;
    public bool CanRedo => _undoManager.CanRedo;
    public string? UndoDescription => _undoManager.UndoDescription;
    public string? RedoDescription => _undoManager.RedoDescription;

    public IGeometricEntity? SelectedEntity =>
        _selectionManager.SelectedIds.Count > 0
            ? _sceneManager.GetById(_selectionManager.SelectedIds[0])
            : null;

    [RelayCommand]
    private void NewScene()
    {
        _selectionManager.ClearSelection();
        _sceneManager.Clear();
        _undoManager.Clear();
        _currentFilePath = null;
        Title = "GeoModeler3D";
        StatusText = "New scene created";
    }

    [RelayCommand]
    private void OpenScene()
    {
        var path = _fileDialogService.ShowOpenFileDialog(
            "GeoModeler3D Files (*.geo3d)|*.geo3d|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            "Open Scene");
        if (path is null) return;

        try
        {
            _selectionManager.ClearSelection();
            _undoManager.Clear();
            _projectSerializer.Load(_sceneManager, path);
            _currentFilePath = path;
            Title = $"GeoModeler3D - {System.IO.Path.GetFileName(path)}";
            StatusText = $"Opened: {System.IO.Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage($"Failed to open file:\n{ex.Message}", "Open Error");
        }
    }

    [RelayCommand]
    private void SaveScene()
    {
        if (_currentFilePath is not null)
        {
            try
            {
                _projectSerializer.Save(_sceneManager, _currentFilePath);
                StatusText = $"Saved: {System.IO.Path.GetFileName(_currentFilePath)}";
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Failed to save file:\n{ex.Message}", "Save Error");
            }
        }
        else
        {
            SaveSceneAs();
        }
    }

    [RelayCommand]
    private void SaveSceneAs()
    {
        var path = _fileDialogService.ShowSaveFileDialog(
            "GeoModeler3D Files (*.geo3d)|*.geo3d|JSON Files (*.json)|*.json",
            "Save Scene As");
        if (path is null) return;

        try
        {
            _projectSerializer.Save(_sceneManager, path);
            _currentFilePath = path;
            Title = $"GeoModeler3D - {System.IO.Path.GetFileName(path)}";
            StatusText = $"Saved: {System.IO.Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage($"Failed to save file:\n{ex.Message}", "Save Error");
        }
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _undoManager.Undo();
        StatusText = $"Undo: {_undoManager.RedoDescription}";
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _undoManager.Redo();
        StatusText = $"Redo: {_undoManager.UndoDescription}";
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        var ids = _selectionManager.SelectedIds.ToList();
        if (ids.Count == 0) return;

        _selectionManager.ClearSelection();
        foreach (var id in ids)
        {
            var entity = _sceneManager.GetById(id);
            if (entity is null) continue;
            var cmd = new DeleteEntityCommand(_sceneManager, entity);
            _undoManager.Execute(cmd);
        }
        StatusText = $"Deleted {ids.Count} entity(ies)";
    }

    [RelayCommand]
    private void CreateSphere(SphereCreationParams? p)
    {
        if (p is null) return;
        var entity = new SphereEntity(p.Center, p.Radius);
        var cmd = new CreateEntityCommand(_sceneManager, entity);
        _undoManager.Execute(cmd);
        StatusText = $"Created Sphere";
    }

    [RelayCommand]
    private void CreateCylinder(CylinderCreationParams? p)
    {
        if (p is null) return;
        var entity = new CylinderEntity(p.BaseCenter, p.Axis, p.Radius, p.Height);
        var cmd = new CreateEntityCommand(_sceneManager, entity);
        _undoManager.Execute(cmd);
        StatusText = $"Created Cylinder";
    }

    [RelayCommand]
    private void CreateCone(ConeCreationParams? p)
    {
        if (p is null) return;
        var entity = new ConeEntity(p.BaseCenter, p.Axis, p.BaseRadius, p.Height);
        var cmd = new CreateEntityCommand(_sceneManager, entity);
        _undoManager.Execute(cmd);
        StatusText = $"Created Cone";
    }

    [RelayCommand]
    private void CreateTorus(TorusCreationParams? p)
    {
        if (p is null) return;
        var entity = new TorusEntity(p.Center, p.Normal, p.MajorRadius, p.MinorRadius);
        var cmd = new CreateEntityCommand(_sceneManager, entity);
        _undoManager.Execute(cmd);
        StatusText = $"Created Torus";
    }

    [RelayCommand]
    private void CreatePoint(PointCreationParams? p)
    {
        if (p is null) return;
        var entity = new PointEntity(p.Position);
        var cmd = new CreateEntityCommand(_sceneManager, entity);
        _undoManager.Execute(cmd);
        StatusText = $"Created Point";
    }

    [RelayCommand]
    private void CreateTriangle(TriangleCreationParams? p)
    {
        if (p is null) return;
        var entity = new TriangleEntity(p.V0, p.V1, p.V2);
        var cmd = new CreateEntityCommand(_sceneManager, entity);
        _undoManager.Execute(cmd);
        StatusText = "Created Triangle";
    }

    [RelayCommand]
    private void CreateVector(VectorCreationParams? p)
    {
        if (p is null) return;
        var entity = new VectorEntity(p.Origin, p.Direction);
        _undoManager.Execute(new CreateEntityCommand(_sceneManager, entity));
        StatusText = "Created Vector";
    }

    [RelayCommand]
    private void CreatePlane(PlaneCreationParams? p)
    {
        if (p is null) return;
        var entity = new PlaneEntity(p.Origin, p.Normal);
        _undoManager.Execute(new CreateEntityCommand(_sceneManager, entity));
        StatusText = "Created Plane";
    }

    [RelayCommand]
    private void CreateCuttingPlane(CuttingPlaneCreationParams? p)
    {
        if (p is null) return;
        var entity = new CuttingPlaneEntity(p.Origin, p.Normal)
        {
            DisplayWidth = p.DisplayWidth,
            DisplayHeight = p.DisplayHeight,
            Opacity = p.Opacity,
            IsCappingEnabled = p.IsCappingEnabled,
            ClipSide = p.ClipSide,
            GapDistance = p.GapDistance
        };
        entity.TargetEntityIds.AddRange(p.TargetEntityIds);
        _undoManager.Execute(new CreateEntityCommand(_sceneManager, entity));
        StatusText = "Created Cutting Plane";
    }

    [RelayCommand]
    private void ImportMesh()
    {
        const string filter =
            "All Mesh Files (*.stl;*.obj;*.wrl)|*.stl;*.obj;*.wrl" +
            "|STL Files (*.stl)|*.stl" +
            "|OBJ Files (*.obj)|*.obj" +
            "|VRML Files (*.wrl)|*.wrl";

        var path = _fileDialogService.ShowOpenFileDialog(filter, "Import Mesh");
        if (path is null) return;

        IFileImporter importer;
        try
        {
            importer = Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".stl" => new StlImporter(),
                ".obj" => new ObjImporter(),
                ".wrl" => new WrlImporter(),
                var ext => throw new NotSupportedException($"Format '{ext}' is not supported.")
            };
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage(ex.Message, "Import Error");
            return;
        }

        var validation = importer.Validate(path);
        if (!validation.IsValid)
        {
            _dialogService.ShowMessage(
                $"Cannot import file:\n{validation.ErrorMessage}", "Import Error");
            return;
        }

        IReadOnlyList<IGeometricEntity> entities;
        try
        {
            entities = importer.Import(path);
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage($"Import failed:\n{ex.Message}", "Import Error");
            return;
        }

        if (entities.Count == 0)
        {
            _dialogService.ShowMessage("No triangles found in the file.", "Import Result");
            return;
        }

        // Combine all triangles into a single MeshEntity named after the file
        string meshName = Path.GetFileNameWithoutExtension(path);
        var positions = entities
            .OfType<TriangleEntity>()
            .SelectMany(t => new[] { t.Vertex0, t.Vertex1, t.Vertex2 })
            .ToArray();

        var meshEntity = new MeshEntity(positions, meshName);
        var cmd = new CreateEntityCommand(_sceneManager, meshEntity);
        _undoManager.Execute(cmd);

        string fileName = Path.GetFileName(path);
        StatusText = $"Imported {meshEntity.TriangleCount} triangles from {fileName}";
    }

    private void OnUndoStackChanged()
    {
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoDescription));
        OnPropertyChanged(nameof(RedoDescription));
    }

    private void OnSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedEntity));
    }
}

// Simple parameter records for entity creation
public record SphereCreationParams(Vector3 Center, double Radius);
public record CylinderCreationParams(Vector3 BaseCenter, Vector3 Axis, double Radius, double Height);
public record ConeCreationParams(Vector3 BaseCenter, Vector3 Axis, double BaseRadius, double Height);
public record TorusCreationParams(Vector3 Center, Vector3 Normal, double MajorRadius, double MinorRadius);
public record PointCreationParams(Vector3 Position);
public record TriangleCreationParams(Vector3 V0, Vector3 V1, Vector3 V2);
public record VectorCreationParams(Vector3 Origin, Vector3 Direction);
public record PlaneCreationParams(Vector3 Origin, Vector3 Normal);
public record CuttingPlaneCreationParams(Vector3 Origin, Vector3 Normal, double DisplayWidth, double DisplayHeight, double Opacity, bool IsCappingEnabled, ClipSide ClipSide, double GapDistance, List<Guid> TargetEntityIds);
