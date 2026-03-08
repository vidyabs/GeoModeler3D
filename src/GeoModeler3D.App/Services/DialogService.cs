using System.Windows;

namespace GeoModeler3D.App.Services;

public class DialogService : IDialogService
{
    public bool? ShowDialog<T>(object? viewModel = null) where T : Window, new()
    {
        var dialog = new T();
        if (viewModel != null)
            dialog.DataContext = viewModel;
        dialog.Owner = Application.Current.MainWindow;
        return dialog.ShowDialog();
    }

    public void ShowMessage(string message, string title = "GeoModeler3D")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool Confirm(string message, string title = "Confirm")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
               == MessageBoxResult.Yes;
    }
}
