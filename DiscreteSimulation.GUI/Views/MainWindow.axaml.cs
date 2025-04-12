using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DiscreteSimulation.FurnitureManufacturer.DTOs;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using DiscreteSimulation.GUI.ViewModels;
using ScottPlot;
using ScottPlot.AutoScalers;
using ScottPlot.Plottables;
using Simulation;

namespace DiscreteSimulation.GUI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    
    private readonly List<Coordinates> _replicationsProcessingOrderTimePlotData = new();
    private readonly List<Coordinates> _replicationsProcessingOrderTimeCILowerPlotData = new();
    private readonly List<Coordinates> _replicationsProcessingOrderTimeCIUpperPlotData = new();

    private Scatter? ScatterLineReplicationsCIUpper;
    private Scatter? ScatterLineReplicationsCILower;

    private string _selectedCoordinatesTimeUnit;
    private int _skipFirstNReplications = 0;
    private bool _stopSimulationRequested = false;

    private MySimulation _mySimulation;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        
        _selectedCoordinatesTimeUnit = _viewModel.Shared.SelectedTimeUnits;
        
        SetupCharts();

        _viewModel.Shared.Simulation.SimulationStateChanged += SimulationStateChanged;

        _viewModel.Shared.Simulation.ReplicationEnded += ReplicationEnded;

        _viewModel.Shared.Simulation.SimulationEnded += SimulationEnded;
        
        // -----
        _mySimulation = new MySimulation();
        
        _mySimulation.OnRefreshUI(simulation =>
        {
            var currentQueueLength = _mySimulation.ResourcesAgent.CustomersQueue.Count;
            Console.WriteLine($"Current queue length: {currentQueueLength}");
        });
        
        _mySimulation.OnReplicationDidFinish(simulation =>
        {
            Console.WriteLine($"Replication finished AVG waiting time: {_mySimulation.ResourcesAgent.CustomersQueueWaitingTime.Mean}");
        });
        
        _mySimulation.OnSimulationDidFinish(simulation =>
        {
            Console.WriteLine($"Simulation finished AVG waiting time: {_mySimulation.AverageCustomersQueueWaitingTime.Mean}");
        });
    }

    private void SimulationEnded()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.EnableButtonsForSimulationEnd();
            SimulationStateChanged();
        });
    }

    private async void StartSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        /*
        var averageWaitingTime = simulation.AverageCustomersQueueWaitingTime.Mean;
        var averageQueueLength = simulation.AverageCustomersQueueLength.Mean;

        Console.WriteLine($"*Average Waiting Time: {averageWaitingTime}");
        Console.WriteLine($"*Average Queue Length: {averageQueueLength}");

        Console.WriteLine("Finished...");
         */
        _mySimulation.SetSimSpeed(1, 0.1);
        Console.WriteLine("Simulating...");
        await Task.Run(() => _mySimulation.Simulate(1, 10_000));
        
        return;
        
        _viewModel.DisableButtonsForSimulationStart();

        _replicationsProcessingOrderTimePlotData.Clear();
        _replicationsProcessingOrderTimeCILowerPlotData.Clear();
        _replicationsProcessingOrderTimeCIUpperPlotData.Clear();
        
        // TODO
        //ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        //ProcessingOrderTimePlot.Refresh();
        
        _skipFirstNReplications = Convert.ToInt32(_viewModel.Shared.Replications * (_viewModel.MultipleReplications.SkipFirstNReplicationsInPercent / 100.0));
        
        _viewModel.Shared.Simulation.MaxReplicationTime = _viewModel.Shared.MaxReplicationTime;
        _viewModel.Shared.Simulation.SimulationSpeed = _viewModel.Shared.SpeedOptions.ElementAt(_viewModel.Shared.SelectedSpeedIndex).Key;
        
        // TODO
        // _viewModel.Shared.Simulation.CountOfAssemblyLines = _viewModel.Shared.CountOfAssemblyLines;
        _viewModel.Shared.Simulation.CountOfWorkersGroupA = _viewModel.Shared.CountOfWorkersGroupA;
        _viewModel.Shared.Simulation.CountOfWorkersGroupB = _viewModel.Shared.CountOfWorkersGroupB;
        _viewModel.Shared.Simulation.CountOfWorkersGroupC = _viewModel.Shared.CountOfWorkersGroupC;
        _viewModel.Shared.Simulation.EnableWorkerLocationPreference = _viewModel.Shared.EnableWorkerLocationPreference;
        
        _latestGUIUpdate = -1;
        _skippedGUIUpdates = 0;

        _viewModel.SingleReplication.ResetForSimulationStart();
        _viewModel.MultipleReplications.ResetForSimulationStart();
        
        await Task.Run(() => _viewModel.Simulation.StartSimulation(_viewModel.Shared.Replications));
    }

    private void StopSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _stopSimulationRequested = true;
        _viewModel.Simulation.StopSimulation();
    }
    
    private void PauseResumeSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.Shared.PauseResumeSimulation();
    
    private void SpeedOneSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetDefaultSpeed();
    
    private void DecreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.DecreaseSpeed();

    private void IncreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.IncreaseSpeed();
    
    private void SpeedMaxSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetMaxSpeed();

    private double _latestGUIUpdate = -1;
    private int _skippedGUIUpdates = 0;

    private void SimulationStateChanged()
    {
        var simulationTime = _viewModel.Simulation.SimulationTime;
        var simulationSpeed = _viewModel.Simulation.SimulationSpeed;
        var simulationDelta = _viewModel.Simulation.Delta;

        if (!double.IsInfinity(simulationSpeed))
        {
            // Rozdiel posledneho updatu GUI a aktualneho casu simulacie
            var differenceCurrentAndLast = simulationTime - _latestGUIUpdate;

            // Rychlost updatovania GUI je zavisle od delta
            var customParameter = 0.025;
            var minimalDifference = simulationDelta * customParameter;
        
            if (differenceCurrentAndLast < minimalDifference && !_viewModel.Simulation.IsSimulationPaused && _viewModel.Simulation.IsSimulationRunning)
            {
                return;
            }
        }
        else
        {
            // Ak je rychlost nekonecna, updatovanie nemoze byt zavisle od simulačného času pretože, updaty
            // neprichádzajú cca v ekvidistantných časových intervaloch, preto pocitame preskocenie updatov 
            if (_skippedGUIUpdates <= 10_000 && !_viewModel.Simulation.IsSimulationPaused && _viewModel.Simulation.IsSimulationRunning)
            {
                _skippedGUIUpdates++;
                return;
            }
            
            _skippedGUIUpdates = 0;
        }
        
        _latestGUIUpdate = simulationTime;

        // V prípade že simulácia skončila, zobrazíme maximálny čas simulácie.
        // Väčšinou nastane, že ďalšie udalosti sú za hranicou max. času simulácie,
        // čiže čas "nedobehne" na maximálny čas simulácie
        // (aby mal používateľ pocit, že simulácia skutočne bežala až do konca).
        if (!_viewModel.Simulation.IsSimulationRunning && !_stopSimulationRequested)
        {
            simulationTime = _viewModel.Simulation.MaxReplicationTime;
        }
        
        var averageProcessingOrderTime = _viewModel.Simulation.AverageProcessingOrderTime.Mean;
        
        // TODO: refresh novych 2 frontov
        _viewModel.Simulation.PendingOrdersQueue.RefreshStatistics();
        _viewModel.Simulation.PendingCutMaterialsQueue.RefreshStatistics();
        _viewModel.Simulation.PendingVarnishedMaterialsQueue.RefreshStatistics();
        _viewModel.Simulation.PendingFoldedClosetsQueue.RefreshStatistics();
        
        var averagePendingOrders = _viewModel.Simulation.PendingOrdersQueue.AverageQueueLength;
        // TODO:
        var averagePendingFurnitureItems = -1;
        var averagePendingItemsForStaining = _viewModel.Simulation.PendingCutMaterialsQueue.AverageQueueLength;
        // TODO:
        var averagePendingItemsForVarnishing = -1;
        var averagePendingItemsForFolding = _viewModel.Simulation.PendingVarnishedMaterialsQueue.AverageQueueLength;
        var averagePendingItemsForFittings = _viewModel.Simulation.PendingFoldedClosetsQueue.AverageQueueLength;

        var averagePendingOrdersWaitingTime = _viewModel.Simulation.AverageWaitingTimeInPendingOrdersQueue.Mean;
        // TODO:
        var averageWaitingTimePendingFurnitureItems = -1;
        var averagePendingItemsForStainingWaitingTime = _viewModel.Simulation.AverageWaitingTimeInPendingCutMaterialsQueue.Mean;
        // TODO:
        var averagePendingItemsForVarnishingWaitingTime = -1;
        var averagePendingItemsForFoldingWaitingTime = _viewModel.Simulation.AverageWaitingTimeInPendingVarnishedMaterialsQueue.Mean;
        var averagePendingItemsForFittingsWaitingTime = _viewModel.Simulation.AverageWaitingTimeInPendingFoldedClosetsQueue.Mean;
        
            var pendingOrdersQueueCount = _viewModel.Simulation.PendingOrdersQueue.Count;
        // TODO:
        var pendingFurnitureItemsQueueCount = -1;
        var pendingForStainingQueueCount = _viewModel.Simulation.PendingCutMaterialsQueue.Count;
        // TODO:
        var pendingForVarnishingQueueCount = -1;
        var pendingForFoldingQueueCount = _viewModel.Simulation.PendingVarnishedMaterialsQueue.Count;
        var pendingForFittingsQueueCount = _viewModel.Simulation.PendingFoldedClosetsQueue.Count;
        
        // Refresh štatistík vyťaženia pracovníkov
        foreach (Worker worker in _viewModel.Simulation.WorkersGroupA)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupAUtilization = _viewModel.Simulation.WorkersGroupA.Average(worker => worker.Utilization) * 100;

        foreach (Worker worker in _viewModel.Simulation.WorkersGroupB)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupBUtilization = _viewModel.Simulation.WorkersGroupB.Average(worker => worker.Utilization) * 100;
        
        foreach (Worker worker in _viewModel.Simulation.WorkersGroupC)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupCUtilization = _viewModel.Simulation.WorkersGroupC.Average(worker => worker.Utilization) * 100;

        var orders = _viewModel.Simulation.GetCurrentOrderDTOs();
        var pendingOrders = _viewModel.Simulation.GetCurrentPendingOrdersQueue();
        // TODO:
        var pendingFurnitureItems = new List<FurnitureDTO>();
        var pendingForStainingQueue = _viewModel.Simulation.GetCurrentPendingCutMaterialsQueue();
        // TODO:
        var pendingForVarnishingQueue = new List<FurnitureDTO>();
        var pendingForFoldingQueue = _viewModel.Simulation.GetCurrentPendingVarnishedMaterialsQueue();
        var pendingForFittingsQueue = _viewModel.Simulation.GetCurrentPendingFoldedClosetsQueue();
        var assemblyLines = _viewModel.Simulation.GetCurrentAssemblyLineDTOs();
        var workersGroupA = _viewModel.Simulation.GetCurrentWorkerGroupADTOs();
        var workersGroupB = _viewModel.Simulation.GetCurrentWorkerGroupBDTOs();
        var workersGroupC = _viewModel.Simulation.GetCurrentWorkerGroupCDTOs();
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.SingleReplication.CurrentSimulationTime = simulationTime.FormatToSimulationTime();
            _viewModel.SingleReplication.SetReplicationOrderProcessingTime(averageProcessingOrderTime);
            
            _viewModel.SingleReplication.ReplicationPendingOrders = $"{averagePendingOrders:F2}";
            _viewModel.SingleReplication.ReplicationPendingFurnitureItems = $"{averagePendingFurnitureItems:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForStaining = $"{averagePendingItemsForStaining:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForVarnishing = $"{averagePendingItemsForVarnishing:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForFolding = $"{averagePendingItemsForFolding:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForFittings = $"{averagePendingItemsForFittings:F2}";
            
            _viewModel.SingleReplication.SetReplicationPendingOrdersWaitingTime(averagePendingOrdersWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingFurnitureItemsWaitingTime(averageWaitingTimePendingFurnitureItems);
            _viewModel.SingleReplication.SetReplicationPendingItemsForStainingWaitingTime(averagePendingItemsForStainingWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForVarnishingWaitingTime(averagePendingItemsForVarnishingWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForFoldingWaitingTime(averagePendingItemsForFoldingWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForFittingsWaitingTime(averagePendingItemsForFittingsWaitingTime);
            
            _viewModel.SingleReplication.PendingOrdersQueueCount = $"{pendingOrdersQueueCount}";
            _viewModel.SingleReplication.PendingFurnitureItemsQueueCount = $"{pendingFurnitureItemsQueueCount}";
            _viewModel.SingleReplication.PendingForStainingQueueCount = $"{pendingForStainingQueueCount}";
            _viewModel.SingleReplication.PendingForVarnishingQueueCount = $"{pendingForVarnishingQueueCount}";
            _viewModel.SingleReplication.PendingForFoldingQueueCount = $"{pendingForFoldingQueueCount}";
            _viewModel.SingleReplication.PendingForFittingsQueueCount = $"{pendingForFittingsQueueCount}";
            _viewModel.SingleReplication.ReplicationWorkersGroupAUtilization = $"{workersGroupAUtilization:F2}";
            _viewModel.SingleReplication.ReplicationWorkersGroupBUtilization = $"{workersGroupBUtilization:F2}";
            _viewModel.SingleReplication.ReplicationWorkersGroupCUtilization = $"{workersGroupCUtilization:F2}";
            
            SynchronizeCollection(_viewModel.SingleReplication.Orders, orders);

            // Ak je selectnuta objednavka v GUI, tak aktualizujeme aj jej polozky
            if (_viewModel.SingleReplication.SelectedOrder != null)
            {
                var selectedOrder = _viewModel.SingleReplication.Orders.FirstOrDefault(o => o.Id == _viewModel.SingleReplication.SelectedOrder.Id);
                SynchronizeCollection(_viewModel.SingleReplication.SelectedOrderFurnitureItems, selectedOrder.FurnitureItems);
            }
            
            SynchronizeCollection(_viewModel.SingleReplication.AssemblyLines, assemblyLines);
            SynchronizeCollection(_viewModel.SingleReplication.WorkersGroupA, workersGroupA);
            SynchronizeCollection(_viewModel.SingleReplication.WorkersGroupB, workersGroupB);
            SynchronizeCollection(_viewModel.SingleReplication.WorkersGroupC, workersGroupC);
            SynchronizeCollection(_viewModel.SingleReplication.PendingOrdersQueue, pendingOrders);
            SynchronizeCollection(_viewModel.SingleReplication.PendingFurnitureItemsQueue, pendingFurnitureItems);
            SynchronizeCollection(_viewModel.SingleReplication.PendingForStainingQueue, pendingForStainingQueue);
            SynchronizeCollection(_viewModel.SingleReplication.PendingForVarnishingQueue, pendingForVarnishingQueue);
            SynchronizeCollection(_viewModel.SingleReplication.PendingForFoldingQueue, pendingForFoldingQueue);
            SynchronizeCollection(_viewModel.SingleReplication.PendingForFittingsQueue, pendingForFittingsQueue);
        });
    }

    private void ReplicationEnded()
    {
        var currentReplication = _viewModel.Simulation.CurrentReplication;
        
        // V label popiskoch robime refresh bud kazdych 1000 replikacii
        // alebo podla nastavenia RenderOffset respektive poslednu replikaciu
        // (ak module nevyslo na 0) pre aktualnost
        if ((currentReplication % 1000 != 0) && (currentReplication - _skipFirstNReplications) % (_viewModel.MultipleReplications.RenderOffset + 1) != 0 && currentReplication != _viewModel.Simulation.CurrentMaxReplications)
        {
            return;
        }
        
        var averageProcessingOrderTime = _viewModel.Simulation.SimulationAverageProcessingOrderTime.Mean;
        var averageProcessingOrderTimeCI = _viewModel.Simulation.SimulationAverageProcessingOrderTime.ConfidenceInterval95();
        var averagePendingOrdersCount = _viewModel.Simulation.SimulationAveragePendingOrdersCount.Mean;
        var averagePendingOrdersCountCI = _viewModel.Simulation.SimulationAveragePendingOrdersCount.ConfidenceInterval95();
        // TODO:
        var averagePendingFurnitureItemsCount = -1.0;
        var averagePendingFurnitureItemsCountCI = (-1.0, -1.0);
        var averagePendingItemsForStaining = _viewModel.Simulation.SimulationAveragePendingCutMaterialsCount.Mean;
        var averagePendingItemsForStainingCI = _viewModel.Simulation.SimulationAveragePendingCutMaterialsCount.ConfidenceInterval95();
        // TODO:
        var averagePendingItemsForVarnishing = -1.0;
        var averagePendingItemsForVarnishingCI = (-1.0, -1.0);
        var averagePendingItemsForFolding = _viewModel.Simulation.SimulationAveragePendingVarnishedMaterialsCount.Mean;
        var averagePendingItemsForFoldingCI = _viewModel.Simulation.SimulationAveragePendingVarnishedMaterialsCount.ConfidenceInterval95();
        var averagePendingItemsForFittings = _viewModel.Simulation.SimulationAveragePendingFoldedClosetsCount.Mean;
        var averagePendingItemsForFittingsCI = _viewModel.Simulation.SimulationAveragePendingFoldedClosetsCount.ConfidenceInterval95();
        
        var averagePendingOrdersWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingOrdersQueue.Mean;
        var averagePendingOrdersWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingOrdersQueue.ConfidenceInterval95();
        // TODO:
        var averagePendingFurnitureItemsWaitingTime = -1.0;
        var averagePendingFurnitureItemsWaitingTimeCI = (-1.0, -1.0);
        var averageItemsForStainingWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingCutMaterialsQueue.Mean;
        var averageItemsForStainingWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingCutMaterialsQueue.ConfidenceInterval95();
        // TODO:
        var averageItemsForVarnishingWaitingTime = -1.0;
        var averageItemsForVarnishingWaitingTimeCI = (-1.0, -1.0);
        var averageItemsForFoldingWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingVarnishedMaterialsQueue.Mean;
        var averageItemsForFoldingWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingVarnishedMaterialsQueue.ConfidenceInterval95();
        var averageItemsForFittingsWaitingTime = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingFoldedClosetsQueue.Mean;
        var averageItemsForFittingsWaitingTimeCI = _viewModel.Simulation.SimulationAverageWaitingTimeInPendingFoldedClosetsQueue.ConfidenceInterval95();
        
        
        var averageWorkersGroupAUtilization = _viewModel.Simulation.SimulationAverageWorkersGroupAUtilization.Mean;
        var averageWorkersGroupAUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupAUtilization.ConfidenceInterval95();
        var averageWorkersGroupBUtilization = _viewModel.Simulation.SimulationAverageWorkersGroupBUtilization.Mean;
        var averageWorkersGroupBUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupBUtilization.ConfidenceInterval95();
        var averageWorkersGroupCUtilization = _viewModel.Simulation.SimulationAverageWorkersGroupCUtilization.Mean;
        var averageWorkersGroupCUtilizationCI = _viewModel.Simulation.SimulationAverageWorkersGroupCUtilization.ConfidenceInterval95();

        var allWorkersUtilization = _viewModel.Simulation.GetAllWorkersSimulationUtilization();
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.MultipleReplications.CurrentReplication = currentReplication.ToString();
            _viewModel.MultipleReplications.SetSimulationCurrentProcessingOrderTime(averageProcessingOrderTime, averageProcessingOrderTimeCI.Item1, averageProcessingOrderTimeCI.Item2);
            
            _viewModel.MultipleReplications.SimulationPendingOrders = $"{averagePendingOrdersCount:F2}";
            _viewModel.MultipleReplications.SimulationPendingOrdersCI = $"<{averagePendingOrdersCountCI.Item1:F2} ; {averagePendingOrdersCountCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingFurnitureItems = $"{averagePendingFurnitureItemsCount:F2}";
            _viewModel.MultipleReplications.SimulationPendingFurnitureItemsCI = $"<{averagePendingFurnitureItemsCountCI.Item1:F2} ; {averagePendingFurnitureItemsCountCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForStaining = $"{averagePendingItemsForStaining:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForStainingCI = $"<{averagePendingItemsForStainingCI.Item1:F2} ; {averagePendingItemsForStainingCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForVarnishing = $"{averagePendingItemsForVarnishing:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForVarnishingCI = $"<{averagePendingItemsForVarnishingCI.Item1:F2} ; {averagePendingItemsForVarnishingCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForFolding = $"{averagePendingItemsForFolding:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForFoldingCI = $"<{averagePendingItemsForFoldingCI.Item1:F2} ; {averagePendingItemsForFoldingCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForFittings = $"{averagePendingItemsForFittings:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForFittingsCI = $"<{averagePendingItemsForFittingsCI.Item1:F2} ; {averagePendingItemsForFittingsCI.Item2:F2}>";
            
            _viewModel.MultipleReplications.SetSimulationPendingOrdersWaitingTime(
                averagePendingOrdersWaitingTime, 
                averagePendingOrdersWaitingTimeCI.Item1, 
                averagePendingOrdersWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationPendingFurnitureItemsWaitingTime(
                averagePendingFurnitureItemsWaitingTime, 
                averagePendingFurnitureItemsWaitingTimeCI.Item1, 
                averagePendingFurnitureItemsWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationItemsForStainingWaitingTime(
                averageItemsForStainingWaitingTime, 
                averageItemsForStainingWaitingTimeCI.Item1, 
                averageItemsForStainingWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationItemsForVarnishingWaitingTime(
                averageItemsForVarnishingWaitingTime, 
                averageItemsForVarnishingWaitingTimeCI.Item1, 
                averageItemsForVarnishingWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationItemsForFoldingWaitingTime(
                averageItemsForFoldingWaitingTime, 
                averageItemsForFoldingWaitingTimeCI.Item1, 
                averageItemsForFoldingWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationItemsForFittingsWaitingTime(
                averageItemsForFittingsWaitingTime, 
                averageItemsForFittingsWaitingTimeCI.Item1, 
                averageItemsForFittingsWaitingTimeCI.Item2
            );
            
            _viewModel.MultipleReplications.SimulationWorkersAUtilization = $"{averageWorkersGroupAUtilization * 100:F2}";
            _viewModel.MultipleReplications.SimulationWorkersAUtilizationCI = $"<{(averageWorkersGroupAUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupAUtilizationCI.Item2*100):F2}>";
            _viewModel.MultipleReplications.SimulationWorkersBUtilization = $"{averageWorkersGroupBUtilization * 100:F2}";
            _viewModel.MultipleReplications.SimulationWorkersBUtilizationCI = $"<{(averageWorkersGroupBUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupBUtilizationCI.Item2*100):F2}>";
            _viewModel.MultipleReplications.SimulationWorkersCUtilization = $"{averageWorkersGroupCUtilization * 100:F2}";
            _viewModel.MultipleReplications.SimulationWorkersCUtilizationCI = $"<{(averageWorkersGroupCUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupCUtilizationCI.Item2*100):F2}>";
            
            for (int i = 0; i < allWorkersUtilization.Count; i++)
            {
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].Id = allWorkersUtilization[i].Id;
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].State = allWorkersUtilization[i].State;
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].Utilization = allWorkersUtilization[i].Utilization;
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].Place = allWorkersUtilization[i].Place;
            }
        });
        
        // Berieme iba kazdu (RenderOffset + 1)-tu replikaciu pre vykreslenie
        // Ale ak sme uz na poslednej replikacii, a modulo nevyslo na 0,
        // tak sa zobrazi aj tak
        if ((currentReplication - _skipFirstNReplications) % (_viewModel.MultipleReplications.RenderOffset + 1) != 0 
            && currentReplication != _viewModel.Simulation.CurrentMaxReplications)
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

    private void SynchronizeCollection<TItem>(ObservableCollection<TItem> targetList, List<TItem> sourceList) where TItem : IUpdatable<TItem>
    {
        // Aktualizácia položiek
        for (int i = 0; i < targetList.Count && i < sourceList.Count; i++)
        {
            targetList[i].Update(sourceList[i]);
        }
        
        // Pridanie chýbajúcich položiek v kolekcii
        for (int i = targetList.Count; i < sourceList.Count; i++)
        {
            var item = Activator.CreateInstance<TItem>();
            item.Update(sourceList[i]);
            targetList.Add(item);
        }
        
        // Odstránenie položiek v kolekcii navyše
        for (int i = targetList.Count - 1; i >= sourceList.Count; i--)
        {
            targetList.RemoveAt(i);
        }
    }
    
    private void AddValueToProcessingOrderTimePlot(long replication, double processingOrderTime, double timeLower, double timeUpper)
    {
        var newY = RecalculateCoordinateValue(processingOrderTime, "seconds");
        var newYLower = RecalculateCoordinateValue(timeLower, "seconds");
        var newYUpper = RecalculateCoordinateValue(timeUpper, "seconds");
        
        _replicationsProcessingOrderTimePlotData.Add(new Coordinates(replication, newY));
        _replicationsProcessingOrderTimeCILowerPlotData.Add(new Coordinates(replication, newYLower));
        _replicationsProcessingOrderTimeCIUpperPlotData.Add(new Coordinates(replication, newYUpper));
        
        // TODO
        //ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        //ProcessingOrderTimePlot.Refresh();
    }
    
    private void SetupCharts()
    {
        // TODO
        //var scatterLineReplications = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimePlotData, Colors.Red);
        //scatterLineReplications.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        //ScatterLineReplicationsCIUpper = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimeCIUpperPlotData, Colors.Gray);
        //ScatterLineReplicationsCIUpper.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        //ScatterLineReplicationsCILower = ProcessingOrderTimePlot.Plot.Add.ScatterLine(_replicationsProcessingOrderTimeCILowerPlotData, Colors.Gray);
        //ScatterLineReplicationsCILower.PathStrategy = new ScottPlot.PathStrategies.Straight();
        
        //ProcessingOrderTimePlot.Plot.Axes.Bottom.Label.Text = "Replication";
        //ProcessingOrderTimePlot.Plot.Axes.Left.Label.Text = "Processing order time";

        //ProcessingOrderTimePlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        _viewModel.Shared.SelectedTimeUnits = menuItem.Header.ToString();
        
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
        
        // TODO
        //ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        //ProcessingOrderTimePlot.Refresh();
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

    private void Render95ConfidenceIntervalCheckbox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        
        if (ScatterLineReplicationsCIUpper == null || ScatterLineReplicationsCILower == null)
        {
            return;
        }
        
        ScatterLineReplicationsCIUpper.IsVisible = checkBox.IsChecked == true;
        ScatterLineReplicationsCILower.IsVisible = checkBox.IsChecked == true;
        
        // TODO
        //ProcessingOrderTimePlot.Plot.Axes.AutoScale();
        //ProcessingOrderTimePlot.Refresh();
    }
}
