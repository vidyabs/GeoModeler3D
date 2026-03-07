# Technology Choice Document — GeoModeler3D

**Project Name:** GeoModeler3D  
**Version:** 1.0  
**Date:** March 2026  

---

## 1. Core Platform

**Language:** C# 12 / .NET 8 (LTS)  
**UI Framework:** WPF (Windows Presentation Foundation)

WPF remains the strongest choice for a Windows desktop application that needs to combine traditional UI elements (menus, toolbars, dockable panels) with an embedded 3D rendering surface. Its data-binding engine and the MVVM pattern provide clean separation between the geometric domain model and the presentation layer, which directly supports the extensibility goals outlined in the requirements. .NET 8 is the current long-term-support release, ensuring security patches and ecosystem support through at least November 2026.

---

## 2. 3D Graphics Component — Recommendation

### 2.1 Candidates Evaluated

Four libraries were evaluated for the 3D viewport component. The selection criteria were: free/open-source licensing, native WPF integration (no WinForms-Host hacks), ease of learning for a small team, built-in support for mesh building and camera controls, and a realistic path toward rendering more complex geometry in the future.

| Criteria | HelixToolkit.Wpf | HelixToolkit.Wpf.SharpDX | OpenTK (+ GLWpfControl) | SharpGL |
|----------|:-:|:-:|:-:|:-:|
| License | MIT | MIT | MIT | MIT |
| Native WPF control | Yes | Yes | Requires interop | Yes (but buffer-copy) |
| Built-in camera orbit/pan/zoom | Yes | Yes | No (manual) | No (manual) |
| Built-in MeshBuilder for primitives | Yes | Yes | No | No |
| Rendering performance ceiling | Medium | High (DirectX 11) | High (OpenGL 4.x) | Medium |
| Learning curve | Low | Medium | High | Medium-High |
| MVVM / XAML friendly | Excellent | Excellent | Poor | Fair |
| Active maintenance (2024-25) | v3 in development | v3 in development | Active (v4/v5) | Low activity |
| Suitable for NURBS tessellation | Yes | Yes | Yes (manual mesh) | Yes (manual mesh) |

### 2.2 Primary Recommendation — HelixToolkit.Wpf

**HelixToolkit.Wpf** is the recommended 3D component for v1.0 of GeoModeler3D. The rationale:

**Lowest barrier to entry.** HelixToolkit.Wpf provides a drop-in `HelixViewport3D` XAML control with built-in mouse orbit, pan, zoom, coordinate axes, and grid floor — all features that would otherwise take weeks to implement manually on top of OpenTK or SharpGL. For a team that wants to focus on the geometry domain rather than low-level graphics plumbing, this is a decisive advantage.

**MeshBuilder API.** The library ships with a `MeshBuilder` class that can generate triangle meshes for spheres, cylinders, cones, tori, arrows, tubes, pipes, and arbitrary extruded profiles. This maps directly to the primitive entity set in the requirements (Point, Triangle, Circle, Torus, Sphere, Cylinder, Cone) and dramatically reduces the code needed for the Create menu.

**XAML and MVVM integration.** Because HelixToolkit.Wpf builds on WPF's own `Media3D` infrastructure, 3D scene elements can be expressed declaratively in XAML and bound to ViewModel properties. Selection highlighting, visibility toggles, and color changes become straightforward data-binding operations rather than imperative OpenGL state changes.

**Sufficient performance for the target scope.** For scenes up to ~100K triangles — the stated requirement — WPF's Media3D pipeline running on modern hardware is more than adequate at 30+ FPS. The library is not the right choice for million-polygon real-time visualization, but that is not the v1.0 requirement.

**Clear upgrade path.** If future versions of GeoModeler3D demand higher rendering performance (large NURBS surface tessellations, GPU-based shading effects), the project can migrate to **HelixToolkit.Wpf.SharpDX**, which shares the same conceptual API but runs on a custom DirectX 11 engine. The scene-graph concepts, MeshBuilder patterns, and MVVM approach transfer directly, making the migration incremental rather than a rewrite.

