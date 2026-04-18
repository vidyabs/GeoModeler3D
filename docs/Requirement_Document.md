# Requirements Document — 3D Geometry Modeler

**Project Name:** GeoModeler3D
**Version:** 1.3
**Date:** March 2026
**Status:** Draft
**Change Log:** v1.1 — Added Section 3.8 (Animation and Cutting Planes), updated menu bar (FR-02), expanded data model, added workflows D/E/F, added animation NFRs, updated risks and glossary.
v1.2 — Added FR-22a (Triangle from multi-selected points), FR-29a (Property panel inline editing), updated FR-08 (entity list multi-select), added Workflow G (triangle creation from points).
v1.3 — Updated FR-19 (added VRML/WRL format), added FR-19a (MeshEntity aggregation — one entity per import file), added MeshEntity to Section 5 data model, added Workflow H (mesh file import), added glossary entries for MeshEntity and VRML.

---

## 1. Project Overview

GeoModeler3D is a desktop application built with C# and WPF that provides an interactive 3D rendering environment for creating, importing, manipulating, and persisting geometric entities. The application targets engineers, researchers, and students who need a lightweight tool for working with fundamental 3D geometry — points, triangles, circles, and analytic surfaces — with a clear architectural path toward supporting advanced entities such as NURBS curves and surfaces in later releases.

---

## 2. Goals and Objectives

The primary goals are to deliver a responsive 3D viewport integrated within a standard WPF shell (menu bar, toolbar, status bar), to support both file-based import and native creation of geometric entities, to provide a set of geometric operations (transform, Boolean-style, measurement), and to offer robust session persistence via save/load. The architecture must be extensible so that curves, freeform surfaces, and constraint solvers can be introduced without rewriting the core data model or rendering pipeline.

---

## 3. Functional Requirements

### 3.1 Application Shell

**FR-01 — Main Window Layout.** The application shall present a single main window composed of four regions: a menu bar at the top, a toolbar below the menu bar, a 3D viewport occupying the central area, and a status bar at the bottom. An optional dockable properties panel on the right side shall display attributes of the currently selected entity.

**FR-02 — Menu Bar.** The menu bar shall contain at minimum the following top-level menus: File (New, Open, Save, Save As, Import, Export, Recent Files, Exit), Edit (Undo, Redo, Delete, Select All), Create (Point, Triangle, Circle, Torus, Sphere, Cylinder, Cone, Cutting Plane), View (Zoom to Fit, Top/Front/Right/Isometric views, Toggle Grid, Toggle Axes), Tools (Measure Distance, Measure Angle, Transform), Animation (New Animation, Play, Pause, Stop, Timeline, Export to GIF/Video), and Help (About, Documentation).

**FR-03 — Toolbar.** The toolbar shall provide icon-based quick access to frequently used commands: selection mode, creation tools for each primitive, undo/redo, zoom-to-fit, and view orientation shortcuts.

**FR-04 — Status Bar.** The status bar shall show cursor world-coordinates (X, Y, Z), the name/type of the hovered or selected entity, the total entity count in the scene, and any active command prompts.

### 3.2 3D Viewport

**FR-05 — Rendering Window.** The application shall embed a hardware-accelerated 3D rendering control within the WPF layout. The viewport shall render all geometric entities in the scene graph with smooth interaction at a minimum of 30 FPS for scenes containing up to 100,000 triangles.

**FR-06 — Camera Controls.** The viewport shall support orbit (rotate around a target point), pan, and zoom via mouse (left-drag to orbit, middle-drag to pan, scroll-wheel to zoom). Keyboard shortcuts shall allow snapping to standard orthographic views.

**FR-07 — Visual Aids.** The viewport shall optionally display a ground-plane grid, a coordinate-axes triad, and a bounding box around selected entities.

