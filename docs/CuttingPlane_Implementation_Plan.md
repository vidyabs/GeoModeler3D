# Cutting Plane Feature — Implementation Plan

Covers **FR-36 through FR-41** from the Requirements Document (section 3.8.1).

---

## Requirements Summary

| FR | Description |
|----|-------------|
| FR-36 | Cutting plane creation: origin, normal, display size, opacity, target entity list |
| FR-37 | Visual clipping of target entities: positive side, negative side, or both-with-gap |
| FR-38 | Contour extraction: intersection curve of the plane with each target entity |
| FR-39 | Conic section classification and sampling for analytic geometry (sphere, cylinder, cone, torus) |
| FR-40 | Cross-section capping: filled polygon on the cut face |
| FR-41 | Interactive manipulation: drag/rotate the cutting plane in the viewport via a gizmo |

---

## Phase 1 — Cutting Plane Creation & Visual (FR-36) ✅ DONE

**Deliverables:**
- `CuttingPlaneEntity` with `Origin`, `Normal`, `DisplayWidth`, `DisplayHeight`, `Opacity`, `IsCappingEnabled`, `TargetEntityIds`
- `CuttingPlaneEntityRenderer` — semi-transparent quad + border outline + normal arrow
- `CreateCuttingPlaneDialog` — full creation dialog with target entity multi-select
- `CreateCuttingPlane` relay command in `MainViewModel`
- Menu item under **Create > Create Cutting Plane...**
- Properties panel template for `CuttingPlaneEntity`
- Serialization/deserialization in `EntitySerializationVisitor` + `ProjectSerializer`
- 8 unit tests in `CuttingPlaneEntityTests`

**Key files touched:**
- `src/GeoModeler3D.Core/Entities/CuttingPlaneEntity.cs`
- `src/GeoModeler3D.Rendering/EntityRenderers/CuttingPlaneEntityRenderer.cs`
- `src/GeoModeler3D.App/Views/Dialogs/CreateCuttingPlaneDialog.xaml[.cs]`
- `src/GeoModeler3D.App/ViewModels/MainViewModel.cs`
- `src/GeoModeler3D.App/Views/MainWindow.xaml[.cs]`
- `src/GeoModeler3D.App/Views/PropertiesPanel.xaml`
- `src/GeoModeler3D.Core/Serialization/EntitySerializationVisitor.cs`
- `src/GeoModeler3D.Core/Serialization/ProjectSerializer.cs`
- `src/GeoModeler3D.Tests/Entities/CuttingPlaneEntityTests.cs`

---

## Phase 2 — Visual Clipping (FR-37) ✅ DONE

**Deliverables:**
- `ClipSide` enum: `None`, `Positive`, `Negative`, `BothWithGap`
- `ClipSide` and `GapDistance` properties on `CuttingPlaneEntity`
- `CuttingPlaneVisualizer` — manages `CuttingPlaneGroup` containers from HelixToolkit.Wpf:
  - Moves target entity visuals into clip groups when a cutting plane is active
  - For `BothWithGap`: two `CuttingPlaneGroup` instances with a secondary copy of each target visual
  - Restores originals when the cutting plane is removed or set to `None`
  - Handles late-arriving target entities (added after the cutting plane)
- `RenderingService` wired to the visualizer (`Sync`, `OnEntityAdded`, `OnEntityVisualUpdated`, `OnEntityRemoved`, `Remove`)
- DI registration of `CuttingPlaneVisualizer` in `App.xaml.cs`
- Dialog updated: Clip Side ComboBox + Gap distance field (shown only for `BothWithGap`)
- Properties panel: ClipSide read-only label, GapDistance editable field
- 2 new unit tests (total: 72 passing)

