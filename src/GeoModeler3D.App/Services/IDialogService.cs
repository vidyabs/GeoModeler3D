namespace GeoModeler3D.App.Services;

public interface IDialogService
{
    bool? ShowDialog<T>(object? viewModel = null) where T : System.Windows.Window, new();
    void ShowMessage(string message, string title = "GeoModeler3D");
    bool Confirm(string message, string title = "Confirm");
}
