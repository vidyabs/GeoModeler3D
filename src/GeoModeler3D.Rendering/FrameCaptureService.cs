using System.Windows.Media.Imaging;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

/// <summary>
/// Captures viewport frames to RenderTargetBitmap for animation export.
/// Stub — to be implemented in a future phase.
/// </summary>
public class FrameCaptureService
{
    private HelixViewport3D? _viewport;

    public void Initialize(HelixViewport3D viewport)
    {
        _viewport = viewport;
    }

    public RenderTargetBitmap? CaptureFrame(int width, int height)
    {
        if (_viewport is null) return null;

        var rtb = new RenderTargetBitmap(width, height, 96, 96,
            System.Windows.Media.PixelFormats.Pbgra32);
        rtb.Render(_viewport);
        return rtb;
    }
}