**Key files touched:**
- `src/GeoModeler3D.Core/Entities/ClipSide.cs` _(new)_
- `src/GeoModeler3D.Core/Entities/CuttingPlaneEntity.cs`
- `src/GeoModeler3D.Rendering/CuttingPlaneVisualizer.cs` _(stub → full)_
- `src/GeoModeler3D.Rendering/RenderingService.cs`
- `src/GeoModeler3D.App/App.xaml.cs`
- `src/GeoModeler3D.App/Views/Dialogs/CreateCuttingPlaneDialog.xaml[.cs]`
- `src/GeoModeler3D.App/ViewModels/MainViewModel.cs`
- `src/GeoModeler3D.App/Views/PropertiesPanel.xaml`
- `src/GeoModeler3D.Core/Serialization/EntitySerializationVisitor.cs`
- `src/GeoModeler3D.Core/Serialization/ProjectSerializer.cs`
- `src/GeoModeler3D.Tests/Entities/CuttingPlaneEntityTests.cs`

**Architecture note:**
`CuttingPlaneGroup` (HelixToolkit.Wpf) clips `MeshGeometry3D` in software on the CPU. Entity visuals are physically moved from `_viewport.Children` into a `CuttingPlaneGroup` child list; `_entityVisuals` still maps to the same Visual3D object (parent changes, reference does not). Selection highlighting continues to work because it modifies `GeometryModel3D.Material` directly.

**Known limitation:** An entity can only be clipped by one cutting plane at a time (first one wins). Multi-plane clipping of the same entity is not yet supported.

---

## Phase 3 — Contour Extraction (FR-38)

**Goal:** When a cutting plane intersects a target entity, compute and display the intersection curve as a `ContourCurveEntity`.

**Deliverables:**

### Core — `src/GeoModeler3D.Core/Services/`
- `PlaneMeshIntersector` — Sutherland-Hodgman style triangle-plane intersection; returns ordered edge segments
- `ContourBuilder` — stitches edge segments into closed/open polylines
- Analytic intersectors:
  - `PlaneSphereIntersector` → circle (always a circle)
  - `PlaneCylinderIntersector` → circle or ellipse
  - `PlaneTorusIntersector` → sampled curve (Villarceau circles or general case)
  - `PlaneConeIntersector` → conic section (dispatches to Phase 4)
- `ContourExtractionService` — orchestrates the above; returns `IReadOnlyList<ContourCurveEntity>`

### Rendering — `src/GeoModeler3D.Rendering/EntityRenderers/`
- `ContourCurveEntityRenderer` — renders `ContourCurveEntity` as a `LinesVisual3D` polyline (currently a stub)

### App
- Wire contour extraction to `SceneManager.EntityChanged` for cutting planes and their targets
- Debounce: only recompute 200 ms after the last change (use a `DispatcherTimer`)
- Auto-add/remove `ContourCurveEntity` objects owned by the cutting plane

**Key files:**
- `src/GeoModeler3D.Core/Services/PlaneMeshIntersector.cs` _(new)_
- `src/GeoModeler3D.Core/Services/ContourBuilder.cs` _(new)_
- `src/GeoModeler3D.Core/Services/ContourExtractionService.cs` _(new)_
- `src/GeoModeler3D.Rendering/EntityRenderers/ContourCurveEntityRenderer.cs` _(stub → full)_
- `src/GeoModeler3D.App/Services/ContourUpdateService.cs` _(new, wired in App.xaml.cs)_

---

## Phase 4 — Conic Section Classification (FR-39)

**Goal:** For plane-cone intersections, classify and precisely sample the conic section type and set `ContourCurveEntity.ConicType`.

**Deliverables:**
- `ConicSectionClassifier` — given a plane and cone, returns `ConicSectionType` (`Circle`, `Ellipse`, `Parabola`, `Hyperbola`) based on the angle between the plane and the cone axis relative to the half-angle
- `ConeConicSampler` — produces sample points for each conic type:
  - **Circle/Ellipse**: parametric sampling using the ellipse equation
  - **Parabola**: sample along the parameter range that fits within the display
  - **Hyperbola**: sample both branches
