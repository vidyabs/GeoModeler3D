namespace GeoModeler3D.Core.SceneGraph;

public class LayerManager
{
    public const string DefaultLayer = "Default";

    private readonly List<string> _layers = [DefaultLayer];

    public IReadOnlyList<string> Layers => _layers.AsReadOnly();

    public void AddLayer(string name)
    {
        if (!_layers.Contains(name))
            _layers.Add(name);
    }

    public void RemoveLayer(string name)
    {
        if (name != DefaultLayer)
            _layers.Remove(name);
    }
}