**FR-08 — Selection.** The user shall be able to click on an entity to select it (highlighted in a distinct color), shift-click in the viewport to toggle-select, and draw a rectangular marquee to select multiple entities. In the entity list panel, Ctrl+click or Shift+click shall add or range-select entities (standard extended-selection behavior). The entity list and the viewport selection state shall remain synchronized in both directions.

**FR-09 — Rendering Modes.** The viewport shall support at minimum wireframe, shaded (flat and smooth), and shaded-with-edges display modes, toggled via the View menu or toolbar.

### 3.3 Entity Model

**FR-10 — Point Entity.** A point is defined by a 3D position (X, Y, Z). Points shall be rendered as small spheres or dots with a configurable display size.

**FR-11 — Triangle Entity.** A triangle is defined by three 3D vertices. It shall be rendered as a filled face with an optional wireframe outline. The computed normal shall be available as a read-only property.

**FR-12 — Circle Entity.** A circle is defined by a center point, a normal vector, and a radius. It shall be rendered as a tessellated polygon approximation with a configurable segment count.

**FR-13 — Torus Entity.** A torus is defined by a center, a normal (axis), a major radius, and a minor radius.

**FR-14 — Sphere Entity.** A sphere is defined by a center and a radius.

**FR-15 — Cylinder Entity.** A cylinder is defined by a base center point, an axis direction vector, a radius, and a height.

**FR-16 — Cone Entity.** A cone is defined by a base center, an axis direction, a base radius, and a height (apex at height along the axis).

**FR-17 — Entity Metadata.** Every entity shall carry a unique identifier (GUID), a user-editable name, a color, a visibility flag, and a layer/group assignment. Metadata shall be editable in the properties panel.

### 3.4 File Import

**FR-18 — Point Cloud Import.** The application shall import point data from plain-text files (CSV, TXT, XYZ) where each line contains at minimum X, Y, Z coordinates separated by a delimiter (comma, space, or tab). Optional columns for R, G, B color shall be recognized.

**FR-19 — Triangle Mesh Import.** The application shall import triangle meshes from STL (ASCII and binary), OBJ, and VRML 2.0 (.wrl) file formats. The File > Import Mesh menu item (keyboard shortcut Ctrl+I) shall open a single file dialog with a combined filter covering all supported formats. Each imported file shall be converted into a single `MeshEntity` as described in FR-19a.

**FR-19a — Single MeshEntity per Import.** Each mesh file import shall produce exactly one `MeshEntity` in the scene, named after the source file (without extension). The entity stores all imported triangles as a flat `Vector3[]` positions array (every three consecutive elements form one triangle). This ensures large meshes with thousands of triangles do not flood the entity list. The import operation shall be undoable as a single undo step (Ctrl+Z removes the entire mesh).

**FR-20 — Import Validation.** On import, the application shall validate file integrity, report parsing errors with line numbers, and allow the user to skip malformed records or abort.

### 3.5 Native Entity Creation

**FR-21 — Creation Dialogs.** For each primitive type (Point through Cone), the application shall present a dialog or input panel where the user enters the defining parameters (coordinates, radii, vectors). Default values shall be pre-populated. A live preview in the viewport shall update as parameters change.

**FR-22 — Creation via Console (Future).** The architecture shall accommodate a command-line/script console for programmatic entity creation in a future release.

**FR-22a — Triangle from Selected Points.** The user shall be able to create a `TriangleEntity` from exactly three existing `PointEntity` objects that are currently multi-selected. The workflow is: (1) select three point entities using Ctrl+click in the entity list or Shift+click in the viewport; (2) invoke **Create > Triangle from 3 Points…**; (3) a confirmation dialog shall display the name and coordinates of each selected point; (4) clicking **Create** shall validate that the points are non-collinear (cross-product magnitude ≥ 1×10⁻⁶) and distinct; (5) if validation fails the dialog shall display a descriptive error message and remain open; (6) on success, the triangle entity shall be added to the scene and the operation shall be undoable. If the selection does not contain exactly three `PointEntity` objects when the menu item is invoked, the application shall display an informational message box describing the requirement without opening the dialog.

