# GeoModeler3D -- Demo Walkthrough

This guide walks you through every feature available in the GeoModeler3D demo application.

---

## 1. Building and Launching

**Prerequisites:** .NET 10 SDK, Windows 10/11

```
cd GeoModeler3D
dotnet build
dotnet run --project src/GeoModeler3D.App
```

The application opens maximized with three panels:

```
+------------------+-----------------------------+------------------+
| Entities (list)  |     3D Viewport             | Properties       |
|                  |     (grid, axes, viewcube)  |                  |
|                  |                             |                  |
+------------------+-----------------------------+------------------+
| Status: Ready                          | Entities: 0             |
+--------------------------------------------------------------- --+
```

---

## 2. Creating Entities

Use the **Create** menu to add 3D primitives. Each opens a dialog where you specify parameters.

### 2.1 Create a Sphere

1. Click **Create > Sphere...**
2. Enter Center (X=0, Y=0, Z=0) and Radius (1)
3. Click **OK**

A blue sphere appears at the origin. The entity list shows "Sphere" with a color swatch, and the status bar updates to "Entities: 1".

### 2.2 Create a Cylinder

1. Click **Create > Cylinder...**
2. Enter Base Center (X=3, Y=0, Z=0), Axis (X=0, Y=0, Z=1), Radius (0.5), Height (3)
3. Click **OK**

An orange cylinder appears next to the sphere, standing upright along the Z axis.

### 2.3 Create a Cone

1. Click **Create > Cone...**
2. Enter Base Center (X=-3, Y=0, Z=0), Axis (X=0, Y=0, Z=1), Base Radius (1), Height (2)
3. Click **OK**

A red cone appears on the opposite side.

### 2.4 Create a Torus

1. Click **Create > Torus...**
2. Enter Center (X=0, Y=3, Z=0), Normal (X=0, Y=0, Z=1), Major Radius (2), Minor Radius (0.4)
3. Click **OK**

A magenta torus appears in front of the sphere.

### 2.5 Create a Point

1. Click **Create > Point...**
2. Enter Position (X=0, Y=0, Z=3)
3. Click **OK**

A small white sphere marker appears above the origin.

### 2.6 Create a Triangle from 3 Existing Points

A triangle requires exactly three `PointEntity` objects to already exist in the scene. The workflow uses multi-selection to identify the three vertices.

1. Create three points at distinct, non-collinear positions. For example:
   - Point A: (1, 0, 0)
   - Point B: (0, 1, 0)
   - Point C: (0, 0, 1)
2. In the **Entities** list, click **Point A** to select it
3. **Ctrl+click** **Point B** to add it to the selection (both are now highlighted)
4. **Ctrl+click** **Point C** to add it (all three selected)
5. Click **Create > Triangle from 3 Points...**

A confirmation dialog appears showing the name and coordinates of each selected point:

```
Creating triangle from the 3 selected points:
Point 1    Point A   (1.000, 0.000, 0.000)
Point 2    Point B   (0.000, 1.000, 0.000)
Point 3    Point C   (0.000, 0.000, 1.000)
[Create]  [Cancel]
```

6. Click **Create**

The triangle appears in the viewport. The entity list now shows "Triangle" with a cyan color swatch.

#### Validation Errors

| Situation | Error shown |
|---|---|
| Fewer or more than 3 entities selected | "Please select exactly 3 point entities…" (before dialog opens) |
| Selection contains non-Point entities | "All 3 selected entities must be Point entities…" (before dialog opens) |
| Two or more points share the same position | "Two or more selected points occupy the same position…" (inside dialog on Create) |
| Three points are collinear | "The three selected points are collinear…" (inside dialog on Create) |

---

## 3. Navigating the 3D Viewport

The viewport uses HelixToolkit's built-in camera controls:

| Action | Mouse/Key |
|---|---|
| **Rotate** | Left-click + drag |
| **Pan** | Middle-click + drag (or Shift + left-click + drag) |
| **Zoom** | Scroll wheel |
| **View Cube** | Click faces/edges/corners of the cube in the top-right |

