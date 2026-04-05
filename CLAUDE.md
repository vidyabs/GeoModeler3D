# GeoModeler3D — Claude Code Guide

## Build & Test

```bash
dotnet build          # build all projects
dotnet test           # run all tests (currently 62 tests, all pass)
```

No special setup required. The solution targets `.NET 10` with `LangVersion: preview`.

---

## Architecture Overview

Four projects with strict unidirectional dependencies:

```
GeoModeler3D.App  →  GeoModeler3D.Rendering  →  GeoModeler3D.Core
GeoModeler3D.Tests                             →  GeoModeler3D.Core
```

| Project | Target | Role |
|---------|--------|------|
| `GeoModeler3D.Core` | `net10.0` | Entities, commands, serialization, import — NO WPF |
| `GeoModeler3D.Rendering` | `net10.0-windows` | IEntityRenderer implementations (HelixToolkit.Wpf) |
| `GeoModeler3D.App` | `net10.0-windows` | WPF shell, MVVM ViewModels, dialogs, DI wiring |
| `GeoModeler3D.Tests` | `net10.0` | xUnit tests for Core |

**Key NuGet packages:**
- `HelixToolkit.Wpf 3.1.2` — 3D rendering (ArrowVisual3D, SphereVisual3D, MeshGeometry3D, MaterialHelper, etc.)
- `CommunityToolkit.Mvvm 8.*` — `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`
- `Microsoft.Extensions.DependencyInjection 9.*` — DI container wired in `App.xaml.cs`

---

## Entity System — How to Add a New Entity

This is the most common extension point. Every entity follows the same 12-step pattern:

### Step 1 — Entity class (`src/GeoModeler3D.Core/Entities/`)

Inherit from `EntityBase`. Required members:
- Constructor(s) — one public for creation, one private with `Guid id` param for deserialization (look at `PointEntity.cs` as the simplest example)
- Properties using `SetField(ref _field, value)` for change notification
- `Transform(Matrix4x4)` — use `Vector3.Transform` for positions, `Vector3.TransformNormal` for directions, then call `OnPropertyChanged` for each affected property
- `Clone()` — create new instance with `Guid.NewGuid()`, call `CopyMetadataTo(clone)`
- `Accept(IEntityVisitor visitor) => visitor.Visit(this)`
- `ComputeBoundingBox()` — use `BoundingBox3D.FromPoint(v)` or `BoundingBox3D.FromPoints([v1, v2, ...])`

**Available `EntityColor` constants:** `White`, `Cyan`, `Yellow`, `Green`, `Red` — or `new EntityColor(R, G, B)` / `new EntityColor(R, G, B, A)`

### Step 2 — Add to `IEntityVisitor` (`src/GeoModeler3D.Core/Entities/IEntityVisitor.cs`)

```csharp
void Visit(MyNewEntity entity);
```

### Step 3 — Add to `EntitySerializationVisitor` (`src/GeoModeler3D.Core/Serialization/EntitySerializationVisitor.cs`)

```csharp
public void Visit(MyNewEntity entity)
{
    SetCommon(entity, "MyNew");          // sets type/id/name/color/isVisible/layer
    Result["someVec"] = Vec3(entity.SomeVector3);
    Result["someDouble"] = entity.SomeDouble;
}
```

### Step 4 — Add deserialization to `ProjectSerializer` (`src/GeoModeler3D.Core/Serialization/ProjectSerializer.cs`)

Add a case in the `type switch` inside `DeserializeEntity()`:
```csharp
"MyNew" => DeserializeMyNew(elem),
```
Add the private method using `ReadVec3(elem.GetProperty("propName"))` and `elem.GetProperty("propName").GetDouble()`.

### Step 5 — Renderer (`src/GeoModeler3D.Rendering/EntityRenderers/MyNewEntityRenderer.cs`)

```csharp
public class MyNewEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(MyNewEntity);
    public Visual3D CreateVisual(IGeometricEntity entity) { var v = new SomeHelixVisual3D(); Apply(...); return v; }
    public void UpdateVisual(IGeometricEntity entity, Visual3D visual) => Apply(...);
    public void DisposeVisual(Visual3D visual) { }
    private static void Apply(MyNewEntity e, SomeHelixVisual3D v) { ... v.Visible = e.IsVisible; }
}
```

**Available HelixToolkit visuals:** `SphereVisual3D`, `ArrowVisual3D`, `ModelVisual3D` (for custom `MeshGeometry3D`), `LinesVisual3D`. Use `MaterialHelper.CreateMaterial(color)` for materials. Extension methods `ToPoint3D()`, `ToVector3D()` on `Vector3` are in `GeoModeler3D.Rendering.Extensions.ConversionExtensions`.

