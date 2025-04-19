using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DiscreteSimulation.GUI.ViewModels.Panels;
using ScottPlot;
using ScottPlot.AutoScalers;
using ScottPlot.Plottables;
using Simulation;

namespace DiscreteSimulation.GUI.Views.Panels;

public partial class MultipleReplicationsPanel : UserControl
{
    private MultipleReplicationsPanelViewModel _viewModel;
    
    private readonly List<Coordinates> _replicationsProcessingOrderTimePlotData = new();
    private readonly List<Coordinates> _replicationsProcessingOrderTimeCILowerPlotData = new();
    private readonly List<Coordinates> _replicationsProcessingOrderTimeCIUpperPlotData = new();

    private Scatter? ScatterLineReplicationsCIUpper;
    private Scatter? ScatterLineReplicationsCILower;
    
    private string _selectedCoordinatesTimeUnit;
    private int _skipFirstNReplications = 0;
    
    public MultipleReplicationsPanel()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        _viewModel = (MultipleReplicationsPanelViewModel)DataContext!;
        
        _viewModel.Shared.Simulation.OnSimulationWillStart(SimulationStarted);
        
        _viewModel.Shared.ReplicationEnded += ReplicationEnded;
        
        _viewModel.Shared.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(_viewModel.Shared.EnableRender95ConfidenceInterval))
            {
                Render95ConfidenceIntervalCheckboxChanged();
            }
            else if (args.PropertyName == nameof(_viewModel.Shared.SelectedTimeUnits))
            {
                UpdateChartTimeUnits();
            }
        };
        
        _selectedCoordinatesTimeUnit = _viewModel.Shared.SelectedTimeUnits;

        SetupCharts();
    }
    
    private void SimulationStarted(OSPABA.Simulation simulation)
    {
        _replicationsProcessingOrderTimePlotData.Clear();
        _replicationsProcessingOrderTimeCILowerPlotData.Clear();
        _replicationsProcessingOrderTimeCIUpperPlotData.Clear();

        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
        
        _skipFirstNReplications = Convert.ToInt32(_viewModel.Shared.Replications * (_viewModel.SkipFirstNReplicationsInPercent / 100.0));
    }

    private void ReplicationEnded(OSPABA.Simulation simulation)
    {
        var mySimulation = (MySimulation)simulation;
        
        var currentReplication = mySimulation.CurrentReplication + 1;
        
        var averageProcessingOrderTime = _viewModel.Shared.Simulation.AverageProcessingOrderTime.Mean;
        var averageProcessingOrderTimeCI = _viewModel.Shared.Simulation.AverageProcessingOrderTime.ConfidenceInterval95();
        
        // Berieme iba kazdu (RenderOffset + 1)-tu replikaciu pre vykreslenie, ale ak sme
        // uz na poslednej replikacii, a modulo nevyslo na 0, tak sa zobrazi aj tak
        if ((currentReplication - _skipFirstNReplications) % (_viewModel.RenderOffset + 1) != 0 
            && currentReplication != _viewModel.Shared.Simulation.ReplicationCount)
        {
            return;
        }
        
        // Preskocime prvych niekolko replikacii
        if (currentReplication < _skipFirstNReplications)
        {
            return;
        }
        
        Dispatcher.UIThread.Post(() => AddValueToProcessingOrderTimePlot(
            currentReplication,
            averageProcessingOrderTime,
            averageProcessingOrderTimeCI.Item1,
            averageProcessingOrderTimeCI.Item2
        ));
    }
    
    private void TimeUnitsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        _viewModel.Shared.SelectedTimeUnits = menuItem.Header.ToString();
    }

    private void UpdateChartTimeUnits()
    {
        var newData = new List<Coordinates>();
        
        // Replication data
        foreach (var coordinates in _replicationsProcessingOrderTimePlotData)
        {
            var newX = coordinates.X;
            var newY = RecalculateCoordinateValue(coordinates.Y);
            
            newData.Add(new Coordinates(newX, newY));
        }
        
        _replicationsProcessingOrderTimePlotData.Clear();
        
        foreach (var coordinates in newData)
        {
            _replicationsProcessingOrderTimePlotData.Add(new Coordinates(coordinates.X, coordinates.Y));
        }
        
        // Lower CI data
        newData.Clear();
        
        foreach (var coordinates in _replicationsProcessingOrderTimeCILowerPlotData)
        {
            var newX = coordinates.X;
            var newY = RecalculateCoordinateValue(coordinates.Y);
            
            newData.Add(new Coordinates(newX, newY));
        }
        
        _replicationsProcessingOrderTimeCILowerPlotData.Clear();
        
        foreach (var coordinates in newData)
        {
            _replicationsProcessingOrderTimeCILowerPlotData.Add(new Coordinates(coordinates.X, coordinates.Y));
        }
        
        // Upper CI data
        newData.Clear();
        
        foreach (var coordinates in _replicationsProcessingOrderTimeCIUpperPlotData)
        {
            var newX = coordinates.X;
            var newY = RecalculateCoordinateValue(coordinates.Y);
            
            newData.Add(new Coordinates(newX, newY));
        }
        
        _replicationsProcessingOrderTimeCIUpperPlotData.Clear();
        
        foreach (var coordinates in newData)
        {
            _replicationsProcessingOrderTimeCIUpperPlotData.Add(new Coordinates(coordinates.X, coordinates.Y));
        }
        
        _selectedCoordinatesTimeUnit = _viewModel.Shared.SelectedTimeUnits;
        
        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
    }
    
    private void AddValueToProcessingOrderTimePlot(long replication, double processingOrderTime, double timeLower, double timeUpper)
    {
        var newY = RecalculateCoordinateValue(processingOrderTime, "seconds");
        var newYLower = RecalculateCoordinateValue(timeLower, "seconds");
        var newYUpper = RecalculateCoordinateValue(timeUpper, "seconds");
        
        _replicationsProcessingOrderTimePlotData.Add(new Coordinates(replication, newY));
        _replicationsProcessingOrderTimeCILowerPlotData.Add(new Coordinates(replication, newYLower));
        _replicationsProcessingOrderTimeCIUpperPlotData.Add(new Coordinates(replication, newYUpper));
        
        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
    }
    
    private void SetupCharts()
    {
        var scatterLineReplications = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimePlotData, Colors.Red);
        scatterLineReplications.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        ScatterLineReplicationsCIUpper = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimeCIUpperPlotData, Colors.Gray);
        ScatterLineReplicationsCIUpper.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        ScatterLineReplicationsCILower = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimeCILowerPlotData, Colors.Gray);
        ScatterLineReplicationsCILower.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        ProcessingOrderTimePlot.Plot.Axes.Bottom.Label.Text = "Replication";
        ProcessingOrderTimePlot.Plot.Axes.Left.Label.Text = "Processing order time";

        ProcessingOrderTimePlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
    }
    
    private double RecalculateCoordinateValue(double coordinate, string? currentTimeUnit = null)
    {
        if (currentTimeUnit == null)
        {
            currentTimeUnit = _selectedCoordinatesTimeUnit;
        }
        
        return currentTimeUnit switch
        {
            "seconds" => _viewModel.Shared.SelectedTimeUnits switch
            {
                "seconds" => coordinate,
                "minutes" => coordinate / 60,
                "hours" => coordinate / 3600,
                _ => coordinate
            },
            "minutes" => _viewModel.Shared.SelectedTimeUnits switch
            {
                "seconds" => coordinate * 60,
                "minutes" => coordinate,
                "hours" => coordinate / 60,
                _ => coordinate
            },
            "hours" => _viewModel.Shared.SelectedTimeUnits switch
            {
                "seconds" => coordinate * 3600,
                "minutes" => coordinate * 60,
                "hours" => coordinate,
                _ => coordinate
            },
        };
    }
    
    private void Render95ConfidenceIntervalCheckboxChanged()
    {
        if (ScatterLineReplicationsCIUpper == null || ScatterLineReplicationsCILower == null)
        {
            return;
        }

        ScatterLineReplicationsCIUpper.IsVisible = _viewModel.Shared.EnableRender95ConfidenceInterval;
        ScatterLineReplicationsCILower.IsVisible = _viewModel.Shared.EnableRender95ConfidenceInterval;
        
        ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        ProcessingOrderTimePlot.Refresh();
    }
}
