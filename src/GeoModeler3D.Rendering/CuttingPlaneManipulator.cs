using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

/// <summary>
/// Attaches a <see cref="CombinedManipulator"/> gizmo to a <see cref="CuttingPlaneEntity"/>
/// so the user can drag it to translate or rotate the plane in the viewport.
///
/// <para>
/// Interaction model:
/// <list type="bullet">
///   <item>While dragging, the plane's <c>Origin</c> and <c>Normal</c> are updated live
///     by applying the accumulated <see cref="MatrixTransform3D"/> to the
///     pre-drag snapshot.  ContourUpdateService's 200 ms debounce keeps the
///     contour curves in sync without stuttering.</item>
///   <item>On mouse-up, <see cref="DragCompleted"/> is fired with the entity and the
///     pre-drag Origin/Normal so that the caller can push a single
///     undoable command representing the whole drag gesture.</item>
/// </list>
/// </para>
/// </summary>
public class CuttingPlaneManipulator
{
    private readonly ViewportManager _viewportManager;

    private CombinedManipulator? _manipulator;

    // The MatrixTransform3D that HelixToolkit writes to during drag.
    // We subscribe to its Changed event instead of polling.
    private readonly MatrixTransform3D _transform = new();

    private CuttingPlaneEntity? _activePlane;
    private Vector3 _preDragOrigin;
    private Vector3 _preDragNormal;
    private bool _isDragging;

    // Suppresses our own Changed handler while we reset the transform to identity.
    private bool _suppressChanges;

    /// <summary>
    /// Fired when the user releases the mouse after a drag.
    /// Arguments: (plane, originBeforeDrag, normalBeforeDrag).
    /// The plane's Origin and Normal have already been updated to the post-drag values.
    /// </summary>
    public event Action<CuttingPlaneEntity, Vector3, Vector3>? DragCompleted;

    public CuttingPlaneManipulator(ViewportManager viewportManager)
    {
        _viewportManager = viewportManager;
        _transform.Changed += OnTransformChanged;
    }

    // ── public API ────────────────────────────────────────────────────────────

    /// <summary>Adds the gizmo to the viewport and starts tracking <paramref name="plane"/>.</summary>
    public void AttachTo(CuttingPlaneEntity plane)
    {
        Detach();

        var viewport = _viewportManager.Viewport;
        if (viewport is null) return;

        _activePlane = plane;
        _isDragging = false;

        _suppressChanges = true;
        _transform.Matrix = Matrix3D.Identity;
        _suppressChanges = false;

        _manipulator = new CombinedManipulator
        {
            Position       = plane.Origin.ToPoint3D(),
            TargetTransform = _transform,
            CanTranslateX  = true,
            CanTranslateY  = true,
            CanTranslateZ  = true,
            CanRotateX     = true,
            CanRotateY     = true,
            CanRotateZ     = true,
            Diameter       = System.Math.Max(plane.DisplayWidth, plane.DisplayHeight) * 0.3
        };

        viewport.Children.Add(_manipulator);
        viewport.PreviewMouseLeftButtonUp += OnViewportMouseUp;
    }

    /// <summary>Removes the gizmo from the viewport.</summary>
    public void Detach()
    {
        if (_activePlane is null) return;

        var viewport = _viewportManager.Viewport;
        if (viewport is not null)
        {
            if (_manipulator is not null)
                viewport.Children.Remove(_manipulator);
            viewport.PreviewMouseLeftButtonUp -= OnViewportMouseUp;
        }

        _manipulator = null;
        _activePlane = null;
        _isDragging = false;
    }

    /// <summary>
    /// Repositions the gizmo to the current <c>plane.Origin</c> without starting a drag.
    /// Call this when the plane is moved by means other than the gizmo (e.g. undo/redo,
    /// properties panel).
    /// </summary>
    public void UpdatePosition()
    {
        if (_activePlane is null || _manipulator is null || _isDragging) return;
        _manipulator.Position = _activePlane.Origin.ToPoint3D();
    }

    public bool IsAttached => _activePlane is not null;
    public Guid? ActivePlaneId => _activePlane?.Id;

    // ── event handlers ────────────────────────────────────────────────────────

    private void OnTransformChanged(object? sender, EventArgs e)
    {
        if (_suppressChanges || _activePlane is null) return;

        var matrix = _transform.Matrix;

        // Ignore the very first notification that fires when TargetTransform is
        // assigned; the matrix is still identity at that point.
        if (matrix.IsIdentity) return;

        if (!_isDragging)
        {
            // First real movement — capture the entity state before this drag.
            _preDragOrigin = _activePlane.Origin;
            _preDragNormal = _activePlane.Normal;
            _isDragging = true;
        }

        // Apply the accumulated transform to the pre-drag snapshot so we always work
        // relative to where the drag started (not incremental per-frame deltas).
        var numerics = ToNumerics(matrix);
        _activePlane.Origin = Vector3.Transform(_preDragOrigin, numerics);
        _activePlane.Normal = Vector3.Normalize(Vector3.TransformNormal(_preDragNormal, numerics));
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _activePlane is null) return;

        _isDragging = false;

        var completedPlane = _activePlane;
        var preDragOrigin  = _preDragOrigin;
        var preDragNormal  = _preDragNormal;

        // Reset the accumulated transform to identity; reposition the gizmo handles
        // to the entity's new world position so the next drag starts correctly.
        _suppressChanges = true;
        _transform.Matrix = Matrix3D.Identity;
        _suppressChanges = false;

        if (_manipulator is not null)
            _manipulator.Position = completedPlane.Origin.ToPoint3D();

        DragCompleted?.Invoke(completedPlane, preDragOrigin, preDragNormal);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a WPF <see cref="Matrix3D"/> (row-major, translation in OffsetX/Y/Z)
    /// to a <see cref="Matrix4x4"/> in the same layout.
    /// </summary>
    private static Matrix4x4 ToNumerics(Matrix3D m) =>
        new(
            (float)m.M11,     (float)m.M12,     (float)m.M13,     (float)m.M14,
            (float)m.M21,     (float)m.M22,     (float)m.M23,     (float)m.M24,
            (float)m.M31,     (float)m.M32,     (float)m.M33,     (float)m.M34,
            (float)m.OffsetX, (float)m.OffsetY, (float)m.OffsetZ, (float)m.M44);
}