**Visibility pattern for `ModelVisual3D`** (WPF 3D has no `Visibility` property on `Visual3D`):
```csharp
if (!entity.IsVisible) { visual.Content = null; return; }   // hides
// to show: rebuild visual.Content
```
`SphereVisual3D`, `ArrowVisual3D` etc. have `.Visible` property — use that directly.

### Step 6 — Register renderer in `App.xaml.cs`

```csharp
registry.Register(new MyNewEntityRenderer());
```

### Step 7 — Creation dialog (`src/GeoModeler3D.App/Views/Dialogs/CreateMyNewDialog.xaml` + `.xaml.cs`)

- XAML: `SizeToContent="WidthAndHeight"`, `ResizeMode="NoResize"`, `WindowStartupLocation="CenterOwner"`
- Code-behind: constructor takes any pre-selected entities, `OnCreate` validates + sets `Result`, `DialogResult = true`
- Result type: a simple record defined at the bottom of `MainViewModel.cs`

### Step 8 — Add `[RelayCommand]` to `MainViewModel` (`src/GeoModeler3D.App/ViewModels/MainViewModel.cs`)

```csharp
[RelayCommand]
private void CreateMyNew(MyNewCreationParams? p)
{
    if (p is null) return;
    var entity = new MyNewEntity(p.Prop1, p.Prop2);
    _undoManager.Execute(new CreateEntityCommand(_sceneManager, entity));
    StatusText = "Created MyNew";
}
```

Add the params record at the bottom of the file:
```csharp
public record MyNewCreationParams(Vector3 Prop1, double Prop2);
```

### Step 9 — Menu item in `MainWindow.xaml`

Add inside `<MenuItem Header="_Create">`.

### Step 10 — Handler in `MainWindow.xaml.cs`

```csharp
private void OnCreateMyNew(object sender, RoutedEventArgs e)
{
    var dialog = new CreateMyNewDialog(...) { Owner = this };
    if (dialog.ShowDialog() == true)
        ViewModel.CreateMyNewCommand.Execute(dialog.Result);
}
```
Selection is accessed via `ViewModel.SelectionManager.SelectedIds` + `ViewModel.SceneManager.GetById(id)`.

### Step 11 — Properties panel template (`src/GeoModeler3D.App/Views/PropertiesPanel.xaml`)

Add a `<DataTemplate DataType="{x:Type entities:MyNewEntity}">` inside `<UserControl.Resources>`.

- Editable `Vector3` component: `Tag="PropName.X"`, bind to `{Binding PropName, Converter={StaticResource Vec3Comp}, ConverterParameter=X, Mode=OneWay}`, add `LostFocus="OnEditLostFocus" KeyDown="OnEditKeyDown"`
- Editable `double`: `Tag="PropName"`, bind to `{Binding PropName, StringFormat=F3, Mode=OneWay}`, add same events
- Read-only: use `<TextBlock>` with `Style="{StaticResource PropValue}"` instead of `<TextBox>`

### Step 12 — Update `TestVisitor` in `src/GeoModeler3D.Tests/Entities/SphereEntityTests.cs`

```csharp
public void Visit(MyNewEntity entity) => VisitedType = nameof(MyNewEntity);
```

---

## Current Entity Types

| Entity | Color default | Key properties | Renderer visual |
|--------|--------------|----------------|-----------------|
| `PointEntity` | White | `Position: Vector3` | `SphereVisual3D` (r=0.15) |
| `TriangleEntity` | Cyan | `Vertex0/1/2: Vector3`; computed `Normal`, `Area` | `ModelVisual3D` (MeshGeometry3D, double-sided) |
| `CircleEntity` | Cyan | `Center`, `Normal`, `Radius`, `SegmentCount` | tessellated polygon |
| `SphereEntity` | Cyan | `Center`, `Radius` | `SphereVisual3D` (32×32) |
| `CylinderEntity` | Cyan | `BaseCenter`, `Axis`, `Radius`, `Height` | HelixToolkit cylinder |
| `ConeEntity` | Cyan | `BaseCenter`, `Axis`, `BaseRadius`, `Height`; computed `HalfAngle` | HelixToolkit cone |
| `TorusEntity` | Cyan | `Center`, `Normal`, `MajorRadius`, `MinorRadius` | HelixToolkit torus |
| `MeshEntity` | Cyan | `Positions: Vector3[]` (flat, every 3 = triangle); computed `TriangleCount` | single `MeshGeometry3D` |
| `VectorEntity` | Yellow | `Origin`, `Direction` (not normalized); computed `Tip`, `Magnitude` | `ArrowVisual3D` |
| `PlaneEntity` | Cornflower blue | `Origin`, `Normal` (auto-normalized), `DisplaySize` | semi-transparent `MeshGeometry3D` quad; `PlaneEntity.ComputeTangents(normal)` gives tangent pair |
| `CuttingPlaneEntity` | Blue (semi-transparent) | `Origin`, `Normal`, `DisplayWidth`, `DisplayHeight`, `Opacity`, `TargetEntityIds`, `IsCappingEnabled` | stub (empty `ModelVisual3D`) |
| `ContourCurveEntity` | — | `Points`, `IsClosed`, `SourcePlaneId`, `SourceEntityId`, `ConicType` | stub |

