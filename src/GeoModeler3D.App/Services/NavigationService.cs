namespace GeoModeler3D.App.Services;

/// <summary>Service for navigating between views/panels in the application.</summary>
public class NavigationService
{
    public event Action<string>? NavigationRequested;

    public void NavigateTo(string viewName)
    {
        NavigationRequested?.Invoke(viewName);
    }
}