**Installation:**

```
dotnet add package HelixToolkit.Wpf
```

Or for .NET Core WPF projects targeting .NET 8+, use the latest v3.x pre-release packages from NuGet. The stable v2.26 line targets .NET Framework 4.x and .NET Core 3.1+.

### 2.3 When to Consider Alternatives

**HelixToolkit.Wpf.SharpDX** — Choose this variant instead of the plain WPF version if the project needs to handle 500K+ triangles at 60 FPS, requires custom shader effects (FXAA, SSAO, transparency sorting), or needs to load complex 3D file formats via the Assimp importer. The trade-off is a higher learning curve (DirectX concepts, shader management) and a dependency on the SharpDX/DirectX runtime.

**OpenTK** — Consider OpenTK only if cross-platform support (Linux, macOS) becomes a hard requirement, or if the team has strong existing OpenGL expertise and prefers full control over the rendering pipeline. OpenTK is a low-level wrapper; it provides no scene graph, no camera controller, and no mesh builder — everything must be written from scratch. WPF integration requires either the GLWpfControl NuGet package (which depends on the NV_DX_interop extension and is not universally supported) or a manual child-window approach with Win32 interop.

**SharpGL** — SharpGL offers a WPF control and wraps OpenGL, but its built-in scene graph is limited, the project has seen low maintenance activity in recent years, and the rendering approach (copy framebuffer into a WPF image) introduces latency. It is best suited for legacy projects already committed to raw OpenGL draw calls.

---

## 3. Architecture Pattern — MVVM

The application shall follow the Model-View-ViewModel pattern enforced by the CommunityToolkit.Mvvm NuGet package (source generators for `ObservableProperty`, `RelayCommand`, etc.). Key layers:

**Model layer** contains the geometric entity classes (`IGeometricEntity`, `SphereEntity`, `CylinderEntity`, …), the scene graph, the project serializer, and the command/undo infrastructure. This layer has zero dependency on WPF or any graphics library.

**ViewModel layer** exposes observable collections of entity view-models, creation/transform commands, and viewport state (camera position, display mode). It depends on the Model layer and on abstractions (interfaces) for the rendering adapter.

**View layer** is pure XAML with minimal code-behind. The main window hosts the `HelixViewport3D` control, menus, toolbar, and properties panel. It binds to ViewModels via `DataContext`.

**Rendering adapter** sits between the ViewModel and HelixToolkit. It converts domain entities into HelixToolkit `Visual3D` objects (e.g., `MeshGeometryVisual3D`, `PointsVisual3D`). This adapter is the only layer that references HelixToolkit types, making it replaceable if the rendering backend changes.

---

## 4. Key NuGet Packages

| Package | Purpose | Version (approx.) |
|---------|---------|-------------------|
| `HelixToolkit.Wpf` | 3D viewport, camera, MeshBuilder | 3.x (or 2.26 stable) |
| `CommunityToolkit.Mvvm` | MVVM source generators, messaging | 8.x |
| `Microsoft.Extensions.DependencyInjection` | IoC container | 8.x |
| `System.Text.Json` | Project file serialization (JSON) | Built into .NET 8 |
| `Serilog` + `Serilog.Sinks.File` | Structured logging | 3.x |
| `xunit` + `Moq` | Unit testing and mocking | Latest |

---

## 5. Project File Format

Session data shall be serialized to a `.gm3d` file, which is a UTF-8 JSON document. A top-level schema:

```json
{
  "formatVersion": "1.0",
  "createdWith": "GeoModeler3D 1.0",
  "camera": { "position": [x,y,z], "lookDirection": [x,y,z], "upDirection": [x,y,z] },
  "settings": { "gridVisible": true, "displayMode": "ShadedWithEdges" },
  "entities": [
    {
      "type": "Sphere",
      "id": "guid",
      "name": "Sphere1",
      "color": "#FF0000",
      "isVisible": true,
      "layer": "Default",
      "parameters": { "center": [0,0,0], "radius": 5.0 }
    }
  ]
}
```

