using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class TorusEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(TorusEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var torus = (TorusEntity)entity;
        var visual = new TorusVisual3D();
        Apply(torus, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((TorusEntity)entity, (TorusVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(TorusEntity torus, TorusVisual3D visual)
    {
        visual.TorusDiameter = torus.MajorRadius * 2;
        visual.TubeDiameter = torus.MinorRadius * 2;
        visual.ThetaDiv = 64;
        visual.PhiDiv = 32;
        visual.Material = MaterialHelper.CreateMaterial(torus.Color.ToWpfColor());
        visual.Visible = torus.IsVisible;

        // Position and orient
        var transform = new Transform3DGroup();
        transform.Children.Add(ComputeOrientationTransform(torus.Normal));
        transform.Children.Add(new TranslateTransform3D(
            torus.Center.X, torus.Center.Y, torus.Center.Z));
        visual.Transform = transform;
    }

    private static RotateTransform3D ComputeOrientationTransform(Vector3 normal)
    {
        var defaultNormal = new Vector3D(0, 0, 1);
        var targetNormal = normal.ToVector3D();
        targetNormal.Normalize();

        var axis = Vector3D.CrossProduct(defaultNormal, targetNormal);
        if (axis.LengthSquared < 1e-10)
        {
            if (Vector3D.DotProduct(defaultNormal, targetNormal) < 0)
                return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 180));
            return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0));
        }

        axis.Normalize();
        var angle = System.Math.Acos(
            System.Math.Clamp(Vector3D.DotProduct(defaultNormal, targetNormal), -1.0, 1.0));
        return new RotateTransform3D(
            new AxisAngleRotation3D(axis, angle * 180.0 / System.Math.PI));
    }
}
