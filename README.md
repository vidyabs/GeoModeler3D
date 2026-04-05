# GeoModeler3D

A 3D geometry modeler built with C# / WPF and HelixToolkit. Designed for interactive creation, inspection, and manipulation of fundamental geometric primitives — useful for point-cloud diagnostics, mesh inspection, and geometric education. 

---

## Features

### Entity Types
- **Point** — 3D position, rendered as a sphere marker
- **Sphere** — center + radius
- **Cylinder** — base center, axis, radius, height
- **Cone** — base center, axis, base radius, height (computed half-angle)
- **Torus** — center, normal, major radius, minor radius
- **Triangle** — three vertices (V0, V1, V2); computed normal and area

### Creation
- Parameter dialogs for all primitives (pre-populated defaults)
- **Triangle from 3 selected points** — multi-select three existing `PointEntity` objects, then **Create > Triangle from 3 Points...**; validates distinct positions and non-collinear geometry before creating

### Editing (Properties Panel)
- Inline editing of all geometric properties after creation (center, radius, axis, vertex positions, name, color, layer)
- Edits commit on Enter or focus-loss and are individually **undoable**
- Computed properties (Normal, Area, HalfAngle) are displayed read-only

### Selection
- Single-click in viewport or entity list
- **Multi-select**: Ctrl+click / Shift+click in the entity list; Shift+click in the viewport
- Selected entities highlighted yellow in the 3D view

### Undo / Redo
- Full undo stack (depth 50) for create, delete, and every property edit
- `Ctrl+Z` / `Ctrl+Y`

### File Persistence
- Save / Open project files (`.geo3d` / `.json`, human-readable JSON)
- `Ctrl+S` / `Ctrl+Shift+S` / `Ctrl+O`

### Viewport
- Orbit, pan, zoom with mouse
- Camera presets: Top, Front, Right, Isometric
- Toggle grid and coordinate axes
- Zoom to fit

---

## Quick Start

**Prerequisites:** .NET 10 SDK, Windows 10/11

```sh
git clone <repo>
cd GeoModeler3D
dotnet build
dotnet run --project src/GeoModeler3D.App
```

See [docs/Demo_Walkthrough.md](docs/Demo_Walkthrough.md) for a full feature tour.

---

## Architecture

```
GeoModeler3D.Core        -- entities, scene graph, commands (no UI deps)
GeoModeler3D.Rendering   -- HelixToolkit renderers, RenderingService
GeoModeler3D.App         -- WPF shell, MVVM, dialogs, DI
GeoModeler3D.Tests       -- xUnit unit tests (Core only)
```

See [docs/Developer_Guide.md](docs/Developer_Guide.md) for a deep-dive into the scene graph, rendering pipeline, command system, and how to add new entity types.

---

## Documentation

| Document | Description |
|---|---|
| [Requirement_Document.md](docs/Requirement_Document.md) | Full functional and non-functional requirements |
| [Architecture.md](docs/Architecture.md) | High-level architecture and design patterns |
| [Developer_Guide.md](docs/Developer_Guide.md) | Developer deep-dive: scene graph, rendering, commands, DI |
| [Demo_Walkthrough.md](docs/Demo_Walkthrough.md) | Step-by-step feature walkthrough and demo script |
| [Implementation_Plan.md](docs/Implementation_Plan.md) | Phased implementation plan |
| [Technology_Choice.md](docs/Technology_Choice.md) | Technology selection rationale |

---

## Diagram Tools

Class diagrams and UML are authored in [Mermaid](https://mermaid.live/).

- **Option 1**: Install the Mermaid preview extension in VS Code
- **Option 2**: Use the online editor at https://mermaid.live/
- **Option 3 (CLI)**: `npm i -g @mermaid-js/mermaid-cli` then `mmdc -i docs/diagram.mmd -o docs/diagram.svg`


# Prompts
Build an implementation plan based on the requirement and architecture diagram so that a simple demo ready code exists. I will use Visual Studio 2026 and .Net 10 to build and run.

Add new feature that allows to import triangulation of various open formats like .stl, .obj and .wrl. show the triangulation as single entity in the list. When selected show the property: number of triangles. Allow to save the triangulation as part of serialisation. support desrialisation of triangulation. Support both ascii and binary format, if valid. create implementation plan.