Using JSON instead of a binary format simplifies debugging, enables diffing in version control, and makes forward/backward schema migration tractable via explicit `formatVersion` handling. For large point clouds, a companion binary blob file (e.g., `.gm3d.bin`) can be referenced by the JSON to avoid multi-megabyte text files.

---

## 6. Serialization Strategy for Extensibility

Because new entity types will be added over time, the serializer uses a **type-discriminator** pattern: each entity writes a `"type"` string (e.g., `"Sphere"`, `"NURBSSurface"`) and a `"parameters"` dictionary. A registry maps type strings to deserializer factories. Adding a new entity type means implementing the entity class, registering its factory, and the existing save/load pipeline handles it without modification.

---

## 7. Undo/Redo Infrastructure

All mutating actions (create, delete, transform, property edit) shall be encapsulated as Command objects implementing:

```csharp
public interface IUndoableCommand
{
    string Description { get; }
    void Execute();
    void Undo();
}
```

An `UndoManager` maintains two stacks (undo and redo). Executing a new command pushes it onto the undo stack and clears the redo stack. This is a well-established pattern that scales cleanly as new operations are introduced.

---

## 8. Folder Structure (Recommended)

```
GeoModeler3D/
├── src/
│   ├── GeoModeler3D.Core/              # Model layer (entities, commands, serialization)
│   │   ├── Entities/
│   │   ├── Commands/
│   │   ├── SceneGraph/
│   │   ├── Serialization/
│   │   └── Math/                       # Vector3, Matrix4x4 helpers if needed
│   ├── GeoModeler3D.Rendering/         # Rendering adapter (HelixToolkit specifics)
│   │   ├── EntityRenderers/
│   │   └── ViewportManager.cs
│   ├── GeoModeler3D.App/               # WPF application (Views + ViewModels)
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── Resources/
│   │   └── App.xaml
│   └── GeoModeler3D.Tests/             # Unit and integration tests
├── docs/
│   ├── Requirement_Document.md
│   └── Technology_Choice.md
└── GeoModeler3D.sln
```

The critical architectural boundary is between `GeoModeler3D.Core` (no UI or graphics dependencies) and `GeoModeler3D.Rendering` (HelixToolkit dependency isolated here). This ensures that if the rendering library is ever swapped, only the Rendering project changes.

---

## 9. Future Technology Considerations

When NURBS curves and surfaces are introduced, the project will need a computational geometry library for evaluation, tessellation, and intersection. Candidates include:

- **G-SharpKit / geometry3Sharp** — open-source C# mesh processing library with useful algorithms (Delaunay, remeshing, spatial indexing).
- **Open CASCADE (via P/Invoke or a C++/CLI wrapper)** — the industrial-strength BREP kernel used by FreeCAD, suitable if the project evolves toward full solid modeling.
- **Custom implementation** — for simpler B-spline/NURBS evaluation (De Boor's algorithm), a focused in-house library may be preferable to pulling in a massive dependency.

The entity abstraction (`IGeometricEntity`) and the rendering adapter pattern already accommodate these additions: a `NURBSSurfaceEntity` would implement the interface, carry its control-point/knot data, and its renderer would tessellate to a triangle mesh for display via MeshBuilder.

---

## 10. Summary of Recommendation

For a team building a WPF-based 3D geometry tool that needs to be productive quickly, **HelixToolkit.Wpf** is the clear winner. It is free (MIT), specifically designed for WPF, provides high-level abstractions (viewport, camera, mesh building) out of the box, follows XAML/MVVM conventions, and has a natural upgrade path to DirectX 11 performance via HelixToolkit.Wpf.SharpDX when the project's needs grow. Combined with the MVVM pattern, dependency injection, and a clean entity abstraction layer, this technology stack gives GeoModeler3D a solid foundation that can evolve from simple primitives today to NURBS surfaces tomorrow.
