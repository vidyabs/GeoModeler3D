# GeoModeler3D -- Implementation Plan for Demo-Ready Codebase

## Context

GeoModeler3D has detailed architecture and requirements docs (`docs/Architecture.md`, `docs/Requirement_Document.md`) but **zero source code**. The goal is to produce a working demo that launches, renders 3D primitives, supports entity creation via menus, selection, and undo/redo. Target: **.NET 10 + Visual Studio 2026** (docs say .NET 8 -- we upgrade all TFMs).

### Compatibility Notes
- **HelixToolkit.Wpf v3.1.2** ships `net6.0` assets -- consumable from `net10.0-windows` via forward compatibility. If runtime issues arise, build from source.
- **CommunityToolkit.Mvvm v8.4.0** needs `<LangVersion>preview</LangVersion>` on .NET 10 (known issue).
- **Color in Core**: Use custom `EntityColor` record struct (no WPF dep). Convert in Rendering layer.

---

## Phase 0: Solution Scaffold (~12 files)

**Goal:** 4-project solution that compiles. Empty WPF window launches.

### Files
- `GeoModeler3D.sln`
- `.gitignore` -- replace C/C++ template with .NET standard
- `src/GeoModeler3D.Core/GeoModeler3D.Core.csproj` -- `net10.0`, no UI deps
- `src/GeoModeler3D.Rendering/GeoModeler3D.Rendering.csproj` -- `net10.0-windows`, `UseWPF`, refs Core + HelixToolkit.Wpf 3.1.2
- `src/GeoModeler3D.App/GeoModeler3D.App.csproj` -- `net10.0-windows`, WinExe, refs Core + Rendering + CommunityToolkit.Mvvm 8.4.0 + Microsoft.Extensions.DependencyInjection 10.0.0 + Serilog 4.2.0
- `src/GeoModeler3D.Tests/GeoModeler3D.Tests.csproj` -- `net10.0`, xunit 2.9.x + Moq 4.20.x
- `src/GeoModeler3D.App/App.xaml` + `App.xaml.cs` -- minimal WPF entry
- `src/GeoModeler3D.App/Views/MainWindow.xaml` + `.cs` -- empty window

**Verify:** `dotnet build` succeeds, `dotnet run --project src/GeoModeler3D.App` shows empty window.

---

## Phase 1: Core Domain Model (~22 files)

**Goal:** Entity hierarchy, math utils, scene graph, selection. Pure C#, no UI. Unit tests pass.

### Step 1.1: Math Utilities
- `src/GeoModeler3D.Core/Math/BoundingBox3D.cs` -- readonly struct, Min/Max Vector3, Merge(), Contains()
- `src/GeoModeler3D.Core/Math/Plane3D.cs` -- Origin + Normal, DistanceToPoint(), ClassifySide()
- `src/GeoModeler3D.Core/Math/MathConstants.cs` -- Tolerance = 1e-9, DefaultSegmentCount = 64
- `src/GeoModeler3D.Core/Math/GeometryUtils.cs` -- AngleBetweenVectors(), LinePlaneIntersection()

Uses `System.Numerics.Vector3` and `Matrix4x4` (built-in).

### Step 1.2: Entity Interface + Base
- `src/GeoModeler3D.Core/Entities/EntityColor.cs` -- `readonly record struct EntityColor(byte R, byte G, byte B, byte A = 255)`
- `src/GeoModeler3D.Core/Entities/IGeometricEntity.cs` -- Id, Name, Color, IsVisible, Layer, BoundingBox, Transform(), Clone(), Accept()
- `src/GeoModeler3D.Core/Entities/EntityBase.cs` -- abstract, implements INotifyPropertyChanged, provides common properties

### Step 1.3: Concrete Entities (all 9 + enum)
- `PointEntity.cs` -- Position (Vector3)
- `TriangleEntity.cs` -- Vertex0/1/2, computed Normal/Area
- `CircleEntity.cs` -- Center, Normal, Radius, SegmentCount
- `SphereEntity.cs` -- Center, Radius
- `CylinderEntity.cs` -- BaseCenter, Axis, Radius, Height
- `ConeEntity.cs` -- BaseCenter, Axis, BaseRadius, Height, computed HalfAngle
- `TorusEntity.cs` -- Center, Normal, MajorRadius, MinorRadius
- `CuttingPlaneEntity.cs` -- stub with properties
- `ContourCurveEntity.cs` -- stub with properties
- `ConicSectionType.cs` -- enum

### Step 1.4: Visitor Interface
- `src/GeoModeler3D.Core/Operations/IEntityVisitor.cs` -- Visit() overloads for all 9 types

### Step 1.5: SceneGraph
- `src/GeoModeler3D.Core/SceneGraph/SceneManager.cs` -- ObservableCollection, Add/Remove/GetById, EntityAdded/Changed/Removed events
- `src/GeoModeler3D.Core/SceneGraph/SelectionManager.cs` -- Select/ToggleSelect/Clear, SelectionChanged event
- `src/GeoModeler3D.Core/SceneGraph/LayerManager.cs` -- stub

### Step 1.6: Unit Tests
- `src/GeoModeler3D.Tests/Entities/SphereEntityTests.cs`
- `src/GeoModeler3D.Tests/Entities/ConeEntityTests.cs`
- `src/GeoModeler3D.Tests/Entities/PointEntityTests.cs`
- `src/GeoModeler3D.Tests/SceneGraph/SceneManagerTests.cs`

