using System.Numerics;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Services;

/// <summary>
/// Determines what type of conic section results from intersecting an infinite
/// cone with a plane, using the angle between the cutting plane and the cone axis
/// relative to the cone's half-angle.
/// </summary>
public static class ConicSectionClassifier
{
    // Tolerance for comparing angles (radians-equivalent, applied to sine values)
    private const float Tolerance = 1e-3f;

    /// <summary>
    /// Classifies the conic section produced by cutting <paramref name="cone"/> with
    /// <paramref name="plane"/>.
    /// </summary>
    /// <remarks>
    /// Let α = cone half-angle = atan2(BaseRadius, Height).
    /// Let β = complement of the angle between the plane normal and the cone axis
    ///         = asin(|dot(planeNormal, coneAxis)|).
    ///
    /// In terms of sines (to avoid an extra trig call):
    ///   s  = |dot(planeNormal, coneAxis)|   (= sin β)
    ///   sα = sin α = BaseRadius / slant-height
    ///
    ///   s ≈ 1         → plane ⊥ axis   → Circle
    ///   s > sα + tol  → β > α          → Ellipse
    ///   |s − sα| ≤ tol → β ≈ α         → Parabola
    ///   s < sα − tol  → β < α          → Hyperbola
    /// </remarks>
    public static ConicSectionType Classify(Plane3D plane, ConeEntity cone)
    {
        float s = MathF.Abs(Vector3.Dot(plane.Normal, cone.Axis));
        s = System.Math.Clamp(s, 0f, 1f);

        double slant = System.Math.Sqrt(cone.BaseRadius * cone.BaseRadius + cone.Height * cone.Height);
        float sinAlpha = (float)(cone.BaseRadius / slant);

        if (s >= 1f - Tolerance)
            return ConicSectionType.Circle;

        float diff = s - sinAlpha;

        if (diff >= Tolerance)
            return ConicSectionType.Ellipse;

        if (MathF.Abs(diff) < Tolerance)
            return ConicSectionType.Parabola;

        return ConicSectionType.Hyperbola;
    }
}