### 3.6 Operations

**FR-23 — Transform Operations.** The user shall be able to translate, rotate (about an arbitrary axis), scale (uniform and non-uniform), and mirror selected entities. Transforms shall be specified numerically via a dialog or interactively via gizmo handles in the viewport.

**FR-24 — Duplicate.** The user shall be able to duplicate selected entities, with the copies offset by a user-specified vector.

**FR-25 — Delete.** The user shall be able to delete selected entities with undo support.

**FR-26 — Measurement.** The user shall be able to measure the Euclidean distance between two points, the angle between two vectors or normals, and the area of a selected triangle or set of triangles.

**FR-27 — Bounding Box Query.** The user shall be able to query the axis-aligned bounding box (min/max corners) of the current selection.

**FR-28 — Undo/Redo.** All entity creation, deletion, transformation, and property-change operations shall be undoable and redoable through a linear undo stack with a configurable depth (default 50).

**FR-29a — Properties Panel Inline Editing.** The properties panel shall allow the user to edit any geometric property of the selected entity (e.g., center coordinates, radius, height, vertex positions, name, layer, color) directly in the panel without opening a separate dialog. Edits shall be committed on focus-loss or Enter key. Each committed edit shall be recorded as a `ChangePropertyCommand` on the undo stack so that it is reversible with Ctrl+Z. Invalid input (non-numeric text for numeric fields) shall be silently discarded and the field shall revert to the current entity value. Read-only computed properties (e.g., HalfAngle, Normal, Area) shall be displayed but not editable.

### 3.7 Session Persistence

**FR-29 — Save/Load Project.** The application shall save the entire scene state (all entities with their properties, camera position, display settings) to a custom project file format (e.g., `.gm3d`). The file format shall be JSON-based to facilitate debugging, version control diffing, and future schema migration.

**FR-30 — Auto-Save.** The application shall optionally auto-save to a recovery file at a user-configurable interval (default 5 minutes).

**FR-31 — Export.** The application shall export visible entities to STL (ASCII/binary), OBJ, and PLY formats.

### 3.8 Animation and Cutting Planes

#### 3.8.1 Cutting Plane Entity

**FR-36 — Cutting Plane Creation.** The user shall be able to create a cutting plane defined by a point (origin) and a normal vector. The cutting plane shall be rendered as a semi-transparent rectangular visual in the viewport, with configurable size and opacity. The plane shall be creatable via the Create menu or toolbar.

**FR-37 — Visual Clipping.** When a cutting plane is applied to one or more target entities, the viewport shall visually clip the target geometry along the plane in real time. The user shall be able to choose which side of the plane is retained (positive normal side, negative normal side, or both with a gap). Multiple cutting planes (up to 8) shall be supported simultaneously on a single entity or group.

**FR-38 — Contour Extraction.** The application shall compute the intersection contour (cross-section curve) where a cutting plane meets a mesh entity. The contour shall be returned as an ordered set of 3D line segments and rendered as a highlighted polyline in the viewport. For analytic entities (cone, cylinder, sphere, torus), the contour represents the exact mathematical cross-section (e.g., conic sections for a cone).

**FR-39 — Conic Section Generation.** When a cutting plane intersects a cone entity, the application shall identify the resulting conic section type (circle, ellipse, parabola, or hyperbola) based on the angle between the plane normal and the cone axis. The extracted contour curve shall be displayed as a distinct entity that the user can name, color, and persist independently.

**FR-40 — Cross-Section Capping.** Optionally, the application shall fill (cap) the planar opening created by the cut with a triangulated surface, producing a closed solid appearance rather than a hollow shell. Capping shall use an ear-clipping or similar polygon triangulation algorithm on the extracted contour.

**FR-41 — Interactive Cutting Plane Manipulation.** The user shall be able to translate and rotate a cutting plane interactively using a gizmo or by dragging the plane visual directly. The clipping result and contour shall update in real time as the plane moves.