**Verify:** `dotnet test` passes ~20-30 tests. Core has zero WPF references.

---

## Phase 2: Command & Undo/Redo (~9 files)

**Goal:** Command pattern with UndoManager. Create/Delete entities are undoable.

### Files
- `src/GeoModeler3D.Core/Commands/IUndoableCommand.cs` -- Description, Execute(), Undo()
- `src/GeoModeler3D.Core/Commands/UndoManager.cs` -- undo/redo stacks, MaxDepth=50, CanUndo/CanRedo, StackChanged event
- `src/GeoModeler3D.Core/Commands/CreateEntityCommand.cs` -- Add on Execute, Remove on Undo
- `src/GeoModeler3D.Core/Commands/DeleteEntityCommand.cs` -- Remove on Execute, Insert at original index on Undo
- `src/GeoModeler3D.Core/Commands/TransformEntityCommand.cs` -- stub
- `src/GeoModeler3D.Core/Commands/ChangePropertyCommand.cs` -- stub
- `src/GeoModeler3D.Core/Commands/MacroCommand.cs` -- stub
- `src/GeoModeler3D.Tests/Commands/UndoManagerTests.cs`
- `src/GeoModeler3D.Tests/Commands/CreateEntityCommandTests.cs`

**Verify:** `dotnet test` passes ~35-45 tests.

---

## Phase 3: Rendering Adapter Layer (~18 files)

**Goal:** HelixToolkit renderers for all demo entities. RenderingService bridges SceneManager events to Visual3D objects.

### Step 3.1: Infrastructure
- `IEntityRenderer.cs` -- SupportedEntityType, CreateVisual(), UpdateVisual(), DisposeVisual()
- `IRenderingService.cs` -- Initialize(), AddEntity(), RemoveEntity(), HighlightEntities()
- `RenderingService.cs` -- subscribes to SceneManager events, maps Guid->Visual3D
- `ViewportManager.cs` -- wraps HelixViewport3D setup (camera, grid, axes, lights)
- `EntityRendererRegistry.cs` -- Type->IEntityRenderer mapping
- `SelectionHighlighter.cs` -- material swapping for selection
- `DisplayMode.cs` -- enum
- `Extensions/ConversionExtensions.cs` -- Vector3<->Point3D, EntityColor<->WpfColor
- Stubs: GizmoManager, CuttingPlaneVisualizer, FrameCaptureService

### Step 3.2: Entity Renderers (7 functional + 2 stubs)
- PointEntityRenderer -- small sphere
- SphereEntityRenderer -- MeshBuilder.AddSphere
- CylinderEntityRenderer -- MeshBuilder.AddCylinder
- ConeEntityRenderer -- MeshBuilder.AddCone
- TorusEntityRenderer -- MeshBuilder.AddTorus
- CircleEntityRenderer -- tessellated polyline
- TriangleEntityRenderer -- MeshGeometry3D with 3 vertices
- CuttingPlane + ContourCurve -- stubs

---

## Phase 4: WPF App Shell + MVVM (~30 files)

**Goal:** Full working demo. MainWindow with menus, 3D viewport, entity list, properties panel, creation dialogs.

### Step 4.1: DI + App Startup
- App.xaml.cs -- ConfigureServices(), register all singletons
- IDialogService + DialogService
- IFileDialogService + FileDialogService

### Step 4.2: ViewModels
- MainViewModel -- owns SceneManager, UndoManager; RelayCommands for New, Exit, Undo, Redo, Delete, Create*
- ViewportViewModel, PropertiesPanelViewModel, StatusBarViewModel
- EntityViewModels (Sphere, Cylinder, Cone, Torus, Point)

### Step 4.3: Views
- MainWindow.xaml -- Menu, StatusBar, Grid(EntityList | Viewport | Properties)
- ViewportView.xaml -- HelixViewport3D
- PropertiesPanel.xaml, StatusBarView.xaml
- Create dialogs: Sphere, Cylinder, Cone, Torus, Point
- Converters, Styles

### Step 4.4: Hit-Testing & Selection
- ViewportView code-behind: MouseLeftButtonDown -> FindHits -> SelectionManager

---

## Phase 5: Stubs for Deferred Features (~45 files)

All remaining files from Architecture doc: Operations, Animation, Serialization, Import, Export, remaining ViewModels/Views/Dialogs/Controls.

---

## Phase 6: Polish (~5 files)

- View menu commands (ZoomToFit, ToggleGrid, standard views)
- Keyboard shortcuts (Ctrl+Z/Y, Delete, Ctrl+N)
- Serilog file logging
- About dialog

---

## Summary

| Phase | Description | ~Files | Cumulative |
|-------|-------------|--------|------------|
| 0 | Solution scaffold | 12 | 12 |
| 1 | Core entities + math + scene | 22 | 34 |
| 2 | Commands + undo/redo | 9 | 43 |
| 3 | Rendering layer | 18 | 61 |
| 4 | WPF app shell + MVVM | 30 | 91 |
| 5 | Stubs for deferred features | 45 | 136 |
| 6 | Polish | 5 | 141 |

Total: ~141 files for a complete, demo-ready codebase.
