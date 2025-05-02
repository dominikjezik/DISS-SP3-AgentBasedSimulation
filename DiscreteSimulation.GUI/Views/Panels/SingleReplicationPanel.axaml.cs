using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using DiscreteSimulation.GUI.ViewModels.Panels;
using OSPAnimator;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms.Integration;

namespace DiscreteSimulation.GUI.Views.Panels;

public partial class SingleReplicationPanel : Avalonia.Controls.UserControl
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
        
        _viewModel.AnimatorRequestChanged += OnAnimatorRequestChanged;
        
        _viewModel.Shared.AnimatorDeleted += DeleteCanvas;

        OnAnimatorRequestChanged();
    }
    
    private void OnAnimatorRequestChanged()
    {
        if (_viewModel.Shared.IsAnimatorOn && !_viewModel.Shared.Simulation.AnimatorExists)
        {
            _viewModel.Shared.Simulation.CreateAnimator();

            var frameworkElementCanvas = _viewModel.Shared.Simulation.Animator.Canvas;

            _viewModel.Shared.Simulation.Animator.SetSynchronizedTime(false);

            var embedSample = new EmbedFrameworkElement(frameworkElementCanvas);
            MyContentControl.Content = embedSample;
        }
    }
    
    private void DeleteCanvas()
    {
        MyContentControl.Content = null;
    }

    private void TimeUnitsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = sender as Avalonia.Controls.MenuItem;
        _viewModel.Shared.SelectedTimeUnits = menuItem.Header.ToString();
    }
}