#### 3.8.2 Animation System

**FR-42 — Animation Definition.** The user shall be able to define an animation as an ordered sequence of keyframes. Each keyframe specifies a time (in seconds) and a set of entity property values (position, rotation, scale, cutting-plane origin, cutting-plane normal, visibility, color, opacity). The animation system shall interpolate between keyframes using configurable easing (linear, ease-in, ease-out, ease-in-out).

**FR-43 — Timeline Panel.** The application shall provide a timeline UI panel (dockable, below or beside the viewport) that displays all active animations as horizontal tracks. Each track represents one entity's animated property. Keyframe markers shall be visible on the track and shall be draggable to adjust timing. The timeline shall show a playhead indicating the current time.

**FR-44 — Playback Controls.** The application shall provide Play, Pause, Stop, and Scrub controls. Play advances the playhead forward at real time (1×) or at a user-selected speed multiplier (0.25×, 0.5×, 1×, 2×, 4×). Stop resets the playhead to time zero and restores all entities to their initial state. Scrub allows the user to drag the playhead to any point on the timeline to preview the scene at that instant.

**FR-45 — Entity Motion Animation.** The user shall be able to animate the translation, rotation, and scale of any entity along a path or between keyframe poses. For example, translating a plane entity through a torus at a constant velocity, or rotating a cutting plane around a cone's apex to sweep through conic section angles.

**FR-46 — Cutting Plane Sweep Animation.** The user shall be able to animate a cutting plane's origin (translation along its normal or along an arbitrary axis) and its normal direction (rotation). As the cutting plane moves, both the visual clipping and the extracted contour curve shall update each frame. This enables demonstrations such as sweeping a plane through a cone to visualize the transition from circle → ellipse → parabola → hyperbola.

**FR-47 — Camera Animation.** The user shall be able to animate the camera's position, look-direction, and up-direction between keyframes, enabling fly-through or orbit sequences that can be combined with entity animations.

**FR-48 — Animation Preview.** During animation playback, the viewport shall maintain ≥ 24 FPS for scenes within the standard complexity budget (100,000 triangles). For scenes where contour recomputation causes frame drops, the application shall offer a "draft" mode that skips contour updates and only shows visual clipping.

**FR-49 — Animation Export.** The user shall be able to export an animation as a sequence of PNG frames at a user-specified resolution and frame rate (e.g., 1920×1080 at 30 FPS). An optional post-export step shall assemble frames into an MP4 video or animated GIF using an integrated or external encoder (e.g., FFmpeg).

**FR-50 — Animation Persistence.** Animation definitions (keyframes, tracks, timing, easing) shall be saved as part of the `.gm3d` project file. When a project is reopened, all animations shall be fully restorable and replayable.

#### 3.8.3 Slider-Driven Interactive Exploration

**FR-51 — Parameter Sliders.** As an alternative to timeline-based animation, the user shall be able to bind a slider control to any single animatable property (e.g., cutting-plane offset distance, rotation angle). Dragging the slider shall update the viewport in real time. This provides a lightweight, interactive way to explore geometric relationships (e.g., sliding a plane through a torus) without defining a full keyframed animation.

**FR-52 — Linked Sliders.** Multiple sliders shall be linkable so that adjusting one parameter proportionally drives another (e.g., simultaneously translating and rotating a cutting plane to follow a helical path through an entity).

### 3.9 Extensibility Provisions

**FR-53 — Entity Abstraction.** The entity data model shall define an abstract base class (or interface) from which all concrete entity types inherit. Adding a new entity type (e.g., BSplineCurve, NURBSSurface) shall require implementing the interface and registering a factory/renderer, without modifying existing entity code.

**FR-54 — Operation Abstraction.** Operations shall follow the Command pattern so that new operations can be added by implementing a command interface with Execute/Undo methods.

