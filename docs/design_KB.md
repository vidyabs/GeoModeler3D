# GeoModeler3D — Cutting Plane Design Knowledge Base

Covers **Phases 1–4** of the cutting plane feature (FR-36 to FR-39).  
Each phase is self-contained: read only the section you need.

---

## Contents

1. [Architecture at a Glance](#1-architecture-at-a-glance)
2. [Phase 1 — Cutting Plane Entity & Visual (FR-36)](#2-phase-1--cutting-plane-entity--visual-fr-36)
3. [Phase 2 — Visual Clipping (FR-37)](#3-phase-2--visual-clipping-fr-37)
4. [Phase 3 — Contour Extraction (FR-38)](#4-phase-3--contour-extraction-fr-38)
5. [Phase 4 — Conic Section Classification (FR-39)](#5-phase-4--conic-section-classification-fr-39)
   - [5.4 Ear Clipping Triangulator](#54-ear-clipping-triangulator)
   - [5.5 Interactive Manipulation — Cutting Plane Gizmo (FR-41)](#55-interactive-manipulation--cutting-plane-gizmo-fr-41)
6. [Data Flow Summary](#6-data-flow-summary)
7. [How to Extend](#7-how-to-extend)

---

## 1. Architecture at a Glance

```
GeoModeler3D.Core          — entities, math, services. No WPF.
GeoModeler3D.Rendering     — IEntityRenderer, RenderingService, HelixToolkit visuals.
GeoModeler3D.App           — WPF shell, ViewModels, App-layer services, DI wiring.
GeoModeler3D.Tests         — xUnit tests for Core only.
```

Dependency direction is strictly one-way:

```
App  →  Rendering  →  Core
Tests               →  Core
```

**Key singleton services (registered in `App.xaml.cs`):**

| Service | Layer | Role |
|---|---|---|
| `SceneManager` | Core | Entity store; fires `EntityAdded/Changed/Removed` events |
| `RenderingService` | Rendering | Creates/updates/removes `Visual3D` for each entity |
| `CuttingPlaneVisualizer` | Rendering | Manages `CuttingPlaneGroup` clip containers |
| `ContourExtractionService` | Core | Computes intersection curves analytically |
| `ContourUpdateService` | App | Debounce timer → triggers contour recompute on scene changes |
| `CappingService` | App | Generates filled cap visuals when `IsCappingEnabled` |

**Entity change lifecycle:**

```
User edits property
  → entity fires PropertyChanged
    → SceneManager raises EntityChanged(entity)
      → RenderingService.UpdateEntity(entity)  [updates Visual3D]
      → ContourUpdateService.OnEntityChanged   [schedules recompute]
      → CappingService.OnEntityChanged         [rebuilds cap if needed]
```

---

## 2. Phase 1 — Cutting Plane Entity & Visual (FR-36)

### 2.1 The Entity

**File:** `src/GeoModeler3D.Core/Entities/CuttingPlaneEntity.cs`

```
CuttingPlaneEntity : EntityBase
  Origin          : Vector3          — a point on the plane
  Normal          : Vector3          — always unit-length (normalised in setter)
  DisplayWidth    : double = 10.0    — visual quad width
  DisplayHeight   : double = 10.0    — visual quad height
  Opacity         : double = 0.3     — visual transparency (0–1)
  IsCappingEnabled: bool  = false    — Phase 5: fill cut faces
  ClipSide        : ClipSide = None  — Phase 2: which side to clip
  GapDistance     : double = 0.5     — Phase 2: gap for BothWithGap
  TargetEntityIds : List<Guid>       — entities this plane intersects
```

`TargetEntityIds` is a plain `List<Guid>` (not an observable collection). Adding/removing IDs does **not** automatically raise change events. After modifying the list the caller must signal a change explicitly (or re-execute the command that built the entity).

**Tangent basis** — the renderer and all intersection algorithms use `PlaneEntity.ComputeTangents(normal)`:

```csharp
// src/GeoModeler3D.Core/Entities/PlaneEntity.cs
public static (Vector3 u, Vector3 v) ComputeTangents(Vector3 normal)
{
    // Choose an "up" vector not parallel to the normal
    var up = Abs(Dot(normal, UnitZ)) < 0.99f ? UnitZ : UnitX;
    var u = Normalize(Cross(normal, up));
    var v = Cross(normal, u);          // already unit length
    return (u, v);
}
```

The pair `(u, v, normal)` forms a right-handed orthonormal frame centred at `Origin`. Every point on the plane can be written as `Origin + s·u + t·v`.

### 2.2 Renderer

**File:** `src/GeoModeler3D.Rendering/EntityRenderers/CuttingPlaneEntityRenderer.cs`

The renderer builds a `ModelVisual3D` with three child visuals on every `Apply` call:

```
ModelVisual3D (container)
  ├── Content = GeometryModel3D        ← semi-transparent quad (double-sided)
  ├── Children[0] = LinesVisual3D     ← cornflower-blue border outline
  └── Children[1] = ArrowVisual3D    ← normal direction arrow
```

**Quad construction:**

```
p0 = Origin − hw·u − hh·v     (bottom-left)
p1 = Origin + hw·u − hh·v     (bottom-right)
p2 = Origin + hw·u + hh·v     (top-right)
p3 = Origin − hw·u + hh·v     (top-left)

TriangleIndices (double-sided):
  Front:  0,1,2  0,2,3
  Back:   2,1,0  3,2,0
```

**Visibility rule for `ModelVisual3D`:** WPF 3D `Visual3D` has no `Visibility` property. Set `visual.Content = null` to hide, rebuild content to show.

### 2.3 Serialization

**Serialise** (`EntitySerializationVisitor.Visit(CuttingPlaneEntity)`):

```json
{
  "type": "CuttingPlane",
  "id": "...",
  "name": "...",
  "color": "#RRGGBBAA",
  "isVisible": true,
  "layer": "Default",
  "origin": { "x": 0, "y": 0, "z": 0 },
  "normal": { "x": 0, "y": 0, "z": 1 },
  "displayWidth": 10.0,
  "displayHeight": 10.0,
  "opacity": 0.3,
  "isCappingEnabled": false,
  "clipSide": "None",
  "gapDistance": 0.5,
  "targetEntityIds": ["guid1", "guid2"]
}
```

**Deserialise** (`ProjectSerializer.DeserializeCuttingPlane`): reads all fields with `TryGetProperty` (graceful on missing keys for forward compatibility). `clipSide` is parsed via `Enum.TryParse<ClipSide>`.

**ContourCurveEntity** objects are computed at runtime and **skipped** during save (`ProjectSerializer.Save` has an explicit `if (entity is ContourCurveEntity) continue;`). They are regenerated after load by `ContourUpdateService`.

### 2.4 Adding a New Property to `CuttingPlaneEntity`

1. Add a private backing field + public property with `SetField`.
2. Set it in `Clone()`.
3. Emit it in `EntitySerializationVisitor.Visit(CuttingPlaneEntity)`.
4. Read it in `ProjectSerializer.DeserializeCuttingPlane` with `TryGetProperty`.
5. Update the properties panel template in `PropertiesPanel.xaml`.
6. Update `CuttingPlaneEntityTests` (constructor, clone, serialize round-trip tests).

---

## 3. Phase 2 — Visual Clipping (FR-37)

### 3.1 ClipSide Enum

**File:** `src/GeoModeler3D.Core/Entities/ClipSide.cs`

```
None        = 0   — no clipping; plane is purely visual
Positive    = 1   — keep geometry on the normal-pointing side
Negative    = 2   — keep geometry on the opposite side
BothWithGap = 3   — show both halves with a gap between them
```

`ClipSide.Positive` means the HelixToolkit `CuttingPlaneGroup` keeps what is on the **positive** side of the plane equation `dot(P − Origin, Normal) > 0`.

### 3.2 How HelixToolkit CuttingPlaneGroup Works

`CuttingPlaneGroup` is a `ModelVisual3D` subclass in HelixToolkit.Wpf. It uses **software clipping** on the CPU:

- Any `MeshGeometry3D` inside its `Children` is clipped to the half-spaces defined by its `CuttingPlanes` collection.
- A `Plane3D` in HelixToolkit takes a `Point3D position` and `Vector3D normal`; geometry on the **positive** side of the plane (same direction as the normal) is **kept**.
- Moving a `Visual3D` from `viewport.Children` into `group.Children` subjects it to the clipping.

```
Positive clip group:
  CuttingPlanes.Add( new Plane3D(origin, +normal) )   ← keeps positive side

Negative clip group:
  CuttingPlanes.Add( new Plane3D(origin, −normal) )   ← keeps negative side (flip normal)

BothWithGap:
  group1: new Plane3D(origin + offset, +normal)        ← positive half, shifted inward
  group2: new Plane3D(origin − offset, −normal)        ← negative half, shifted inward
  where offset = Normal * (GapDistance / 2)
```

### 3.3 CuttingPlaneVisualizer Algorithm

**File:** `src/GeoModeler3D.Rendering/CuttingPlaneVisualizer.cs`

The visualiser owns two dictionaries:

```
_states      : Dictionary<Guid, PlaneState>    — keyed by CuttingPlane.Id
_entityToPlane: Dictionary<Guid, Guid>         — entityId → planeId (reverse map)
```

`PlaneState` holds:
- `Groups : List<CuttingPlaneGroup>` — 1 group for Positive/Negative, 2 for BothWithGap
- `SecondaryVisuals : Dictionary<Guid, Visual3D>` — copies for group2 (BothWithGap only)
- `TrackedEntities : Dictionary<Guid, IGeometricEntity>` — entities currently moved into groups

**`Sync(plane)` — full rebuild for one cutting plane:**

```
1. TeardownState(plane.Id)
     For each tracked entity:
       remove Visual3D from group1
       move it back to _viewport.Children
       call renderer.UpdateVisual to refresh geometry
     Remove all groups from _viewport
     Delete _states[plane.Id]

2. If ClipSide == None OR !plane.IsVisible: return (no clipping)

3. BuildState(plane):
     Create group1 (+ maybe group2) with CuttingPlane math above.
     For each targetId in plane.TargetEntityIds:
       if entity not in scene OR no Visual3D exists: skip
       Remove primaryVisual from _viewport.Children
       Add primaryVisual to group1.Children
       If BothWithGap:
         Create a secondary Visual3D via renderer.CreateVisual(entity)
         Add secondary to group2.Children
     Add group1 (and group2) to _viewport.Children
     Store PlaneState

4. Record _entityToPlane[tid] = plane.Id for every target
   (even if BuildState returns null — for late-arriving targets)
```

**`OnEntityAdded(entityId)` — handles late-arriving targets:**

```
If entityId is in _entityToPlane:
  If no active PlaneState for that plane yet:
    Look up the plane entity and call Sync(plane)
```

This covers the case where a cutting plane is created before all its targets exist in the scene.

**`OnEntityVisualUpdated(entity)` — keeps secondary visuals in sync:**

```
For each PlaneState:
  If entity.Id is in state.SecondaryVisuals:
    Call renderer.UpdateVisual(entity, secondaryVisual)
```

**`OnEntityRemoved(entityId)` — clean up one target:**

```
Remove entityId from _entityToPlane
For each PlaneState that tracks this entity:
  Remove primaryVisual from group1
  Remove secondaryVisual from group2 (if exists)
  Remove from state.TrackedEntities and state.SecondaryVisuals
```

**`Remove(planeId)` — called when a CuttingPlane entity is deleted:**

```
Remove all _entityToPlane entries pointing to this planeId
Call TeardownState(planeId)
```

### 3.4 Known Limitation

An entity can only be in **one** `CuttingPlaneGroup` at a time (WPF visual tree constraint: a `Visual3D` can have only one parent). If two cutting planes target the same entity, the first one wins. The second `Sync` call finds the visual already missing from `_viewport.Children` and skips it.

### 3.5 RenderingService Wiring

`RenderingService` calls into `CuttingPlaneVisualizer` at these points:

| `RenderingService` method | Visualizer call |
|---|---|
| `AddEntity(CuttingPlaneEntity)` | `Sync(cp)` |
| `AddEntity(other)` | `OnEntityAdded(entity.Id)` |
| `UpdateEntity(CuttingPlaneEntity)` | `Sync(cp)` |
| `UpdateEntity(other)` | `OnEntityVisualUpdated(entity)` |
| `RemoveEntity(id)` | `OnEntityRemoved(id)` then `Remove(id)` |

`Remove(id)` is a no-op when `id` is not a cutting plane (no entry in `_states`), so it is safe to call unconditionally.

---

## 4. Phase 3 — Contour Extraction (FR-38)

### 4.1 Overall Flow

```
User changes cutting plane or target entity
  → ContourUpdateService schedules recompute (200 ms debounce)
  → RecomputeAll():
      Remove all ContourCurveEntity objects from SceneManager
      For each CuttingPlaneEntity in scene:
        For each targetId in plane.TargetEntityIds:
          call ContourExtractionService.Extract(plane, entity)
          Add returned ContourCurveEntity objects to SceneManager
      RenderingService creates LinesVisual3D for each new ContourCurveEntity
```

### 4.2 PlaneMeshIntersector — Triangle-Plane Intersection

**File:** `src/GeoModeler3D.Core/Services/PlaneMeshIntersector.cs`

Input: a flat `Vector3[]` where every 3 consecutive vertices form one triangle.  
Output: list of `(Vector3 A, Vector3 B)` segments — one per intersected triangle.

**Per-triangle algorithm:**

```
For each triangle (v0, v1, v2):
  Compute signed distances: d0 = dot(v0 − Origin, Normal)
                            d1 = dot(v1 − Origin, Normal)
                            d2 = dot(v2 − Origin, Normal)

  Collect crossing points by calling AddCrossing for each edge:
    edge (v0→v1), edge (v1→v2), edge (v2→v0)

  If 2 or more crossing points:
    Emit segment (pts[0], pts[last])
    Skip if segment length² < 1e-12  (degenerate)
```

**`AddCrossing(vA, dA, vB, dB, pts)` — one edge:**

```
tol = 1e-7

If |dA| ≤ tol:                 vA is ON the plane → add vA, return
If |dB| ≤ tol:                 vB is ON the plane → skip (will be handled
                                  as vA of the next edge)
If sign(dA) == sign(dB):       same side → no crossing, return
Otherwise (genuine crossing):
  t = dA / (dA − dB)           interpolation parameter in [0,1]
  add Lerp(vA, vB, t)
```

The `pts[last]` trick (instead of always `pts[1]`) handles the edge case of a vertex exactly on the plane, which deposits two points into `pts`.

### 4.3 ContourBuilder — Segment Stitching

**File:** `src/GeoModeler3D.Core/Services/ContourBuilder.cs`

Input: unordered list of `(A, B)` segments from `PlaneMeshIntersector`.  
Output: list of `(List<Vector3> Points, bool IsClosed)` chains.

**Algorithm (greedy O(N²)):**

```
used[i] = false for all segments
tolerance² = (1e-4)²

While there are unused segments:
  1. Pick the first unused segment as the chain seed.
     chain = [seedA, seedB]; mark seed used.

  2. Extend FORWARD from chain.Last:
     Find any unused segment whose A or B endpoint is within tolerance of chain.Last
     Append the far end (whichever of A/B is NOT the matched end)
     Repeat until no match.

  3. Extend BACKWARD from chain.First:
     Same logic, insert at front of chain.

  4. Check closure:
     if (chain.Last − chain.First).Length² < tol²:
       isClosed = true
       Remove the duplicate last point (it equals chain.First)

  5. If chain.Count >= 2: add (chain, isClosed) to result.
```

This O(N²) approach is fine for the contour sizes produced by typical geometry (hundreds of segments). For meshes with millions of triangles a spatial hash would be needed.

### 4.4 ContourExtractionService — Analytic Intersectors

**File:** `src/GeoModeler3D.Core/Services/ContourExtractionService.cs`

Dispatch:
```
entity switch:
  MeshEntity    → PlaneMeshIntersector + ContourBuilder
  SphereEntity  → analytic circle
  CylinderEntity→ parametric sweep
  ConeEntity    → parametric sweep + ConicSectionClassifier
  TorusEntity   → per-ring analytic solve
  _             → []  (unsupported → empty, no error)
```

**NSamples = 64** (`MathConstants.DefaultSegmentCount`) for all parametric methods.

---

#### 4.4.1 Sphere

The intersection of an infinite plane with a sphere is always a circle (or empty).

```
d = dot(sphere.Center − plane.Origin, plane.Normal)   // signed distance from centre to plane
r² = sphere.Radius² − d²

If r² ≤ 0: no intersection (plane misses sphere)

circleRadius   = sqrt(r²)
circleCenter   = sphere.Center − d · plane.Normal     // project centre onto plane

Sample circleRadius points:
  (u, v) = ComputeTangents(plane.Normal)
  for i in 0..NSamples-1:
    φ = 2π·i / NSamples
    P = circleCenter + circleRadius·(cos φ·u + sin φ·v)
```

Result is one closed `ContourCurveEntity`.

---

#### 4.4.2 Cylinder

A finite cylinder: `BaseCenter`, unit `Axis`, `Radius R`, `Height H`.

Every point on the cylinder surface is:
```
P(φ, h) = BaseCenter + h·Axis + R·cos(φ)·u + R·sin(φ)·v
```
where `(u, v)` = `ComputeTangents(Axis)`.

The plane condition `dot(P − planeOrigin, planeNormal) = 0` gives:
```
d0 + h·da + R·cos(φ)·du + R·sin(φ)·dv = 0

where:
  d0 = dot(BaseCenter − planeOrigin, planeNormal)
  da = dot(Axis, planeNormal)
  du = dot(u, planeNormal)
  dv = dot(v, planeNormal)
```

Solving for h:
```
h(φ) = −(d0 + R·cos(φ)·du + R·sin(φ)·dv) / da
```

If `|da| < 1e-5`: plane is nearly parallel to the cylinder axis — degenerate, return empty.

For each φ in `[0, 2π)`:
- Compute h(φ)
- If h ∉ [0, H]: skip (outside finite cylinder bounds)
- Otherwise: emit point `BaseCenter + h·Axis + R·cos(φ)·u + R·sin(φ)·v`

`IsClosed = (emitted point count == NSamples)` — i.e. all 64 samples landed within bounds, meaning the plane cuts the full circumference.

---

#### 4.4.3 Cone

A finite cone: `BaseCenter`, unit `Axis`, base `Radius R`, `Height H`.  
Apex = `BaseCenter + H·Axis`. Radius at height h = `R·(1 − h/H)`.

Every point on the cone surface:
```
P(φ, h) = BaseCenter + h·Axis + R·(1 − h/H)·(cos(φ)·u + sin(φ)·v)
```

Plane condition `dot(P − planeOrigin, planeNormal) = 0`:
```
d0 + h·da + R·(1 − h/H)·(cos(φ)·du + sin(φ)·dv) = 0

Let dPhi = R·(cos(φ)·du + sin(φ)·dv)

d0 + h·da + dPhi − h·dPhi/H = 0
h·(da − dPhi/H) = −(d0 + dPhi)
h(φ) = −(d0 + dPhi) / (da − dPhi/H)
```

If `|da − dPhi/H| < 1e-7`: denominator is zero → skip this φ sample.

For each φ:
- Compute `h(φ)`, skip if h ∉ [0, H]
- `rH = R·(1 − h/H)`
- Emit `BaseCenter + h·Axis + rH·cos(φ)·u + rH·sin(φ)·v`

`IsClosed` same rule as cylinder (all samples within bounds).  
`ConicType` is set by `ConicSectionClassifier.Classify(plane, cone)` (Phase 4).

---

#### 4.4.4 Torus

A torus: `Center`, unit `Normal` N, major radius R, minor radius r.

Every point on the surface:
```
C(φ) = Center + R·(cos(φ)·u + sin(φ)·v)     ← centre of the tube ring at angle φ
P(φ, θ) = C(φ) + r·(cos(θ)·radial + sin(θ)·N)
where radial = cos(φ)·u + sin(φ)·v
```

Plane condition `dot(P − planeOrigin, planeNormal) = 0` for fixed φ becomes:
```
dc + A·cos(θ) + B·sin(θ) = 0

where:
  dc = dot(C(φ) − planeOrigin, planeNormal)
  A  = r · dot(radial, planeNormal)
  B  = r · dot(N, planeNormal)          ← constant for all φ
  amp = sqrt(A² + B²)
```

This is the equation `amp·cos(θ − phiBase) = −dc`, which has:
- **0 solutions** if `|dc| > amp` (ring doesn't intersect the plane)
- **1 solution** if `|dc| = amp` (tangent)
- **2 solutions** if `|dc| < amp`

```
ratio   = clamp(−dc / amp, −1, 1)
phiBase = atan2(B, A)
dTheta  = acos(ratio)

θ₁ = phiBase + dTheta   →  point on curve 1
θ₂ = phiBase − dTheta   →  point on curve 2
```

Iterating over 128 φ values builds two candidate curves. The second curve is only added if it is **distinct** from the first (average point distance > 0.01). For a symmetric cut, both curves are identical and only one contour is returned.

---

### 4.5 ContourUpdateService — Debounce Pattern

**File:** `src/GeoModeler3D.App/Services/ContourUpdateService.cs`

```
_debounceTimer: DispatcherTimer (200 ms)
_planeContours: Dictionary<Guid, List<Guid>>   planeId → [contourId, ...]
_recomputing:   bool                           guard against re-entrant events
```

**Event handlers:**

```
OnEntityAdded(entity):
  if _recomputing or entity is ContourCurveEntity: return
  Schedule()

OnEntityChanged(entity):
  if _recomputing or entity is ContourCurveEntity: return
  Schedule()

OnEntityRemoved(id):
  if _recomputing: return
  if id is a known plane (in _planeContours):
    _recomputing = true
    Remove all its contour IDs from SceneManager
    _recomputing = false
    _planeContours.Remove(id)
    Schedule()    ← re-run remaining planes
    return
  if id is one of our own generated contours: return   ← ignore
  Schedule()
```

**`Schedule()`** — stops and restarts the 200 ms timer. Rapid successive changes collapse into one recompute.

**`RecomputeAll()`** — runs on the UI thread (timer fires on the dispatcher):

```
_recomputing = true
  Remove all currently tracked contour IDs from SceneManager
  _planeContours.Clear()

  For each CuttingPlaneEntity in scene:
    For each targetId in plane.TargetEntityIds:
      entity = SceneManager.GetById(targetId)
      if entity is null or ContourCurveEntity: skip
      contours = ContourExtractionService.Extract(plane, entity)
      For each contour:
        SceneManager.Add(contour)     ← fires EntityAdded, but _recomputing=true guards it
        record contour.Id under _planeContours[plane.Id]
_recomputing = false
```

The `_recomputing` flag is **essential**: adding contour entities to the scene fires `EntityAdded`, which would ordinarily trigger another `Schedule()`. The flag suppresses that.

---

## 5. Phase 4 — Conic Section Classification (FR-39)

### 5.1 Background

When a plane cuts a cone, the cross-section is one of four conic sections depending on the angle between the plane and the cone's axis.

```
α = cone half-angle = atan2(BaseRadius, Height)
β = angle between the CUTTING PLANE itself and the cone axis
  = π/2 − (angle between planeNormal and coneAxis)
  → sin β = |dot(planeNormal, coneAxis)|
```

| Condition | Section |
|---|---|
| β = 90° (plane ⊥ axis, sin β = 1) | **Circle** |
| β > α | **Ellipse** |
| β = α | **Parabola** |
| β < α | **Hyperbola** |

### 5.2 Implementation

**File:** `src/GeoModeler3D.Core/Services/ConicSectionClassifier.cs`

```
s      = |dot(planeNormal, coneAxis)|          // sin β, clamped to [0, 1]
slant  = sqrt(BaseRadius² + Height²)           // slant height of cone
sinAlpha = BaseRadius / slant                  // sin α = sin(half-angle)
tol    = 1e-3 (applied to sine values)

If s ≥ 1 − tol:          Circle      (plane nearly perpendicular to axis)
If s − sinAlpha ≥ tol:   Ellipse     (β > α)
If |s − sinAlpha| < tol: Parabola    (β ≈ α, within tolerance)
Otherwise:               Hyperbola   (β < α)
```

**Why use sines instead of angles?** Avoids calling `acos`/`asin` — working directly with dot products is numerically cheaper and avoids domain errors.

**Integration in `ContourExtractionService.ExtractFromCone`:**

After the parametric sampling (Section 4.4.3) produces a set of points:
```csharp
var conicType = ConicSectionClassifier.Classify(plane, cone);
return [new ContourCurveEntity(pts, planeId, cone.Id)
{
    IsClosed = pts.Count == NSamples,
    ConicType = conicType          // nullable ConicSectionType? set here
}];
```

`ConicType` is readable in the UI (properties panel) and stored in the JSON file as a string (`"Circle"`, `"Ellipse"`, `"Parabola"`, `"Hyperbola"`, or `null` for non-cone sources).

### 5.3 Visualising Different Conic Types

The current Phase 4 implementation uses the **same** parametric sampling algorithm for all conic types. The `ConicType` property exists on `ContourCurveEntity` to inform the UI and future algorithms.

For more precise sampling per conic type, the approach would be:
- **Circle/Ellipse**: use the exact ellipse parametric equation after computing the semi-axes.
- **Parabola**: clamp h to the finite cone height and sample uniformly in h.
- **Hyperbola**: sample only the finite branch that intersects the cone (h ∈ [0, H]).

This refinement would live in a `ConeConicSampler` class and replace the current sweep in `ExtractFromCone`.

---

### 5.4 Ear Clipping Triangulator

**File:** `src/GeoModeler3D.Core/Math/EarClippingTriangulator.cs`  
**Used by:** `CappingVisualGenerator` (Phase 5) to fill cross-section polygons.

#### What is an ear?

A vertex `curr` of a polygon is an **ear** when:

1. The triangle `(prev, curr, next)` — formed with its two neighbours — is **convex** (does not fold inward at `curr`).
2. No other polygon vertex lies **inside** that triangle.

Removing an ear clips one triangle off the polygon and leaves a smaller valid polygon. Repeating this process until three vertices remain gives a complete triangulation.

#### Winding order

All geometry in this codebase uses **counter-clockwise (CCW)** vertex order when viewed from the front. The ear-clipping algorithm relies on this: a convex vertex in a CCW polygon has a **positive** 2-D cross product.

The 2-D cross product of two edge vectors is the z-component of their 3-D cross product:

```
Cross2D(u, v) = u.x · v.y − u.y · v.x

For edge (a→b) then (b→c):
  Cross2D(b − a, c − b) > 0  →  left turn  →  convex vertex (CCW polygon)
  Cross2D(b − a, c − b) ≤ 0  →  right turn →  reflex vertex (skip as ear candidate)
```

#### Step 0 — Detect and fix winding

Before processing, the signed area of the polygon is computed using the **Shoelace formula**:

```
SignedArea = ½ · Σ (xᵢ · yᵢ₊₁ − xᵢ₊₁ · yᵢ)   for i = 0 … n−1 (wrapping)

Positive → CCW  (standard, no action needed)
Negative → CW   (reverse the working index list to normalise to CCW)
```

The original `polygon` array is never modified; only the working `indices` list is reversed when necessary.

#### Step 1 — Point-in-triangle test

Before declaring a vertex an ear, the algorithm checks that no other polygon vertex is inside the candidate triangle `(a, b, c)`.

The test uses **signed areas of sub-triangles** (equivalent to barycentric coordinates):

```
d1 = Cross2D(p − a, b − a)
d2 = Cross2D(p − b, c − b)
d3 = Cross2D(p − c, a − c)

If all three have the same sign (all ≥ 0 or all ≤ 0):
  p is INSIDE or ON THE BOUNDARY of triangle (a, b, c)
Otherwise:
  p is OUTSIDE
```

Boundary points (`d = 0`) are treated as inside. This conservatively rejects ear candidates where another vertex lies exactly on an ear edge, preventing zero-area degenerate triangles.

#### Step 2 — Main loop

```
indices = [0, 1, 2, …, n−1]   (reversed if CW)
result  = []

While indices.Count > 3  AND  iterations < n²:
  For i = 0 … indices.Count − 1:
    prev = indices[(i − 1 + count) mod count]
    curr = indices[i]
    next = indices[(i + 1) mod count]

    If Cross2D(poly[curr]−poly[prev], poly[next]−poly[curr]) ≤ 0:
      continue                          ← reflex vertex, not an ear

    If any other index j has PointInTriangle(poly[j], poly[prev], poly[curr], poly[next]):
      continue                          ← another vertex blocks this ear

    ✓ Ear found:
      result.Append(prev, curr, next)   ← one triangle
      indices.Remove(i)                 ← clip the ear
      break                             ← restart search from beginning

If no ear was found in a full pass: break  ← degenerate polygon, stop

Add final triangle: result.Append(indices[0], indices[1], indices[2])
```

The `n²` iteration cap prevents an infinite loop on degenerate input (e.g. all vertices collinear).

#### Complexity

| Case | Time |
|---|---|
| Convex polygon | O(n) — every vertex is immediately an ear |
| Simple concave polygon | O(n²) — worst case: each pass scans all remaining vertices |
| Contour curves (n = 64) | Imperceptible; finishes in < 1 ms |

For the polygon sizes produced by contour extraction (64 – 256 points), O(n²) is entirely acceptable. A spatial acceleration structure (e.g. interval tree for point-in-triangle queries) would only be warranted for polygons with thousands of vertices.

#### Relationship to `PlaneProjector`

`EarClippingTriangulator` works in **2-D** (`Vector2`). Before calling it, `CappingVisualGenerator` uses `PlaneProjector.Project` to flatten the 3-D contour points onto the cutting plane's local `(u, v)` frame:

```
3-D contour points
  ──PlaneProjector.Project──▶  2-D (u, v) polygon
      ──EarClippingTriangulator.Triangulate──▶  triangle index list
          ──CappingVisualGenerator──▶  MeshGeometry3D (using original 3-D positions)
```

The triangle indices produced by the triangulator index directly into the original `contour.Points` list, so the `MeshGeometry3D` is built from the exact 3-D positions without a round-trip lift (avoiding floating-point drift).

#### Edge cases handled

| Input | Behaviour |
|---|---|
| Fewer than 3 points | Returns empty list immediately |
| Exactly 3 points | Returns `[0, 1, 2]` without entering the main loop |
| CW winding | Index list is reversed before processing; output indices still reference original polygon |
| Vertex exactly on an ear edge | Treated as inside the triangle → ear rejected → no zero-area triangle emitted |
| All vertices collinear | No ear is ever found → loop exits via the `n²` cap → empty result |

---

## 5.5 Interactive Manipulation — Cutting Plane Gizmo (FR-41)

**Files:**
- `src/GeoModeler3D.Rendering/CuttingPlaneManipulator.cs`
- `src/GeoModeler3D.App/Services/ManipulatorService.cs`

### Overview

When a `CuttingPlaneEntity` is selected, a HelixToolkit `CombinedManipulator` gizmo appears at the plane's `Origin`. The user can:
- **Translate** the plane along any world axis using the coloured arrow handles.
- **Rotate** the plane around any world axis using the ring handles.

The entity updates **live** during the drag (so clipping and contour curves track in real time), and a single undo entry is recorded for the whole gesture when the mouse is released.

### Component Responsibilities

| Component | Layer | Responsibility |
|---|---|---|
| `CombinedManipulator` | HelixToolkit | Renders arrow + ring handles; writes to a shared `MatrixTransform3D` while the user drags |
| `CuttingPlaneManipulator` | Rendering | Owns the `MatrixTransform3D`; applies accumulated transform to entity live; fires `DragCompleted` on mouse-up |
| `ManipulatorService` | App | Wires selection → attach/detach; converts `DragCompleted` into a single `MacroCommand` on the undo stack |

### How HelixToolkit CombinedManipulator Is Used

`CombinedManipulator` does not fire drag-start/drag-end events. Instead it mutates the `Matrix` of whatever `Transform3D` instance is assigned to its `TargetTransform` dependency property during user interaction.

```csharp
// CuttingPlaneManipulator.AttachTo
var _transform = new MatrixTransform3D();          // initially Identity
_transform.Changed += OnTransformChanged;          // our callback

_manipulator = new CombinedManipulator
{
    Position        = plane.Origin.ToPoint3D(),    // gizmo centre in world space
    TargetTransform = _transform,                  // HelixToolkit writes here during drag
    CanTranslateX = true, CanTranslateY = true, CanTranslateZ = true,
    CanRotateX    = true, CanRotateY    = true, CanRotateZ    = true,
    Diameter = Math.Max(plane.DisplayWidth, plane.DisplayHeight) * 0.3
};
viewport.Children.Add(_manipulator);
viewport.PreviewMouseLeftButtonUp += OnViewportMouseUp;
```

The gizmo is added directly to `viewport.Children` — **not** inside a `CuttingPlaneGroup` — so it is never clipped or hidden by the cutting operation.

### Live-Update Algorithm

```
OnTransformChanged(sender, e):
  if _suppressChanges: return        ← guard for programmatic resets
  if matrix.IsIdentity: return       ← skip the initial assignment notification

  if NOT _isDragging:
    _preDragOrigin ← plane.Origin    ← snapshot before first frame of drag
    _preDragNormal ← plane.Normal
    _isDragging ← true

  numericsMatrix ← ToNumerics(_transform.Matrix)

  // Apply ACCUMULATED transform to pre-drag snapshot (not incremental per-frame)
  plane.Origin ← Vector3.Transform(_preDragOrigin, numericsMatrix)
  plane.Normal ← Normalize(Vector3.TransformNormal(_preDragNormal, numericsMatrix))
```

**Why apply to the snapshot rather than applying delta per frame?**
Incremental per-frame deltas accumulate floating-point error and create drift. By always computing `preDragState + totalAccumulatedTransform` the result is as accurate as the input, regardless of how many frames the drag lasts.

**Effect of live updates on downstream systems:**
Each assignment to `plane.Origin` / `plane.Normal` fires `PropertyChanged` → `SceneManager.EntityChanged` → `RenderingService.UpdateEntity` (rebuilds the quad visual + syncs clip groups) and `ContourUpdateService.OnEntityChanged` (restarts the 200 ms debounce timer). Contour curves therefore lag by at most 200 ms; clipping updates every frame.

### Matrix Conversion: `Matrix3D` → `Matrix4x4`

WPF uses `System.Windows.Media.Media3D.Matrix3D`; the Core layer uses `System.Numerics.Matrix4x4`. Both are **row-major**, with the translation in the last row (OffsetX/Y/Z in WPF, M41/M42/M43 in Numerics):

```csharp
private static Matrix4x4 ToNumerics(Matrix3D m) =>
    new(
        (float)m.M11,     (float)m.M12,     (float)m.M13,     (float)m.M14,
        (float)m.M21,     (float)m.M22,     (float)m.M23,     (float)m.M24,
        (float)m.M31,     (float)m.M32,     (float)m.M33,     (float)m.M34,
        (float)m.OffsetX, (float)m.OffsetY, (float)m.OffsetZ, (float)m.M44);
```

`Vector3.Transform(point, matrix)` applies all four rows (includes translation).  
`Vector3.TransformNormal(dir, matrix)` applies only the upper 3×3 (ignores translation — correct for direction vectors such as `Normal`).

### Drag-End and the Reset Pattern

When the mouse button is released, the viewport fires `PreviewMouseLeftButtonUp`:

```
OnViewportMouseUp:
  if NOT _isDragging: return        ← regular click, not a manipulator drag

  _isDragging ← false
  capture (completedPlane, preDragOrigin, preDragNormal)

  _suppressChanges ← true
  _transform.Matrix ← Matrix3D.Identity   ← reset; suppressed so we don't re-apply
  _suppressChanges ← false

  _manipulator.Position ← completedPlane.Origin   ← reposition handles to new location

  DragCompleted.Invoke(completedPlane, preDragOrigin, preDragNormal)
```

The **`_suppressChanges` flag** is essential: resetting `_transform.Matrix` fires `Changed`, which would otherwise re-apply the now-identity transform to the entity (snapping it back to the pre-drag position).

After the reset, `Position` is updated to the entity's new `Origin` so that the next drag starts from the correct world-space location.

### Undo/Redo — Single Macro Command per Drag

`CuttingPlaneManipulator.DragCompleted` provides the entity reference and the **pre-drag** `Origin` and `Normal`. The entity is already at its post-drag state. `ManipulatorService.OnDragCompleted` builds:

```csharp
var cmdOrigin = new ChangePropertyCommand<Vector3>(
    plane, "Origin", preDragOrigin, plane.Origin);   // Execute: set to post; Undo: set to pre

var cmdNormal = new ChangePropertyCommand<Vector3>(
    plane, "Normal", preDragNormal, plane.Normal);

_undoManager.Execute(
    new MacroCommand("Move Cutting Plane", [cmdOrigin, cmdNormal]));
```

`UndoManager.Execute` calls `MacroCommand.Execute()` first, which sets `Origin` and `Normal` to their already-current values (a benign re-set that triggers one extra `EntityChanged` event but produces no visible change), then pushes the command onto the undo stack.

**Undo** (`Ctrl+Z`) calls `MacroCommand.Undo()` in reverse order:
1. `cmdNormal.Undo()` → restores pre-drag `Normal`
2. `cmdOrigin.Undo()` → restores pre-drag `Origin`

**Redo** (`Ctrl+Y`) calls `MacroCommand.Execute()` again, restoring the post-drag state.

A **no-move guard** in `OnDragCompleted` skips the push entirely when `postDragOrigin == preDragOrigin && postDragNormal == preDragNormal` (e.g. the user clicked a handle but didn't move the mouse).

### ManipulatorService — Attach / Detach Logic

```
SelectionChanged:
  if selectedEntity is CuttingPlaneEntity → AttachTo(plane)
  else                                    → Detach()

EntityRemoved(id):
  if id == ActivePlaneId                  → Detach()

EntityChanged(entity):
  if entity is CuttingPlaneEntity AND entity.Id == ActivePlaneId AND NOT _isDragging:
    UpdatePosition()   ← keeps gizmo aligned after undo, properties-panel edits, etc.
```

`UpdatePosition()` only updates `_manipulator.Position`; it does not reset the accumulated transform or fire any events.

### Interaction Diagram

```
User grabs translate handle
  │
  ▼
CombinedManipulator writes to _transform.Matrix
  │
  ▼
MatrixTransform3D.Changed fires (every mouse-move frame)
  │
  ▼
CuttingPlaneManipulator.OnTransformChanged
  ├─ (first frame) snapshot preDragOrigin, preDragNormal
  └─ plane.Origin ← Transform(preDragOrigin, accumulatedMatrix)
     plane.Normal ← TransformNormal(preDragNormal, accumulatedMatrix)
          │
          ▼
     SceneManager.EntityChanged(plane)
       ├─ RenderingService: rebuild quad + sync clip groups  [every frame]
       └─ ContourUpdateService: restart 200 ms timer        [debounced]

User releases mouse button
  │
  ▼
viewport.PreviewMouseLeftButtonUp
  │
  ▼
CuttingPlaneManipulator.OnViewportMouseUp
  ├─ reset _transform to Identity (suppressed)
  ├─ reposition _manipulator.Position
  └─ DragCompleted.Invoke(plane, preDragOrigin, preDragNormal)
          │
          ▼
     ManipulatorService.OnDragCompleted
       └─ UndoManager.Execute(MacroCommand[cmdOrigin, cmdNormal])
```

---

## 6. Data Flow Summary

```
┌─────────────────────────────────────────────────────────┐
│ User creates CuttingPlane via dialog                    │
│   → CreateEntityCommand(SceneManager, CuttingPlaneEntity)│
│   → SceneManager.Add(plane)                             │
│     → RenderingService: AddEntity(plane)                │
│         → CuttingPlaneEntityRenderer creates Visual3D   │
│         → CuttingPlaneVisualizer.Sync(plane)            │
│             (ClipSide=None → no groups created)         │
│     → ContourUpdateService.OnEntityAdded: Schedule()    │
└─────────────────────────────────────────────────────────┘
                        ▼ 200 ms later
┌─────────────────────────────────────────────────────────┐
│ ContourUpdateService.RecomputeAll()                     │
│   For each target of each cutting plane:                │
│     ContourExtractionService.Extract(plane, entity)     │
│       → analytic / mesh algorithm                       │
│       → returns ContourCurveEntity[]                    │
│     SceneManager.Add(contour)                           │
│       → RenderingService: AddEntity(contour)            │
│           → ContourCurveEntityRenderer: LinesVisual3D   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ User sets ClipSide = Positive on the cutting plane      │
│   → ChangePropertyCommand sets plane.ClipSide           │
│   → SceneManager.EntityChanged(plane)                   │
│     → RenderingService: UpdateEntity(plane)             │
│         → CuttingPlaneVisualizer.Sync(plane)            │
│             TeardownState: move visuals back to viewport │
│             BuildState:                                 │
│               Create CuttingPlaneGroup with +Normal     │
│               Move target Visual3D into group           │
│               Add group to viewport                     │
│     → ContourUpdateService.OnEntityChanged: Schedule()  │
│         (contours recomputed 200 ms later)              │
└─────────────────────────────────────────────────────────┘
```

---

## 7. How to Extend

### Add a new clippable entity type

Nothing to do — any entity whose renderer produces a `MeshGeometry3D`-backed `ModelVisual3D` is automatically clipped by `CuttingPlaneGroup` once it is added to the group's `Children`. The `CuttingPlaneVisualizer` moves all target visuals into the group.

### Add a new analytic intersector (new entity type)

1. Add the entity type's class to `ContourExtractionService`'s `entity switch`.
2. Write a `private static IReadOnlyList<ContourCurveEntity> ExtractFromMyShape(Plane3D, MyEntity, Guid)` method.
3. Follow the pattern: compute `NSamples` sample points, check bounds per sample, return `[]` when fewer than 2 points, set `IsClosed`.
4. Add tests in `ContourExtractionServiceTests`.

### Change the debounce delay

In `ContourUpdateService` constructor:
```csharp
_debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
```
Raise the value for large/complex scenes; lower it for a more responsive UI.

### Support multiple cutting planes clipping the same entity

The current single-plane-per-entity limitation is because `Visual3D` can only have one WPF parent. To support multi-plane clipping you would need to:
1. Create a fresh `CuttingPlaneGroup` per plane–entity pair and put a **renderer-generated copy** of the visual inside each group (rather than moving the original).
2. Keep the original visual hidden (or removed from the viewport).
3. Track all copies and rebuild them when the entity changes.

This is a significant architecture change. The current design explicitly documents this as a known limitation.

### Suppress contour curves for a specific entity type

In `ContourExtractionService.Extract`, the `_` default arm already returns `[]`. If you want to prevent a normally-supported type from producing contours (e.g. very large meshes), add an early return:

```csharp
if (entity is MeshEntity m && m.TriangleCount > 100_000)
    return [];
```

### Make ConicType visible in the UI

`ContourCurveEntity.ConicType` is a `ConicSectionType?` (nullable). It is already emitted by the serialisation visitor and read back by the deserialiser. To show it in the Properties Panel, add a read-only row to the `ContourCurveEntity` DataTemplate in `PropertiesPanel.xaml`:

```xml
<TextBlock Text="Conic Type" Style="{StaticResource PropLabel}"/>
<TextBlock Text="{Binding ConicType}" Style="{StaticResource PropValue}"/>
```