---

## Key Files Quick Reference

| What | Where |
|------|-------|
| All entity classes | `src/GeoModeler3D.Core/Entities/` |
| Visitor interface | `src/GeoModeler3D.Core/Entities/IEntityVisitor.cs` |
| JSON serialization | `src/GeoModeler3D.Core/Serialization/EntitySerializationVisitor.cs` |
| JSON deserialization | `src/GeoModeler3D.Core/Serialization/ProjectSerializer.cs` |
| All renderers | `src/GeoModeler3D.Rendering/EntityRenderers/` |
| Visibility + rendering wiring | `src/GeoModeler3D.Rendering/RenderingService.cs` |
| DI registration | `src/GeoModeler3D.App/App.xaml.cs` |
| Commands (undo/redo) | `src/GeoModeler3D.App/ViewModels/MainViewModel.cs` |
| Creation dialogs | `src/GeoModeler3D.App/Views/Dialogs/` |
| Menu items | `src/GeoModeler3D.App/Views/MainWindow.xaml` |
| Menu handlers + selection logic | `src/GeoModeler3D.App/Views/MainWindow.xaml.cs` |
| Properties panel templates | `src/GeoModeler3D.App/Views/PropertiesPanel.xaml` |
| Property editing logic | `src/GeoModeler3D.App/ViewModels/PropertiesPanelViewModel.cs` |
| Mesh importers (.stl/.obj/.wrl) | `src/GeoModeler3D.Core/Import/` |
| Math utilities | `src/GeoModeler3D.Core/Math/` (`BoundingBox3D`, `Plane3D`, `GeometryUtils`) |
| WPF↔Numerics converters | `src/GeoModeler3D.Rendering/Extensions/ConversionExtensions.cs` |
| Tests | `src/GeoModeler3D.Tests/` |

---

## Undo/Redo

All mutations go through `_undoManager.Execute(cmd)`. Available commands in `src/GeoModeler3D.Core/Commands/`:
- `CreateEntityCommand(sceneManager, entity)` — adds entity; undo removes it
- `DeleteEntityCommand` — removes; undo re-adds
- `ChangePropertyCommand<T>(entity, propertyName, oldValue, newValue)` — sets via reflection; undo restores old value
- `TransformEntityCommand` — wraps a matrix transform
- `MacroCommand` — groups multiple commands into one undo step

---

## Serialization Format (.gm3d)

JSON file. Each entity in the `entities` array has:
```json
{ "type": "TypeName", "id": "guid", "name": "...", "color": "#RRGGBB", "isVisible": true, "layer": "Default", ...typeSpecificFields }
```
`Vector3` fields are serialized as `{ "x": 1.0, "y": 0.0, "z": 0.0 }`.
`MeshEntity` positions are a flat float array `[x0,y0,z0, x1,y1,z1, ...]`.

Currently registered type strings: `"Point"`, `"Triangle"`, `"Circle"`, `"Sphere"`, `"Cylinder"`, `"Cone"`, `"Torus"`, `"Mesh"`, `"Vector"`, `"Plane"`, `"CuttingPlane"`, `"ContourCurve"`.

---

## Mesh Import

**File > Import Mesh** (Ctrl+I) supports `.stl` (ASCII + binary), `.obj`, `.wrl` (VRML 2.0). Each import creates one `MeshEntity` named after the file. Importers are in `src/GeoModeler3D.Core/Import/` and are stateless — instantiated inline in `MainViewModel.ImportMesh()`.

---

## Notes & Gotchas

- `GeoModeler3D.Core` must NOT reference WPF or HelixToolkit — it targets `net10.0` not `net10.0-windows`.
- `PlaneEntity.ComputeTangents(normal)` is a `public static` helper that returns `(Vector3 u, Vector3 v)` orthogonal to the given normal — used by both the entity's bounding box and the renderer.
- WPF 3D `Visual3D` has no `Visibility` property. To hide a `ModelVisual3D`: set `visual.Content = null`. To show it: call `renderer.UpdateVisual(entity, visual)`. `ArrowVisual3D` and `SphereVisual3D` do have `.Visible`.
- `PropertiesPanelViewModel.CommitEdit` handles `"PropName.X/Y/Z"` (Vector3 component) and plain `"PropName"` (double or string). It uses reflection — the property name in `Tag` must exactly match the C# property name.
- When adding `Visit(NewEntity)` to `IEntityVisitor`, the inner `TestVisitor` class in `SphereEntityTests.cs` must also be updated or the build will fail.
- Dialog code-behind constructors may reference named controls before `InitializeComponent()` is called — always call `InitializeComponent()` first, then set control properties.
