namespace GeoModeler3D.Core.Animation;

/// <summary>Exports animation frames to image sequences or video.</summary>
public class AnimationExporter
{
    public void ExportFrames(AnimationSequence sequence, string outputDirectory, int width, int height)
    {
        // TODO: render each frame and save as PNG
    }

    public void ExportGif(AnimationSequence sequence, string outputPath, int width, int height)
    {
        // TODO: render frames and encode as animated GIF
    }
}