### Camera Presets (View menu)

| Menu Item | Camera Position | What You See |
|---|---|---|
| **Top View** | Looking straight down the Z axis | XY plane from above |
| **Front View** | Looking along the Y axis | XZ plane head-on |
| **Right View** | Looking along the X axis | YZ plane from the side |
| **Isometric View** | Position (10,10,10) looking toward origin | Classic 3D perspective |

### Viewport Controls

| Menu Item | Effect |
|---|---|
| **Zoom to Fit** | Adjusts camera to show all entities |
| **Toggle Grid** | Shows/hides the ground grid |
| **Toggle Axes** | Shows/hides the coordinate system arrows |

---

## 4. Selecting Entities

### 4.1 Single Selection — Click in the Viewport

- **Left-click** on an entity in the 3D viewport to select it
- The selected entity is highlighted in yellow
- **Shift+click** to toggle-select (add/remove from selection)
- **Click empty space** to deselect all

### 4.2 Single or Multi-Selection — Entity List

- **Click** an entity name in the left sidebar to single-select it (clears any previous selection)
- **Ctrl+click** to add an entity to the current selection, or remove it if already selected
- **Shift+click** to range-select all entities between the last-clicked and the current item

When one or more entities are selected:
- The **Properties panel** on the right shows the properties of the first selected entity
- The selected 3D objects are highlighted yellow in the viewport

---

## 5. Editing Entity Properties

After creating an entity, you can edit its geometric properties directly in the **Properties panel** without deleting and recreating it.

### 5.1 Editing Numeric Properties

1. Select an entity (click it in the viewport or entity list)
2. In the **Properties panel**, click into any numeric field (e.g., Radius, Center X, Height)
3. Type the new value
4. Press **Enter** or click elsewhere to commit

The 3D viewport updates immediately. The edit is recorded on the undo stack — press **Ctrl+Z** to revert.

**Example — move a sphere's center:**
1. Select the sphere
2. In Properties, change Center X from `0.000` to `3.000` and press Enter
3. The sphere moves to the new position in the viewport

**Example — resize a cylinder:**
1. Select the cylinder
2. Change Radius from `0.500` to `1.500` and press Enter
3. The cylinder widens in real time

### 5.2 Editing Vector3 Components (Center, Axis, Position, Vertices)

Vector3 properties are split into three separate fields labeled X, Y, Z. Edit each component independently; the full vector is reconstructed and the entity updates after each commit.

### 5.3 Editing the Name

1. Select any entity
2. In the Properties panel, click the **Name** field
3. Type the new name and press Enter
4. The entity list sidebar updates immediately

### 5.4 Editing the Color

1. Select any entity
2. In the Properties panel, click the hex color field (e.g., `#FF00FFFF`)
3. Type a new ARGB hex value (e.g., `#FFFF0000` for red) and press Enter
4. The entity color updates in the viewport and in the entity list color swatch

### 5.5 Toggle Visibility

1. Select an entity
2. In the Properties panel, uncheck the **Visible** checkbox
3. The entity disappears from the viewport but remains in the entity list
4. Re-check to make it reappear

### 5.6 Read-Only Computed Properties

Some properties are computed from geometry and are displayed but cannot be edited directly:

| Entity | Read-only fields |
|---|---|
| Triangle | Normal (cross-product of edges), Area |
| Cone | HalfAngle (derived from BaseRadius and Height) |

### 5.7 Delete Entities

1. Select one or more entities
2. Press the **Delete** key, or use **Edit > Delete**
3. The entities are removed from the scene

### 5.8 Undo and Redo

Every create, delete, and property-edit operation is undoable:

| Action | Shortcut |
|---|---|
| **Undo** | Ctrl+Z (or Edit > Undo) |
| **Redo** | Ctrl+Y (or Edit > Redo) |

