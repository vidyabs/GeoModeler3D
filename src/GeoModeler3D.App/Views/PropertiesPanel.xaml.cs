using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoModeler3D.App.ViewModels;

namespace GeoModeler3D.App.Views;

public partial class PropertiesPanel : UserControl
{
    public PropertiesPanel()
    {
        InitializeComponent();
    }

    private void OnEditLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (DataContext is PropertiesPanelViewModel vm)
            vm.CommitEdit(tb.Tag as string, tb.Text);
    }

    private void OnEditKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (sender is not TextBox tb) return;
        if (DataContext is PropertiesPanelViewModel vm)
            vm.CommitEdit(tb.Tag as string, tb.Text);
        // Move focus so the TextBox refreshes its display from the updated binding
        tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        e.Handled = true;
    }
}
