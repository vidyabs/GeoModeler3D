# GeoModeler3D -- Developer Guide

A comprehensive guide for developers maintaining and extending the GeoModeler3D codebase, with special emphasis on understanding the 3D scene, scene graph, and rendering pipeline.

---

## Table of Contents

1. [Solution Overview](#1-solution-overview)
2. [Layer Architecture](#2-layer-architecture)
3. [Understanding the 3D Scene](#3-understanding-the-3d-scene)
4. [Scene Graph Deep Dive](#4-scene-graph-deep-dive)
5. [Entity System](#5-entity-system)
6. [Rendering Pipeline](#6-rendering-pipeline)
7. [Command System (Undo/Redo)](#7-command-system-undoredo)
8. [Hit-Testing and Selection](#8-hit-testing-and-selection)
9. [Dependency Injection Wiring](#9-dependency-injection-wiring)
10. [How to Add a New Entity Type](#10-how-to-add-a-new-entity-type)
11. [Key Design Decisions](#11-key-design-decisions)
12. [Gotchas and Pitfalls](#12-gotchas-and-pitfalls)

---

## 1. Solution Overview

```
GeoModeler3D.sln
  |
  +-- GeoModeler3D.Core         (net10.0)       -- Domain model, no UI dependencies
  +-- GeoModeler3D.Rendering    (net10.0-windows) -- WPF/HelixToolkit 3D rendering
  +-- GeoModeler3D.App          (net10.0-windows) -- WPF application, MVVM, DI
  +-- GeoModeler3D.Tests        (net10.0)       -- xUnit unit tests
```

Dependencies flow **one way only**: `App -> Rendering -> Core`. The Tests project references only Core.

---

## 2. Layer Architecture

```
+-------------------------------------------------------------+
|                    GeoModeler3D.App                          |
|  MainWindow, ViewModels, Dialogs, DI container              |
|  Depends on: Rendering, Core                                |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                 GeoModeler3D.Rendering                       |
|  HelixToolkit adapters, entity renderers, viewport mgmt     |
|  Depends on: Core                                           |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                    GeoModeler3D.Core                         |
|  Entities, SceneGraph, Commands, Math                       |
|  No UI dependencies (pure .NET)                             |
+-------------------------------------------------------------+
```

**Why this matters:** Core has zero WPF dependencies. It uses `System.Numerics.Vector3` (not `System.Windows.Media.Media3D.Point3D`) and a custom `EntityColor` struct (not `System.Windows.Media.Color`). This means Core logic is fully testable without a UI thread.

---

## 3. Understanding the 3D Scene

If you are new to 3D programming, here is a mental model of how 3D scenes work in WPF/HelixToolkit.

### 3.1 The Coordinate System

GeoModeler3D uses a **right-handed coordinate system** with Z up:

```
        Z (up)
        |
        |
        +------ X (right)
       /
      Y (forward)
```

All entity positions use `System.Numerics.Vector3(X, Y, Z)`.

### 3.2 What Is a Viewport?

A **viewport** is a window into the 3D world. It contains:

- A **camera** (where you are looking from, what direction, field of view)
- **Lights** (without lights, everything is black)
- **Visual3D objects** (the 3D shapes you see)

In our code, `HelixViewport3D` (from HelixToolkit.Wpf) is the viewport. It is a WPF control that handles camera rotation, panning, zooming, and rendering.

### 3.3 What Is a Visual3D?

`Visual3D` is the WPF base class for anything visible in 3D space. Think of it as a "3D widget" you add to the viewport. Examples from HelixToolkit:

| HelixToolkit Class | What it renders | Used for |
|---|---|---|
| `SphereVisual3D` | A sphere | SphereEntity, PointEntity |
| `PipeVisual3D` | A cylinder/pipe | CylinderEntity |
| `TruncatedConeVisual3D` | A cone | ConeEntity |
| `TorusVisual3D` | A torus | TorusEntity |
| `LinesVisual3D` | Wire lines | CircleEntity |
| `ModelVisual3D` | Generic container | TriangleEntity, lights |
| `GridLinesVisual3D` | Ground grid | Viewport background |
| `CoordinateSystemVisual3D` | XYZ arrows | Orientation reference |

To add a shape to the scene, you create one of these and add it to `viewport.Children`.

### 3.4 Materials and Lighting

A **Material** defines how a surface looks (color, shininess, transparency). WPF provides:

- `DiffuseMaterial` -- matte surface, reacts to light direction
- `SpecularMaterial` -- shiny highlights
- `EmissiveMaterial` -- glows regardless of light

HelixToolkit's `MaterialHelper.CreateMaterial(color)` creates a sensible default (diffuse + specular). Our renderers use this.

**Lights** illuminate the scene. ViewportManager sets up:
- 1 ambient light (baseline illumination so nothing is pitch black)
- 2 directional lights (from different angles for depth perception)

### 3.5 Camera Basics

The camera is a `PerspectiveCamera` with:

```csharp
Position      = new Point3D(10, 10, 10)   // where the camera is
LookDirection = new Vector3D(-1, -1, -1)  // which way it points
UpDirection   = new Vector3D(0, 0, 1)     // which way is "up"
FieldOfView   = 45                        // zoom angle in degrees
```

HelixToolkit handles mouse interaction (rotate/pan/zoom) automatically. The `SetCameraView()` method in ViewportManager lets us snap to preset views (Top, Front, Right, Isometric).

---

## 4. Scene Graph Deep Dive

### 4.1 What Is a Scene Graph?

A **scene graph** is a data structure that holds all the objects in a 3D scene. It answers the question: "What exists in this world right now?"

In GeoModeler3D, the scene graph is deliberately simple -- a **flat list** of entities, not a tree:

```
SceneManager
  |
  +-- Entities: ObservableCollection<IGeometricEntity>
  |     [0] SphereEntity { Id=abc, Center=(0,0,0), Radius=1 }
  |     [1] CylinderEntity { Id=def, BaseCenter=(3,0,0), ... }
  |     [2] TorusEntity { Id=ghi, Center=(0,3,0), ... }
  |
  +-- Events:
        EntityAdded(entity)     -- fired when an entity is added
        EntityChanged(entity)   -- fired when any property changes
        EntityRemoved(entityId) -- fired when an entity is removed
```

### 4.2 SceneManager (src/GeoModeler3D.Core/SceneGraph/SceneManager.cs)

This is the **single source of truth** for what exists in the scene. Key responsibilities:

| Method | What it does |
|---|---|
| `Add(entity)` | Adds entity to list, subscribes to PropertyChanged, fires EntityAdded |
| `Remove(id)` | Finds entity by ID, unsubscribes, removes from list, fires EntityRemoved |
| `Insert(index, entity)` | Like Add but at a specific index (used by Undo) |
| `GetById(id)` | Looks up entity by Guid |
| `Clear()` | Removes all entities (fires EntityRemoved for each) |

**Critical detail:** SceneManager subscribes to each entity's `PropertyChanged` event. When you change `sphere.Radius = 2`, SceneManager detects this and fires `EntityChanged`. The rendering layer listens for this and updates the 3D visual.

### 4.3 SelectionManager (src/GeoModeler3D.Core/SceneGraph/SelectionManager.cs)

Tracks which entities are currently selected. It stores a list of `Guid`s (entity IDs), not entity references.

| Method | Behavior |
|---|---|
| `Select(id)` | Clears selection, selects only this entity |
| `ToggleSelect(id)` | Adds if not selected, removes if selected |
| `AddToSelection(id)` | Adds to existing selection |
| `ClearSelection()` | Deselects everything |

The `SelectionChanged` event notifies the UI and rendering layer to update highlights.

### 4.4 Data Flow: Creating an Entity

Here is the complete path when a user creates a Sphere:

```
User clicks Create > Sphere, fills dialog, clicks OK
  |
  v
MainWindow.OnCreateSphere()
  | creates SphereCreationParams
  v
MainViewModel.CreateSphereCommand.Execute(params)
  | creates SphereEntity + CreateEntityCommand
  v
UndoManager.Execute(command)
  | calls command.Execute(), pushes to undo stack
  v
CreateEntityCommand.Execute()
  | calls SceneManager.Add(entity)
  v
SceneManager.Add(entity)
  | adds to ObservableCollection
  | subscribes to entity.PropertyChanged
  | fires EntityAdded event
  v
MainWindow.InitializeServices() wired: EntityAdded -> RenderingService.AddEntity()
  |
  v
RenderingService.AddEntity(entity)
  | looks up renderer: EntityRendererRegistry.GetRenderer(typeof(SphereEntity))
  | calls SphereEntityRenderer.CreateVisual(entity)
  |   -> creates SphereVisual3D, sets Center/Radius/Material
  | stores Guid <-> Visual3D mapping
  | adds Visual3D to viewport.Children
  v
Sphere appears on screen!
```

### 4.5 Data Flow: Property Change

When an entity property changes (e.g., setting Radius):

```
entity.Radius = 2
  | EntityBase.SetField<double>() detects value changed
  | fires PropertyChanged("Radius")
  v
SceneManager.OnEntityPropertyChanged()
  | fires EntityChanged(entity)
  v
RenderingService.UpdateEntity(entity)
  | gets existing Visual3D from _entityVisuals dictionary
  | calls SphereEntityRenderer.UpdateVisual(entity, visual)
  |   -> updates SphereVisual3D.Radius, Center, Material
  v
Visual updates on screen!
```

---

## 5. Entity System

### 5.1 Entity Hierarchy

```
IGeometricEntity (interface)
  |-- INotifyPropertyChanged
  |-- Id, Name, Color, IsVisible, Layer, BoundingBox
  |-- Transform(Matrix4x4), Clone(), Accept(IEntityVisitor)
  |
  +-- EntityBase (abstract class)
        |-- Guid Id (immutable, set in constructor)
        |-- SetField<T>() helper for INotifyPropertyChanged
        |-- CopyMetadataTo() for cloning
        |
        +-- PointEntity       { Position }
        +-- SphereEntity       { Center, Radius }
        +-- CylinderEntity     { BaseCenter, Axis, Radius, Height }
        +-- ConeEntity         { BaseCenter, Axis, BaseRadius, Height }
        +-- TorusEntity        { Center, Normal, MajorRadius, MinorRadius }
        +-- CircleEntity       { Center, Normal, Radius }
        +-- TriangleEntity     { Vertex0, Vertex1, Vertex2 }
        +-- CuttingPlaneEntity { Origin, Normal, ... } (stub)
        +-- ContourCurveEntity { Points, IsClosed, ... } (stub)
```

### 5.2 EntityColor vs WPF Color

Core uses `EntityColor` (a simple RGBA record struct) instead of `System.Windows.Media.Color` because Core must not reference WPF assemblies. The Rendering layer converts using extension methods:

```csharp
// In ConversionExtensions.cs (Rendering layer)
public static Color ToWpfColor(this EntityColor c) => Color.FromArgb(c.A, c.R, c.G, c.B);
public static Point3D ToPoint3D(this Vector3 v) => new(v.X, v.Y, v.Z);
```

### 5.3 BoundingBox3D

Each entity computes its own bounding box via `ComputeBoundingBox()`. This axis-aligned bounding box (AABB) is used for zoom-to-fit and spatial queries.

---

## 6. Rendering Pipeline

### 6.1 Architecture

```
+------------------+        +-------------------+        +------------------+
| IGeometricEntity | -----> | IEntityRenderer   | -----> | Visual3D         |
| (Core data)      |        | (adapter)         |        | (WPF 3D object)  |
+------------------+        +-------------------+        +------------------+
       SphereEntity   ->   SphereEntityRenderer   ->   SphereVisual3D
       ConeEntity     ->   ConeEntityRenderer     ->   TruncatedConeVisual3D
       CylinderEntity ->   CylinderEntityRenderer ->   PipeVisual3D
```

This is the **Adapter pattern**: each renderer adapts a Core entity into a WPF Visual3D.

### 6.2 EntityRendererRegistry

A dictionary mapping `Type -> IEntityRenderer`. When RenderingService needs to render a SphereEntity, it asks: `registry.GetRenderer(typeof(SphereEntity))`. All renderers are registered at startup in `App.xaml.cs`.

### 6.3 RenderingService

The central orchestrator. Maintains two dictionaries:

```
_entityVisuals:   Guid -> Visual3D    (find the visual for an entity)
_visualToEntity:  Visual3D -> Guid    (find the entity for a visual, used in hit-testing)
```

These are inverse maps. When an entity is added, both are updated. When removed, both are cleaned up.

### 6.4 ViewportManager

Owns the viewport setup: camera, lights, grid, coordinate axes. This is separate from RenderingService because viewport infrastructure (grid, axes) is not tied to entities.

### 6.5 SelectionHighlighter

Swaps an entity's Material with a yellow highlight material when selected, and restores the original when deselected. It handles two Visual3D hierarchies:

- `MeshElement3D` (HelixToolkit built-in visuals like SphereVisual3D) -- has `.Material` directly
- `ModelVisual3D` with `GeometryModel3D` content (custom meshes like TriangleEntity) -- access `.Content.Material`

---

## 7. Command System (Undo/Redo)

### 7.1 How It Works

Every user action that modifies the scene goes through UndoManager:

```csharp
// Instead of directly calling scene.Add(entity):
var command = new CreateEntityCommand(sceneManager, entity);
undoManager.Execute(command);  // calls command.Execute() + pushes to undo stack
```

The UndoManager maintains two stacks:

```
Undo Stack:  [CreateSphere, CreateCylinder, DeleteTorus]  <- top
Redo Stack:  [CreateCone]  <- top (populated after undo)
```

- `Undo()` pops from undo stack, calls `command.Undo()`, pushes to redo stack
- `Redo()` pops from redo stack, calls `command.Execute()`, pushes to undo stack
- `Execute()` always clears the redo stack (new action invalidates redo history)

### 7.2 Command Types

| Command | Execute | Undo |
|---|---|---|
| `CreateEntityCommand` | `scene.Add(entity)` | `scene.Remove(entity.Id)` |
| `DeleteEntityCommand` | `scene.Remove(entity.Id)` | `scene.Insert(originalIndex, entity)` |
| `TransformEntityCommand` | `entity.Transform(matrix)` | `entity.Transform(inverseMatrix)` |
| `ChangePropertyCommand` | Sets property via reflection | Restores old value via reflection |
| `MacroCommand` | Executes children in order | Undoes children in reverse order |

---

## 8. Hit-Testing and Selection

### 8.1 How Click-to-Select Works

When the user clicks in the viewport:

```
Mouse click at pixel (x, y)
  |
  v
ViewportView.OnViewportMouseDown()
  | calls Viewport.Viewport.FindHits(position)
  | -> HelixToolkit ray-casts from camera through pixel into 3D scene
  | -> returns list of HitResult objects (sorted by distance)
  v
For each hit:
  | hit.Visual is the Visual3D that was hit
  | but it might be a child visual (e.g., a mesh inside a SphereVisual3D)
  v
RenderingService.GetEntityIdFromVisual(hit.Visual)
  | walks UP the visual tree using VisualTreeHelper.GetParent()
  | checks each parent against _visualToEntity dictionary
  | returns the entity Guid if found
  v
SelectionManager.Select(entityId)
  | updates selection state
  | fires SelectionChanged event
  v
RenderingService.HighlightEntities(selectedIds)
  | swaps materials to yellow highlight
```

The visual tree walk is necessary because HelixToolkit's hit-test may return an internal child visual (like a mesh geometry), not the top-level SphereVisual3D we registered.

---

## 9. Dependency Injection Wiring

All services are registered in `App.xaml.cs -> ConfigureServices()`. The startup sequence is:

```
1. ConfigureServices() registers everything as singletons
2. ServiceProvider resolves MainWindow + MainViewModel
3. MainWindow.DataContext = MainViewModel
4. MainWindow.InitializeServices() is called with:
   - RenderingService, ViewportManager, SelectionManager, SceneManager
5. Inside InitializeServices():
   a. ViewportManager.Initialize(viewport)     -- sets up camera, lights, grid
   b. RenderingService.Initialize(viewport)    -- stores viewport reference
   c. SceneManager events are wired to RenderingService:
      - EntityAdded   -> renderingService.AddEntity()
      - EntityChanged -> renderingService.UpdateEntity()
      - EntityRemoved -> renderingService.RemoveEntity()
```

---

## 10. How to Add a New Entity Type

Follow these steps to add, say, an **EllipsoidEntity**:

### Step 1: Core Entity

Create `src/GeoModeler3D.Core/Entities/EllipsoidEntity.cs`:

```csharp
public class EllipsoidEntity : EntityBase
{
    private Vector3 _center;
    private double _radiusX, _radiusY, _radiusZ;

    // Constructor, properties, Transform(), Clone(), Accept(), ComputeBoundingBox()
}
```

Add a `Visit(EllipsoidEntity)` method to `IEntityVisitor`.

### Step 2: Entity Renderer

Create `src/GeoModeler3D.Rendering/EntityRenderers/EllipsoidEntityRenderer.cs`:

```csharp
public class EllipsoidEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(EllipsoidEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        // Create a HelixToolkit visual (e.g., EllipsoidVisual3D or manual mesh)
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        // Update properties on the existing visual
    }

    public void DisposeVisual(Visual3D visual) { }
}
```

### Step 3: Register the Renderer

In `App.xaml.cs`, add to `ConfigureServices()`:

```csharp
registry.Register(new EllipsoidEntityRenderer());
```

### Step 4: Creation Dialog

Create `Views/Dialogs/CreateEllipsoidDialog.xaml` and `.xaml.cs` with input fields for center and radii.

### Step 5: ViewModel Command

Add to `MainViewModel.cs`:

```csharp
public record EllipsoidCreationParams(Vector3 Center, double RadiusX, double RadiusY, double RadiusZ);

[RelayCommand]
private void CreateEllipsoid(EllipsoidCreationParams? p) { ... }
```

### Step 6: Menu Entry

Add to `MainWindow.xaml`:

```xml
<MenuItem Header="_Ellipsoid..." Click="OnCreateEllipsoid"/>
```

### Step 7: Tests

Add tests in `src/GeoModeler3D.Tests/Entities/EllipsoidEntityTests.cs`.

---

## 11. Key Design Decisions

### Why EntityColor instead of System.Windows.Media.Color?

`System.Windows.Media.Color` lives in `PresentationCore.dll` (WPF). If Core referenced it, Core would no longer be platform-independent. EntityColor is a simple `readonly record struct(byte R, byte G, byte B, byte A)` with zero dependencies.

### Why Vector3 (System.Numerics) instead of Point3D?

`System.Numerics.Vector3` is part of the base .NET runtime. `Point3D` and `Vector3D` are WPF types. Using `System.Numerics` keeps Core portable and leverages SIMD hardware acceleration.

### Why flat list instead of tree scene graph?

For geometric primitives (spheres, cones, etc.), parent-child relationships add complexity without benefit. A flat `ObservableCollection` is simpler and sufficient. If hierarchical grouping is needed later, a `GroupEntity` containing child IDs could be added without restructuring.

### Why adapter pattern for renderers?

Each entity type maps to a different HelixToolkit Visual3D class. The adapter pattern (`IEntityRenderer`) keeps this mapping clean and extensible: adding a new entity type means adding one new renderer class, with no changes to RenderingService.

### Why dual dictionaries in RenderingService?

`_entityVisuals` (Guid -> Visual3D) is for forward lookups: "find the visual for this entity". `_visualToEntity` (Visual3D -> Guid) is for reverse lookups during hit-testing: "what entity does this visual belong to?". Both are needed and must stay in sync.

---

## 12. Gotchas and Pitfalls

### HelixToolkit v3 API Differences

We use HelixToolkit.Wpf v3.1.2 (targeting net6.0+). Key differences from v2:

- `MeshBuilder` is in the `HelixToolkit.Geometry` namespace, not `HelixToolkit.Wpf`
- `MeshBuilder.AddTriangle()` takes `System.Numerics.Vector3`, not `Point3D`
- `MeshBuilder.ToMesh()` returns `HelixToolkit.Geometry.MeshGeometry3D`, not the WPF type
- `CoordinateSystemVisual3D` does not have a `Visible` property -- use add/remove from viewport instead
- Built-in visuals (`SphereVisual3D`, `PipeVisual3D`, etc.) inherit from `MeshElement3D` which has a `Material` property

### WPF Threading

All Visual3D operations must happen on the UI thread. SceneManager events (EntityAdded, EntityChanged, EntityRemoved) are fired synchronously on whatever thread modifies the entity. Since all UI interactions come from the WPF dispatcher thread, this works. If you add background processing, you must `Dispatcher.Invoke()` before touching visuals.

### PropertyChanged Cascading

When a SceneManager entity property changes, the event chain is:
`entity.PropertyChanged -> SceneManager.EntityChanged -> RenderingService.UpdateEntity`

Be careful not to set entity properties inside `UpdateVisual()`, as this would create an infinite loop. Renderers should only read entity properties and write to Visual3D properties.

### Undo After Delete

`DeleteEntityCommand` remembers the entity's index in the list so that Undo can `Insert()` it back at the same position. If you add sorting or filtering to the entity list, this index-based approach may need adjustment.

### GeometryModel3D vs Visual3D

`GeometryModel3D` and `Visual3D` are in **different type hierarchies**. You cannot pattern-match `visual is GeometryModel3D`. To access a GeometryModel3D, check `visual is ModelVisual3D mv && mv.Content is GeometryModel3D gm`. HelixToolkit's built-in visuals (SphereVisual3D, etc.) inherit from `MeshElement3D`, which is a `Visual3D` subclass with a `Material` property.

### Vector3 with Keyword

`System.Numerics.Vector3` is a struct, so `vector with { X = 5 }` creates a copy with X changed. This is used in EntityViewModels for per-component editing. Remember that setting `entity.Position = entity.Position with { X = 5 }` triggers PropertyChanged because it assigns a new Vector3 (struct value comparison in SetField detects the change).
