using Microsoft.Win32;

namespace GeoModeler3D.App.Services;

public class FileDialogService : IFileDialogService
{
    public string? ShowOpenFileDialog(string filter, string title = "Open")
    {
        var dialog = new OpenFileDialog { Filter = filter, Title = title };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter, string title = "Save")
    {
        var dialog = new SaveFileDialog { Filter = filter, Title = title };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