**FR-55 — Rendering Abstraction.** Each entity type shall have a corresponding renderer component that converts the entity's mathematical definition into renderable mesh data. This decouples the geometric model from the graphics API.

**FR-56 — Plugin Architecture (Future).** The architecture shall be structured so that a plugin system (MEF, or a custom assembly-loading mechanism) can be introduced to allow third-party entity types and operations without recompilation of the core.

**FR-57 — Animation Abstraction.** The animation system shall define an `IAnimationTrack` interface so that new interpolation strategies (e.g., spline-based paths, physics-driven motion) and new animatable property types can be added without modifying the core animation engine.

---

## 4. Non-Functional Requirements

**NFR-01 — Performance.** The viewport shall maintain ≥ 30 FPS with 100,000 rendered triangles on a mid-range GPU (e.g., NVIDIA GTX 1650 or equivalent integrated graphics). Entity creation and file import for files up to 1 million points shall complete in under 10 seconds.

**NFR-02 — Animation Performance.** During animation playback, the viewport shall maintain ≥ 24 FPS in "full quality" mode (visual clipping + contour extraction) for scenes up to 50,000 triangles, and ≥ 30 FPS in "draft" mode (visual clipping only, contour extraction deferred) for scenes up to 100,000 triangles. Contour extraction for a single cutting plane on a 50,000-triangle mesh shall complete in under 40 ms per frame.

**NFR-03 — Memory.** Peak RAM usage shall not exceed 1 GB for a scene of 1 million triangles. Animation data (keyframes, tracks) for a 60-second sequence with 10 animated entities shall not exceed 10 MB of additional memory.

**NFR-04 — Startup Time.** The application shall reach a usable state within 3 seconds on an SSD-equipped machine.

**NFR-05 — Platform.** Windows 10 (version 1903) and later. .NET 8 (LTS) or later.

**NFR-06 — Usability.** The application shall follow standard Windows UX conventions (keyboard shortcuts, context menus, drag-and-drop). Tooltips shall be provided for all toolbar icons and menu items. Animation controls shall follow familiar media-player conventions (spacebar for play/pause, arrow keys for frame stepping).

**NFR-07 — Maintainability.** The codebase shall follow MVVM architecture, use dependency injection (e.g., Microsoft.Extensions.DependencyInjection), and maintain ≥ 70% unit-test coverage for the domain/model layer. The animation engine shall be testable independently of the rendering pipeline.

**NFR-08 — Accessibility.** The application shall support high-contrast themes and keyboard-only navigation for all dialogs, menus, and the animation timeline.

---

## 5. Data Model (Conceptual)

