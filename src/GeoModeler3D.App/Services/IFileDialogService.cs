namespace GeoModeler3D.App.Services;

public interface IFileDialogService
{
    string? ShowOpenFileDialog(string filter, string title = "Open");
    string? ShowSaveFileDialog(string filter, string title = "Save");
}
