using Avalonia.Controls;
using Avalonia.Interactivity;
using DiscreteSimulation.GUI.ViewModels.Panels;

namespace DiscreteSimulation.GUI.Views.Panels;

public partial class SingleReplicationPanel : UserControl
{
    private SingleReplicationPanelViewModel _viewModel;
    
    public SingleReplicationPanel()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        _viewModel = (SingleReplicationPanelViewModel)DataContext!;
    }

    private void TimeUnitsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        _viewModel.Shared.SelectedTimeUnits = menuItem.Header.ToString();
    }
}