- Wire into `ContourExtractionService` for `ConeEntity` targets

**Key files:**
- `src/GeoModeler3D.Core/Services/ConicSectionClassifier.cs` _(new)_
- `src/GeoModeler3D.Core/Services/ConeConicSampler.cs` _(new)_
- `src/GeoModeler3D.Core/Entities/ConicSectionType.cs` _(already exists)_

**Classification rules:**

| Condition | Type |
|-----------|------|
| plane ⊥ axis | Circle |
| `angle(plane, axis) > half_angle` | Ellipse |
| `angle(plane, axis) == half_angle` | Parabola |
| `angle(plane, axis) < half_angle` | Hyperbola |

---

## Phase 5 — Cross-Section Capping (FR-40)

**Goal:** When `IsCappingEnabled = true`, fill the cut face with a solid polygon (the cross-section).

**Deliverables:**
- `EarClippingTriangulator` — triangulates a 2D polygon using the ear-clipping algorithm; handles convex and simple concave polygons
- `PlaneProjector` — projects 3D contour points onto the plane's 2D coordinate system `(u, v)` for triangulation, then lifts back to 3D
- `CappingVisualGenerator` — takes a `ContourCurveEntity` and the cutting plane, returns a `MeshGeometry3D` cap face
- The cap is rendered as a child `ModelVisual3D` added to the `CuttingPlaneGroup` (so it is also clipped)
- `CuttingPlaneVisualizer` or `ContourUpdateService` manages cap lifecycle alongside contour curves

**Key files:**
- `src/GeoModeler3D.Core/Math/EarClippingTriangulator.cs` _(new)_
- `src/GeoModeler3D.Core/Math/PlaneProjector.cs` _(new)_
- `src/GeoModeler3D.Rendering/CappingVisualGenerator.cs` _(new)_

**Note:** For non-planar or multi-loop contours, triangulate each loop independently. Holes require a hole-aware triangulator (e.g., monotone partition before ear-clipping).

---

## Phase 6 — Interactive Manipulation (FR-41)

**Goal:** When a cutting plane is selected, show a 3D transform gizmo in the viewport that lets the user drag to translate or rotate it.

**Deliverables:**
- Use HelixToolkit's `CombinedManipulator` (wraps translation + rotation handles)
- `CuttingPlaneManipulator` service — attaches/detaches the manipulator when the selected entity changes
- Each drag gesture produces a `TransformEntityCommand` on mouse-up (single undo step per drag)
- During the drag, update the cutting plane entity live so clipping and contour update in real time (debounced via the Phase 3 `DispatcherTimer`)
- Wire selection change events in `SelectionManager` → manipulator attach/detach

**Key files:**
- `src/GeoModeler3D.Rendering/CuttingPlaneManipulator.cs` _(new)_
- `src/GeoModeler3D.App/Services/ManipulatorService.cs` _(new, wired in App.xaml.cs)_

**Architecture note:** The manipulator must NOT be inside the `CuttingPlaneGroup` (it is not a clipped entity). Add it directly to `_viewport.Children`. Position it at `plane.Origin`; on handle drag, compute the delta matrix and call `TransformEntityCommand`.

---

## Test Coverage Summary

| Phase | Tests added | Total |
|-------|-------------|-------|
| Phase 1 | 8 | 70 |
| Phase 2 | 2 | 72 |
| Phase 3 | ~10 (PlaneMeshIntersector, ContourBuilder, analytic intersectors) | ~82 |
| Phase 4 | ~6 (ConicSectionClassifier, ConeConicSampler) | ~88 |
| Phase 5 | ~4 (EarClippingTriangulator, PlaneProjector) | ~92 |
| Phase 6 | ~2 (command integration) | ~94 |

All tests live in `src/GeoModeler3D.Tests/` under a matching namespace folder.
