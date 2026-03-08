using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.Core.Serialization;

/// <summary>Periodically auto-saves the current scene to a recovery file.</summary>
public class AutoSaveService
{
    private readonly SceneManager _sceneManager;
    private readonly ProjectSerializer _serializer;
    private Timer? _timer;

    public int IntervalMinutes { get; set; } = 5;
    public string AutoSavePath { get; set; } = "autosave.geo3d";

    public AutoSaveService(SceneManager sceneManager, ProjectSerializer serializer)
    {
        _sceneManager = sceneManager;
        _serializer = serializer;
    }

    public void Start()
    {
        // TODO: start periodic auto-save timer
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }
}
