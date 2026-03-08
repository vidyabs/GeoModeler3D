namespace GeoModeler3D.Core.Animation;

/// <summary>Easing function implementations for animation interpolation.</summary>
public static class EasingFunctions
{
    public static double Apply(EasingType type, double t)
    {
        return type switch
        {
            EasingType.Linear => t,
            EasingType.EaseInQuad => t * t,
            EasingType.EaseOutQuad => t * (2 - t),
            EasingType.EaseInOutQuad => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t,
            _ => t
        };
    }
}