The undo stack holds up to 50 operations. Try this sequence:
1. Create a Sphere
2. Edit its Radius to 2 in the Properties panel, press Enter
3. Press Ctrl+Z -- radius reverts to 1
4. Create a Cylinder -- press Ctrl+Z -- Cylinder disappears
5. Press Ctrl+Z -- Sphere disappears
6. Press Ctrl+Y -- Sphere reappears
7. Press Ctrl+Y -- Cylinder reappears

---

## 6. File Operations

| Menu Item | Shortcut | Effect |
|---|---|---|
| **File > New** | Ctrl+N | Clears all entities, selection, and undo history |
| **File > Open...** | Ctrl+O | Opens a saved `.geo3d` or `.json` project file |
| **File > Save** | Ctrl+S | Saves the current scene (prompts for path if unsaved) |
| **File > Save As...** | Ctrl+Shift+S | Saves the scene to a new file path |
| **File > Exit** | -- | Closes the application |

---

## 7. Keyboard Shortcuts Summary

| Shortcut | Action |
|---|---|
| Ctrl+N | New scene |
| Ctrl+O | Open scene |
| Ctrl+S | Save scene |
| Ctrl+Shift+S | Save scene as |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Delete | Delete selected entities |

---

## 8. Status Bar

The bottom status bar shows:
- **Status text** -- feedback on the last action performed (e.g., "Created Sphere", "Created Triangle", "Undo: Change Radius of Sphere")
- **Entity count** -- total number of entities in the scene

---

## 9. About Dialog

Click **Help > About** to see the application name, version, and technology stack.

---

## 10. Logging

The application writes diagnostic logs to:

```
logs/geomodeler3d-YYYYMMDD.log
```

This captures startup/shutdown events and can be extended for debugging.

---

## 11. Suggested Demo Script

For a live demo, follow this sequence to show all major features in about 3 minutes:

1. **Launch** the app -- point out the three-panel layout
2. **Create** a Sphere (defaults) -- note entity list and status bar update
3. **Create** a Cylinder at (3,0,0) -- show two entities in list
4. **Create** a Torus at (0,3,0) -- scene is getting interesting
5. **Rotate** the viewport -- show 3D navigation
6. **Click** the Sphere in the viewport -- show selection highlight and Properties panel
7. **Edit** the Sphere's Radius from 1 to 2 in Properties panel, press Enter -- sphere resizes live
8. **Ctrl+Z** -- radius reverts to 1
9. **Edit** Center X to 2 -- sphere moves; **Ctrl+Z** again to revert
10. **Shift+click** the Cylinder in the viewport -- show multi-select (both highlighted)
11. **Click** empty space -- deselect
12. **Select** the Torus from the entity list -- show list-to-viewport selection sync
13. **Uncheck Visible** in Properties -- Torus disappears; **re-check** -- reappears
14. **Delete** the Cylinder (select + Delete key); **Ctrl+Z** -- Cylinder reappears
15. **View > Top View** -- show camera preset; **View > Isometric View** -- back to 3D
16. **View > Toggle Grid** -- grid disappears and reappears; **View > Zoom to Fit** -- camera adjusts
17. **File > Save As...** -- save the scene; **File > New** -- clean slate; **File > Open...** -- restore it
18. **File > New** -- start fresh for the triangle demo
19. **Create** three points: (1,0,0), (0,1,0), (0,0,1)
20. In the entity list, **click** Point 1 then **Ctrl+click** Point 2 and Point 3 -- all three selected
21. **Create > Triangle from 3 Points...** -- show confirmation dialog with point coordinates
22. Click **Create** -- triangle appears in viewport
23. **Select** the triangle -- show V0/V1/V2 components and computed Normal/Area in Properties panel
24. **Edit** V0 X from 1.000 to 2.000 and press Enter -- triangle reshapes live
25. **Ctrl+Z** -- vertex reverts
26. **Help > About** -- show version info
