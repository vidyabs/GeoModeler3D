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

There are two ways to select entities:

### 4.1 Click in the Viewport

- **Left-click** on an entity in the 3D viewport to select it
- The selected entity is highlighted in yellow
- **Shift+click** to toggle-select (add/remove from selection)
- **Click empty space** to deselect all

### 4.2 Click in the Entity List

- Click an entity name in the left sidebar to select it
- The viewport highlights the corresponding 3D object

When an entity is selected:
- The **Properties panel** on the right shows: Name, ID, Visible checkbox, Color swatch, Layer
- The **Entity List** highlights the selected row
- The 3D object is highlighted in the viewport

---

## 5. Editing Entities

### 5.1 Toggle Visibility

1. Select an entity
2. In the Properties panel, uncheck the **Visible** checkbox
3. The entity disappears from the viewport but remains in the entity list

### 5.2 Delete Entities

1. Select one or more entities
2. Press **Delete** key, or use **Edit > Delete**
3. The entities are removed from the scene

### 5.3 Undo and Redo

Every create and delete operation is undoable:

| Action | Shortcut |
|---|---|
| **Undo** | Ctrl+Z (or Edit > Undo) |
| **Redo** | Ctrl+Y (or Edit > Redo) |

The undo stack holds up to 50 operations. Try this sequence:
1. Create a Sphere
2. Create a Cylinder
3. Press Ctrl+Z -- the Cylinder disappears
4. Press Ctrl+Z -- the Sphere disappears
5. Press Ctrl+Y -- the Sphere reappears
6. Press Ctrl+Y -- the Cylinder reappears

---

## 6. File Operations

| Menu Item | Shortcut | Effect |
|---|---|---|
| **File > New** | Ctrl+N | Clears all entities, selection, and undo history |
| **File > Exit** | -- | Closes the application |

---

## 7. Keyboard Shortcuts Summary

| Shortcut | Action |
|---|---|
| Ctrl+N | New scene |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Delete | Delete selected entities |

---

## 8. Status Bar

The bottom status bar shows:
- **Status text** -- feedback on the last action performed (e.g., "Created Sphere", "Undo: Create Sphere")
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

For a live demo, follow this sequence to show all major features in about 2 minutes:

1. **Launch** the app -- point out the three-panel layout
2. **Create** a Sphere (defaults) -- note entity list and status bar update
3. **Create** a Cylinder at (3,0,0) -- show two entities in list
4. **Create** a Torus at (0,3,0) -- scene is getting interesting
5. **Rotate** the viewport -- show 3D navigation
6. **Click** the Sphere in the viewport -- show selection highlight and properties panel
7. **Shift+click** the Cylinder -- show multi-select
8. **Click** empty space -- deselect
9. **Select** the Torus from the entity list -- show list-to-viewport selection sync
10. **Uncheck Visible** in properties -- Torus disappears
11. **Check Visible** again -- Torus reappears
12. **Delete** the Cylinder (select + Delete key)
13. **Ctrl+Z** -- Cylinder reappears (undo)
14. **Ctrl+Z** twice -- Torus and Cylinder gone (undo creation)
15. **Ctrl+Y** twice -- they come back (redo)
16. **View > Top View** -- show camera preset
17. **View > Isometric View** -- back to 3D
18. **View > Toggle Grid** -- grid disappears and reappears
19. **View > Zoom to Fit** -- camera adjusts
20. **File > New** -- clean slate
21. **Help > About** -- show version info