```
IGeometricEntity (interface)
├── Id : Guid
├── Name : string
├── Color : Color
├── IsVisible : bool
├── Layer : string
├── BoundingBox : AxisAlignedBoundingBox
├── Transform(matrix : Matrix4x4) : void
├── Clone() : IGeometricEntity
├── Accept(visitor : IEntityVisitor) : void   ← Visitor pattern for operations
│
├── PointEntity : IGeometricEntity
│     └── Position : Vector3
│
├── TriangleEntity : IGeometricEntity
│     └── Vertices : Vector3[3]
│     └── Normal : Vector3 (computed)
│
├── CircleEntity : IGeometricEntity
│     └── Center : Vector3
│     └── Normal : Vector3
│     └── Radius : double
│
├── TorusEntity : IGeometricEntity
│     └── Center, Normal, MajorRadius, MinorRadius
│
├── SphereEntity : IGeometricEntity
│     └── Center : Vector3
│     └── Radius : double
│
├── CylinderEntity : IGeometricEntity
│     └── BaseCenter, Axis, Radius, Height
│
├── ConeEntity : IGeometricEntity
│     └── BaseCenter, Axis, BaseRadius, Height
│
├── MeshEntity : IGeometricEntity
│     └── Positions : IReadOnlyList<Vector3>  ← flat array; every 3 entries = one triangle
│     └── TriangleCount : int (computed)
│
├── CuttingPlaneEntity : IGeometricEntity
│     └── Origin : Vector3
│     └── Normal : Vector3
│     └── DisplayWidth : double
│     └── DisplayHeight : double
│     └── Opacity : double
│     └── TargetEntityIds : List<Guid>
│     └── IsCappingEnabled : bool
│     └── ContourCurve : List<Vector3> (computed, read-only)
│
├── ContourCurveEntity : IGeometricEntity        ← extracted cross-section
│     └── Points : List<Vector3>
│     └── IsClosed : bool
│     └── SourcePlaneId : Guid
│     └── SourceEntityId : Guid
│     └── ConicType : ConicSectionType?          ← Circle | Ellipse | Parabola | Hyperbola | None
│
└── (Future) CurveEntity, NURBSSurfaceEntity ...


AnimationModel
├── AnimationSequence
│     └── Id : Guid
│     └── Name : string
│     └── Duration : TimeSpan
│     └── IsLooping : bool
│     └── Tracks : List<IAnimationTrack>
│
├── IAnimationTrack (interface)
│     └── TargetEntityId : Guid
│     └── PropertyName : string                  ← e.g., "Position", "Normal", "CameraPosition"
│     └── Keyframes : SortedList<double, Keyframe>
│     └── EasingMode : EasingType
│     └── Evaluate(time : double) : object
│
├── Keyframe
│     └── Time : double (seconds)
│     └── Value : object                         ← Vector3, Quaternion, double, Color, etc.
│     └── EasingIn : EasingType
│     └── EasingOut : EasingType
│
├── TranslationTrack : IAnimationTrack           ← interpolates Vector3 positions
├── RotationTrack : IAnimationTrack              ← interpolates Quaternion orientations
├── ScalarTrack : IAnimationTrack                ← interpolates double values (radius, opacity)
├── CameraTrack : IAnimationTrack                ← interpolates camera position + look direction

│
└── EasingType : enum { Linear, EaseIn, EaseOut, EaseInOut, CubicBezier }
```

---

## 6. User Workflows

**Workflow A — Import and Inspect.** User launches app → File > Import → selects a CSV point cloud → points appear in viewport → user orbits/zooms to inspect → user selects subset → Properties panel shows coordinates → user saves project.

**Workflow B — Create and Transform.** User launches app → Create > Sphere (enters center and radius) → sphere appears → user duplicates sphere → translates copy → Create > Cylinder → positions cylinder between spheres → saves project.

**Workflow C — Reopen and Extend.** User opens a saved `.gm3d` file → all entities and camera restore → user imports an STL mesh → performs measurements → exports combined scene as OBJ.

**Workflow D — Conic Section Exploration.** User creates a cone (Create > Cone) → creates a cutting plane (Create > Cutting Plane) → positions the plane perpendicular to the cone axis → contour shows a circle → user drags a slider to tilt the plane angle → contour transitions through ellipse → parabola → hyperbola → user extracts each contour as a named entity → saves project.

**Workflow E — Animated Cutting-Plane Sweep.** User creates a torus → creates a cutting plane → opens Animation > New Animation → adds a translation track that moves the plane along the torus axis from one end to the other over 10 seconds → adds a rotation track that tilts the plane 45° mid-way → presses Play → watches the cross-section evolve in real time → exports animation as MP4 video.

**Workflow F — Interactive Teaching Demo.** User creates a sphere and a cone side by side → creates two cutting planes, one for each → binds each plane's offset to a slider → opens a second slider bound to the cone's cutting-plane rotation angle → drags sliders to explore how the cross-section shape changes → captures key frames as screenshots for a presentation.

**Workflow G — Triangle from Existing Points.** User creates three `PointEntity` objects at known positions (e.g., (1,0,0), (0,1,0), (0,0,1)) → Ctrl+clicks all three in the entity list to multi-select → clicks **Create > Triangle from 3 Points…** → confirms the confirmation dialog showing each point's name and coordinates → triangle appears in the viewport → selects the triangle → edits a vertex coordinate in the Properties panel to fine-tune the geometry → Ctrl+Z to undo the vertex edit if needed.

**Workflow H — Mesh File Import.** User opens a project → **File > Import Mesh…** (or Ctrl+I) → file dialog opens filtered to `*.stl;*.obj;*.wrl` → user selects a binary STL file of a mechanical part → application validates the file (checks binary size formula) → parses all triangles → creates one `MeshEntity` named after the file (e.g., "bracket") → entity appears in the entity list as a single item and renders in the viewport → Properties panel shows the triangle count as a read-only field → user toggles visibility via the checkbox to hide/show the mesh → Ctrl+Z removes the entire mesh in one undo step → Ctrl+Y re-adds it.

---

## 7. Constraints and Assumptions

The application is a single-user, single-document desktop tool (not collaborative or cloud-based). Multi-document support (MDI) is out of scope for v1.0 but the architecture should not preclude it. The application assumes a GPU with at least OpenGL 3.3 or DirectX 11 feature-level support. The initial release does not include Boolean operations on solid meshes; this is deferred to a later version alongside NURBS support.

---

## 8. Risks

The main risks are: performance degradation when rendering very large imported meshes (mitigated by level-of-detail and frustum culling), complexity of undo/redo across heterogeneous operations (mitigated by strict use of the Command pattern from day one), the learning curve of the chosen 3D toolkit for the development team (mitigated by selecting a well-documented library with rich examples — see Technology Choice document), frame-rate drops during animated cutting-plane sweeps caused by per-frame contour recomputation on high-polygon meshes (mitigated by offering a "draft" playback mode that shows only visual clipping without contour extraction, and by detaching/reattaching meshes from the visual tree during batch updates), and synchronization complexity between the animation timeline, the undo/redo stack, and the scene graph (mitigated by treating each complete animation playback as a single undoable macro-command rather than recording individual per-frame changes).

---

## 9. Glossary

| Term | Definition |
|------|-----------|
| Entity | Any geometric object in the scene (point, triangle, surface, etc.) |
| Scene Graph | The hierarchical data structure holding all entities |
| Viewport | The 3D rendering area of the application window |
| Tessellation | Converting an analytic shape (sphere, torus) into a triangle mesh for rendering |
| NURBS | Non-Uniform Rational B-Spline — a mathematical model for curves/surfaces |
| MVVM | Model-View-ViewModel — the standard WPF architectural pattern |
| Gizmo | An interactive on-screen handle for translating, rotating, or scaling objects |
| Cutting Plane | A mathematical plane used to visually clip geometry and extract cross-section contours |
| Contour | The intersection curve (polyline) produced where a cutting plane meets a mesh surface |
| Conic Section | A curve produced by intersecting a plane with a cone: circle, ellipse, parabola, or hyperbola |
| Keyframe | A snapshot of one or more entity properties at a specific point in time, used for animation |
| Animation Track | A channel that records keyframes for a single property of a single entity over time |
| Timeline | The UI panel showing animation tracks, keyframes, and the playhead position |
| Playhead | The marker on the timeline indicating the current animation time |
| Easing | A function controlling the acceleration curve between keyframes (linear, ease-in, etc.) |
| Capping | Filling the open face created by a planar cut with a triangulated surface to produce a closed solid |
| Ear Clipping | A polygon triangulation algorithm used to mesh planar contours for cross-section capping |
| MeshEntity | An entity that stores a triangulated mesh as a flat `Vector3[]` positions array; created by importing an STL, OBJ, or WRL file; represented as a single item in the entity list regardless of triangle count |
| VRML / WRL | Virtual Reality Modeling Language 2.0 — an open text-based 3D scene format; files use the `.wrl` extension; the application reads `IndexedFaceSet` nodes to extract triangulated geometry |
