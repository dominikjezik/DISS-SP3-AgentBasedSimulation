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
using MyMessage = Simulation.MyMessage;
using MySimulation = Simulation.MySimulation;

namespace DiscreteSimulation.GUI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    
    private bool _stopSimulationRequested = false;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        
        _viewModel.Shared.Simulation.OnRefreshUI(SimulationStateChanged);
        
        _viewModel.Shared.ReplicationEnded += ReplicationEnded;
        
        _viewModel.Shared.Simulation.OnSimulationDidFinish(SimulationEnded);
    }
    
    private void SimulationEnded(OSPABA.Simulation simulation)
    {
        var mySimulation = (MySimulation)simulation;
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.EnableButtonsForSimulationEnd();
            SimulationStateChanged(mySimulation);
        });
    }

    private async void StartSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.DisableButtonsForSimulationStart();
        
        _viewModel.Simulation.CountOfAssemblyLines = _viewModel.Shared.CountOfAssemblyLines;
        _viewModel.Simulation.CountOfWorkersGroupA = _viewModel.Shared.CountOfWorkersGroupA;
        _viewModel.Simulation.CountOfWorkersGroupB = _viewModel.Shared.CountOfWorkersGroupB;
        _viewModel.Simulation.CountOfWorkersGroupC = _viewModel.Shared.CountOfWorkersGroupC;
        _viewModel.Simulation.EnableWorkerLocationPreference = _viewModel.Shared.EnableWorkerLocationPreference;
        
        _latestGUIUpdate = -1;

        _viewModel.SingleReplication.ResetForSimulationStart();
        _viewModel.MultipleReplications.ResetForSimulationStart();

        if (_viewModel.Shared.Replications == 1 &&  !double.IsInfinity(_viewModel.Shared.SpeedOptions.ElementAt(_viewModel.Shared.SelectedSpeedIndex).Key))
        {
            _viewModel.Simulation.SetSimSpeed(_viewModel.SingleReplication.Delta, _viewModel.SingleReplication.SleepTime);
        }
        
        await Task.Run(() => _viewModel.Simulation.Simulate(_viewModel.Shared.Replications, _viewModel.Shared.MaxReplicationTime));
    }

    private void StopSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Simulation.AnimatorExists)
        {
            _viewModel.Shared.DeleteAnimator();
        }
        
        _stopSimulationRequested = true;
        _viewModel.Simulation.StopSimulation();
    }
    
    private void PauseResumeSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.Shared.PauseResumeSimulation();
    
    private void SpeedOneSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetDefaultSpeed();
    
    private void DecreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.DecreaseSpeed();

    private void IncreaseSpeedSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.IncreaseSpeed();
    
    private void SpeedMaxSimulationButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.SetMaxSpeed();
    
    private void DeleteAnimatorButton_OnClick(object? sender, RoutedEventArgs e) => _viewModel.Shared.DeleteAnimator();

    private double _latestGUIUpdate = -1;

    private void SimulationStateChanged(OSPABA.Simulation simulation)
    {
        var mySimulation = (MySimulation)simulation;
        
        var simulationTime = mySimulation.CurrentTime;
        var simulationSpeed = _viewModel.SingleReplication.SimulationSpeed;
        var simulationDelta = _viewModel.SingleReplication.Delta;

        if (!double.IsInfinity(simulationSpeed))
        {
            // Rozdiel posledneho updatu GUI a aktualneho casu simulacie
            var differenceCurrentAndLast = simulationTime - _latestGUIUpdate;

            // Rychlost updatovania GUI je zavisle od delta
            var customParameter = 0.025;
            var minimalDifference = simulationDelta * customParameter;
        
            if (differenceCurrentAndLast < minimalDifference && !_viewModel.Simulation.IsPaused() && _viewModel.Simulation.IsRunning())
            {
                return;
            }
        }
        
        _latestGUIUpdate = simulationTime;

        var orders = _viewModel.Simulation.ManufacturerAgent.AllOrders.Select(o => o.ToDTO(simulationTime)).ToList();
        var pendingOrders = _viewModel.Simulation.ManufacturerAgent.PendingOrders.OriginalQueue.Select(o => o.ToDTO(simulationTime)).ToList();
        var pendingItemsForLine = _viewModel.Simulation.AssemblyLinesAgent.RequestsQueue.OriginalQueue.Select(o => o.Furniture.ToDTO(simulationTime)).ToList();
        var pendingItemsForWorkerA = _viewModel.Simulation.WorkersGroupAAgent.WorkersRequestsQueue.OriginalQueue.UnorderedItems
            .OrderBy(e => e.Element.Furniture.StartedWaitingTime)
            .Select<(MyMessage Element, MyMessage Priority), FurnitureDTO>(e => e.Element.Furniture.ToDTO(simulationTime)).ToList();
        var pendingItemsForWorkerC = _viewModel.Simulation.WorkersGroupCAgent.WorkersRequestsQueue.OriginalQueue.UnorderedItems
            .OrderBy(e => e.Element.Furniture.StartedWaitingTime)
            .Select<(MyMessage Element, MyMessage Priority), FurnitureDTO>(e => e.Element.Furniture.ToDTO(simulationTime)).ToList();
        var pendingItemsForWorkerB = _viewModel.Simulation.WorkersGroupBAgent.WorkersRequestsQueue.OriginalQueue.UnorderedItems
            .OrderBy(e => e.Element.Furniture.StartedWaitingTime)
            .Select<(MyMessage Element, MyMessage Priority), FurnitureDTO>(e => e.Element.Furniture.ToDTO(simulationTime)).ToList();
        var pendingItemsForWorkerMix = _viewModel.Simulation.WorkersAgent.MixedWorkersRequestsQueue.OriginalQueue.UnorderedItems
            .OrderBy(e => e.Element.Furniture.StartedWaitingTime)
            .Select<(MyMessage Element, MyMessage Priority), FurnitureDTO>(e => e.Element.Furniture.ToDTO(simulationTime)).ToList();
        
        var pendingOrdersQueueCount = _viewModel.Simulation.ManufacturerAgent.PendingOrders.Count;
        var pendingItemsForLineQueueCount = _viewModel.Simulation.AssemblyLinesAgent.RequestsQueue.Count;
        var pendingItemsForWorkerAQueueCount = _viewModel.Simulation.WorkersGroupAAgent.WorkersRequestsQueue.Count;
        var pendingItemsForWorkerCQueueCount = _viewModel.Simulation.WorkersGroupCAgent.WorkersRequestsQueue.Count;
        var pendingItemsForWorkerBQueueCount = _viewModel.Simulation.WorkersGroupBAgent.WorkersRequestsQueue.Count;
        var pendingItemsForWorkerMixQueueCount = _viewModel.Simulation.WorkersAgent.MixedWorkersRequestsQueue.Count;
        
        var averageProcessingOrderTime = _viewModel.Simulation.EnvironmentAgent.ProcessingOrderTime.Mean;
        var averageProcessingOrderItemTime = _viewModel.Simulation.ManufacturerAgent.ProcessingFurnitureTime.Mean;
        
        _viewModel.Simulation.ManufacturerAgent.PendingOrders.RefreshStatistics();
        _viewModel.Simulation.AssemblyLinesAgent.RequestsQueue.RefreshStatistics();
        _viewModel.Simulation.WorkersGroupAAgent.WorkersRequestsQueue.RefreshStatistics();
        _viewModel.Simulation.WorkersGroupCAgent.WorkersRequestsQueue.RefreshStatistics();
        _viewModel.Simulation.WorkersGroupBAgent.WorkersRequestsQueue.RefreshStatistics();
        _viewModel.Simulation.WorkersAgent.MixedWorkersRequestsQueue.RefreshStatistics();
        
        var averagePendingOrders = _viewModel.Simulation.ManufacturerAgent.PendingOrders.AverageQueueLength;
        var averagePendingItemsForLine = _viewModel.Simulation.AssemblyLinesAgent.RequestsQueue.AverageQueueLength;
        var averagePendingItemsForWorkerA = _viewModel.Simulation.WorkersGroupAAgent.WorkersRequestsQueue.AverageQueueLength;
        var averagePendingItemsForWorkerC = _viewModel.Simulation.WorkersGroupCAgent.WorkersRequestsQueue.AverageQueueLength;
        var averagePendingItemsForWorkerB = _viewModel.Simulation.WorkersGroupBAgent.WorkersRequestsQueue.AverageQueueLength;
        var averagePendingItemsForWorkerMix = _viewModel.Simulation.WorkersAgent.MixedWorkersRequestsQueue.AverageQueueLength;
        
        var averagePendingOrdersWaitingTime = _viewModel.Simulation.ManufacturerAgent.PendingOrdersWaitingTime.Mean;
        var averagePendingItemsForLineWaitingTime = _viewModel.Simulation.AssemblyLinesAgent.RequestsQueueWaitingTime.Mean;
        var averagePendingItemsForWorkerAWaitingTime = _viewModel.Simulation.WorkersAgent.WorkersARequestsQueueWaitingTime.Mean;
        var averagePendingItemsForWorkerCWaitingTime = _viewModel.Simulation.WorkersAgent.WorkersCRequestsQueueWaitingTime.Mean;
        var averagePendingItemsForWorkerBWaitingTime = _viewModel.Simulation.WorkersAgent.WorkersBRequestsQueueWaitingTime.Mean;
        var averagePendingItemsForWorkerMixWaitingTime = _viewModel.Simulation.WorkersAgent.WorkersMixRequestsQueueWaitingTime.Mean;

        var assemblyLines = _viewModel.Simulation.AssemblyLinesAgent.AssemblyLines.Select(a => a.ToDTO()).ToList();
        var workersGroupA = _viewModel.Simulation.WorkersGroupAAgent.Workers.Select(w => w.ToDTO()).ToList();
        var workersGroupB = _viewModel.Simulation.WorkersGroupBAgent.Workers.Select(w => w.ToDTO()).ToList();
        var workersGroupC = _viewModel.Simulation.WorkersGroupCAgent.Workers.Select(w => w.ToDTO()).ToList();
        
        // Refresh štatistík vyťaženia výrobných liniek
        foreach (AssemblyLine assemblyLine in _viewModel.Simulation.AssemblyLinesAgent.AssemblyLines)
        {
            assemblyLine.RefreshStatistics();
        }
        
        var assemblyLinesUtilization = _viewModel.Simulation.AssemblyLinesAgent.AssemblyLines.Average(assemblyLine => assemblyLine.Utilization) * 100;
        
        // Refresh štatistík vyťaženia pracovníkov
        foreach (Worker worker in _viewModel.Simulation.WorkersGroupAAgent.Workers)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupAUtilization = _viewModel.Simulation.WorkersGroupAAgent.Workers.Average(worker => worker.Utilization) * 100;

        foreach (Worker worker in _viewModel.Simulation.WorkersGroupBAgent.Workers)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupBUtilization = _viewModel.Simulation.WorkersGroupBAgent.Workers.Average(worker => worker.Utilization) * 100;
        
        foreach (Worker worker in _viewModel.Simulation.WorkersGroupBAgent.Workers)
        {
            worker.RefreshStatistics();
        }
        
        var workersGroupCUtilization = _viewModel.Simulation.WorkersGroupCAgent.Workers.Average(worker => worker.Utilization) * 100;
        
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.Shared.CurrentSimulationTime = simulationTime.FormatToSimulationTime();
            _viewModel.SingleReplication.SetReplicationOrderProcessingTime(averageProcessingOrderTime);
            _viewModel.SingleReplication.SetReplicationOrderItemProcessingTime(averageProcessingOrderItemTime);
            
            _viewModel.SingleReplication.ReplicationPendingOrders = $"{averagePendingOrders:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForLine = $"{averagePendingItemsForLine:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForWorkerA = $"{averagePendingItemsForWorkerA:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForWorkerC = $"{averagePendingItemsForWorkerC:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForWorkerB = $"{averagePendingItemsForWorkerB:F2}";
            _viewModel.SingleReplication.ReplicationPendingItemsForWorkerMix = $"{averagePendingItemsForWorkerMix:F2}";
            
            _viewModel.SingleReplication.SetReplicationPendingOrdersWaitingTime(averagePendingOrdersWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForLineWaitingTime(averagePendingItemsForLineWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForWorkerAWaitingTime(averagePendingItemsForWorkerAWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForWorkerCWaitingTime(averagePendingItemsForWorkerCWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForWorkerBWaitingTime(averagePendingItemsForWorkerBWaitingTime);
            _viewModel.SingleReplication.SetReplicationPendingItemsForWorkerMixWaitingTime(averagePendingItemsForWorkerMixWaitingTime);
            
            _viewModel.SingleReplication.PendingOrdersQueueCount = $"{pendingOrdersQueueCount}";
            _viewModel.SingleReplication.PendingItemsForLineQueueCount = $"{pendingItemsForLineQueueCount}";
            _viewModel.SingleReplication.PendingItemsForWorkerAQueueCount = $"{pendingItemsForWorkerAQueueCount}";
            _viewModel.SingleReplication.PendingItemsForWorkerCQueueCount = $"{pendingItemsForWorkerCQueueCount}";
            _viewModel.SingleReplication.PendingItemsForWorkerBQueueCount = $"{pendingItemsForWorkerBQueueCount}";
            _viewModel.SingleReplication.PendingItemsForWorkerMixQueueCount = $"{pendingItemsForWorkerMixQueueCount}";
            
            SynchronizeCollection(_viewModel.SingleReplication.Orders, orders);
            SynchronizeCollection(_viewModel.SingleReplication.PendingOrdersQueue, pendingOrders);
            SynchronizeCollection(_viewModel.SingleReplication.PendingItemsForLineQueue, pendingItemsForLine);
            SynchronizeCollection(_viewModel.SingleReplication.PendingItemsForWorkerAQueue, pendingItemsForWorkerA);
            SynchronizeCollection(_viewModel.SingleReplication.PendingItemsForWorkerCQueue, pendingItemsForWorkerC);
            SynchronizeCollection(_viewModel.SingleReplication.PendingItemsForWorkerBQueue, pendingItemsForWorkerB);
            SynchronizeCollection(_viewModel.SingleReplication.PendingItemsForWorkerMixQueue, pendingItemsForWorkerMix);
            
            // Ak je selectnuta objednavka v GUI, tak aktualizujeme aj jej polozky
            if (_viewModel.SingleReplication.SelectedOrder != null)
            {
                var selectedOrder = _viewModel.SingleReplication.Orders.FirstOrDefault(o => o.Id == _viewModel.SingleReplication.SelectedOrder.Id);
                SynchronizeCollection(_viewModel.SingleReplication.SelectedOrderFurnitureItems, selectedOrder.FurnitureItems);
            }
            
            _viewModel.SingleReplication.ReplicationAssemblyLinesUtilization = $"{assemblyLinesUtilization:F2}";
            
            _viewModel.SingleReplication.ReplicationWorkersGroupAUtilization = $"{workersGroupAUtilization:F2}";
            _viewModel.SingleReplication.ReplicationWorkersGroupBUtilization = $"{workersGroupBUtilization:F2}";
            _viewModel.SingleReplication.ReplicationWorkersGroupCUtilization = $"{workersGroupCUtilization:F2}";
            
            SynchronizeCollection(_viewModel.SingleReplication.AssemblyLines, assemblyLines);
            SynchronizeCollection(_viewModel.SingleReplication.WorkersGroupA, workersGroupA);
            SynchronizeCollection(_viewModel.SingleReplication.WorkersGroupB, workersGroupB);
            SynchronizeCollection(_viewModel.SingleReplication.WorkersGroupC, workersGroupC);
        });
    }
    
    private void ReplicationEnded(OSPABA.Simulation simulation)
    {
        var mySimulation = (MySimulation)simulation;
        
        var currentReplication = mySimulation.CurrentReplication + 1;
        
        var averageProcessingOrderTime = _viewModel.Simulation.AverageProcessingOrderTime.Mean;
        var averageProcessingOrderTimeCI = _viewModel.Simulation.AverageProcessingOrderTime.ConfidenceInterval95();
        var averageProcessingOrderItemTime = _viewModel.Simulation.AverageProcessingFurnitureTime.Mean;
        var averageProcessingOrderItemTimeCI = _viewModel.Simulation.AverageProcessingFurnitureTime.ConfidenceInterval95();
        
        var averagePendingOrdersCount = _viewModel.Simulation.AveragePendingOrdersCount.Mean;
        var averagePendingOrdersCountCI = _viewModel.Simulation.AveragePendingOrdersCount.ConfidenceInterval95();
        var averagePendingItemsForLineCount = _viewModel.Simulation.AveragePendingItemsForLineCount.Mean;
        var averagePendingItemsForLineCountCI = _viewModel.Simulation.AveragePendingItemsForLineCount.ConfidenceInterval95();
        var averagePendingItemsForWorkerACount = _viewModel.Simulation.AveragePendingItemsForWorkerACount.Mean;
        var averagePendingItemsForWorkerACountCI = _viewModel.Simulation.AveragePendingItemsForWorkerACount.ConfidenceInterval95();
        var averagePendingItemsForWorkerCCount = _viewModel.Simulation.AveragePendingItemsForWorkerCCount.Mean;
        var averagePendingItemsForWorkerCCountCI = _viewModel.Simulation.AveragePendingItemsForWorkerCCount.ConfidenceInterval95();
        var averagePendingItemsForWorkerBCount = _viewModel.Simulation.AveragePendingItemsForWorkerBCount.Mean;
        var averagePendingItemsForWorkerBCountCI = _viewModel.Simulation.AveragePendingItemsForWorkerBCount.ConfidenceInterval95();
        var averagePendingItemsForWorkerMixCount = _viewModel.Simulation.AveragePendingItemsForWorkerMixCount.Mean;
        var averagePendingItemsForWorkerMixCountCI = _viewModel.Simulation.AveragePendingItemsForWorkerMixCount.ConfidenceInterval95();
        
        var averagePendingOrdersWaitingTime = _viewModel.Simulation.AveragePendingOrdersWaitingTime.Mean;
        var averagePendingOrdersWaitingTimeCI = _viewModel.Simulation.AveragePendingOrdersWaitingTime.ConfidenceInterval95();
        var averagePendingItemsForLineWaitingTime = _viewModel.Simulation.AveragePendingItemsForLineWaitingTime.Mean;
        var averagePendingItemsForLineWaitingTimeCI = _viewModel.Simulation.AveragePendingItemsForLineWaitingTime.ConfidenceInterval95();
        var averagePendingItemsForWorkerAWaitingTime = _viewModel.Simulation.AveragePendingItemsForWorkerAWaitingTime.Mean;
        var averagePendingItemsForWorkerAWaitingTimeCI = _viewModel.Simulation.AveragePendingItemsForWorkerAWaitingTime.ConfidenceInterval95();
        var averagePendingItemsForWorkerCWaitingTime = _viewModel.Simulation.AveragePendingItemsForWorkerCWaitingTime.Mean;
        var averagePendingItemsForWorkerCWaitingTimeCI = _viewModel.Simulation.AveragePendingItemsForWorkerCWaitingTime.ConfidenceInterval95();
        var averagePendingItemsForWorkerBWaitingTime = _viewModel.Simulation.AveragePendingItemsForWorkerBWaitingTime.Mean;
        var averagePendingItemsForWorkerBWaitingTimeCI = _viewModel.Simulation.AveragePendingItemsForWorkerBWaitingTime.ConfidenceInterval95();
        var averagePendingItemsForWorkerMixWaitingTime = _viewModel.Simulation.AveragePendingItemsForWorkerMixWaitingTime.Mean;
        var averagePendingItemsForWorkerMixWaitingTimeCI = _viewModel.Simulation.AveragePendingItemsForWorkerMixWaitingTime.ConfidenceInterval95();
        
        var averageAssemblyLinesUtilization = _viewModel.Simulation.AverageAssemblyLinesUtilization.Mean;
        var averageAssemblyLinesUtilizationCI = _viewModel.Simulation.AverageAssemblyLinesUtilization.ConfidenceInterval95();
        
        var averageWorkersGroupAUtilization = _viewModel.Simulation.AverageWorkersGroupAUtilization.Mean;
        var averageWorkersGroupAUtilizationCI = _viewModel.Simulation.AverageWorkersGroupAUtilization.ConfidenceInterval95();
        var averageWorkersGroupBUtilization = _viewModel.Simulation.AverageWorkersGroupBUtilization.Mean;
        var averageWorkersGroupBUtilizationCI = _viewModel.Simulation.AverageWorkersGroupBUtilization.ConfidenceInterval95();
        var averageWorkersGroupCUtilization = _viewModel.Simulation.AverageWorkersGroupCUtilization.Mean;
        var averageWorkersGroupCUtilizationCI = _viewModel.Simulation.AverageWorkersGroupCUtilization.ConfidenceInterval95();
        
        var allAssemblyLinesUtilization = new List<AssemblyLineDTO>();
        
        for (int i = 0; i < _viewModel.Simulation.AverageAllAssemblyLinesUtilization.Length; i++)
        {
            var utilization = _viewModel.Simulation.AverageAllAssemblyLinesUtilization[i].ConfidenceInterval95();
            var mean = _viewModel.Simulation.AverageAllAssemblyLinesUtilization[i].Mean;
            
            var assemblyLine = _viewModel.Simulation.AssemblyLinesAgent.AssemblyLines[i].ToUtilizationDTO(mean, utilization);
            
            allAssemblyLinesUtilization.Add(assemblyLine);
        }
        
        var allWorkersUtilization = new List<WorkerDTO>();
        
        var workersGroupA = _viewModel.Simulation.WorkersGroupAAgent.Workers;
        var workersGroupB = _viewModel.Simulation.WorkersGroupBAgent.Workers;
        var workersGroupC = _viewModel.Simulation.WorkersGroupCAgent.Workers;
        
        for (int i = 0; i < _viewModel.Simulation.AverageAllWorkersUtilization.Length; i++)
        {
            var utilization = _viewModel.Simulation.AverageAllWorkersUtilization[i].ConfidenceInterval95();
            var mean = _viewModel.Simulation.AverageAllWorkersUtilization[i].Mean;
            
            WorkerDTO? worker;
            
            if (i < workersGroupA.Length)
            {
                worker = workersGroupA[i].ToUtilizationDTO(mean, utilization);
            }
            else if (i < workersGroupA.Length + workersGroupB.Length)
            {
                worker = workersGroupB[i - workersGroupA.Length].ToUtilizationDTO(mean, utilization);
            }
            else
            {
                worker = workersGroupC[i - workersGroupA.Length - workersGroupB.Length].ToUtilizationDTO(mean, utilization);
            }
            
            allWorkersUtilization.Add(worker);
        }

        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.MultipleReplications.CurrentReplication = $"{currentReplication}";
            _viewModel.MultipleReplications.SetSimulationCurrentProcessingOrderTime(averageProcessingOrderTime, averageProcessingOrderTimeCI.Item1, averageProcessingOrderTimeCI.Item2);
            _viewModel.MultipleReplications.SetSimulationCurrentProcessingOrderItemTime(averageProcessingOrderItemTime, averageProcessingOrderItemTimeCI.Item1, averageProcessingOrderItemTimeCI.Item2);
            
            _viewModel.MultipleReplications.SimulationPendingOrders = $"{averagePendingOrdersCount:F2}";
            _viewModel.MultipleReplications.SimulationPendingOrdersCI = $"<{averagePendingOrdersCountCI.Item1:F2} ; {averagePendingOrdersCountCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForLine = $"{averagePendingItemsForLineCount:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForLineCI = $"<{averagePendingItemsForLineCountCI.Item1:F2} ; {averagePendingItemsForLineCountCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerA = $"{averagePendingItemsForWorkerACount:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerACI = $"<{averagePendingItemsForWorkerACountCI.Item1:F2} ; {averagePendingItemsForWorkerACountCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerC = $"{averagePendingItemsForWorkerCCount:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerCCI = $"<{averagePendingItemsForWorkerCCountCI.Item1:F2} ; {averagePendingItemsForWorkerCCountCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerB = $"{averagePendingItemsForWorkerBCount:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerBCI = $"<{averagePendingItemsForWorkerBCountCI.Item1:F2} ; {averagePendingItemsForWorkerBCountCI.Item2:F2}>";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerMix = $"{averagePendingItemsForWorkerMixCount:F2}";
            _viewModel.MultipleReplications.SimulationPendingItemsForWorkerMixCI = $"<{averagePendingItemsForWorkerMixCountCI.Item1:F2} ; {averagePendingItemsForWorkerMixCountCI.Item2:F2}>";
            
            _viewModel.MultipleReplications.SetSimulationPendingOrdersWaitingTime(
                averagePendingOrdersWaitingTime, 
                averagePendingOrdersWaitingTimeCI.Item1, 
                averagePendingOrdersWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationPendingItemsForLineWaitingTime(
                averagePendingItemsForLineWaitingTime, 
                averagePendingItemsForLineWaitingTimeCI.Item1, 
                averagePendingItemsForLineWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationPendingItemsForWorkerAWaitingTime(
                averagePendingItemsForWorkerAWaitingTime, 
                averagePendingItemsForWorkerAWaitingTimeCI.Item1, 
                averagePendingItemsForWorkerAWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationPendingItemsForWorkerCWaitingTime(
                averagePendingItemsForWorkerCWaitingTime, 
                averagePendingItemsForWorkerCWaitingTimeCI.Item1, 
                averagePendingItemsForWorkerCWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationPendingItemsForWorkerBWaitingTime(
                averagePendingItemsForWorkerBWaitingTime, 
                averagePendingItemsForWorkerBWaitingTimeCI.Item1, 
                averagePendingItemsForWorkerBWaitingTimeCI.Item2
            );
            _viewModel.MultipleReplications.SetSimulationPendingItemsForWorkerMixWaitingTime(
                averagePendingItemsForWorkerMixWaitingTime, 
                averagePendingItemsForWorkerMixWaitingTimeCI.Item1, 
                averagePendingItemsForWorkerMixWaitingTimeCI.Item2
            );
            
            _viewModel.MultipleReplications.SimulationAssemblyLinesUtilization = $"{averageAssemblyLinesUtilization * 100:F2}";
            _viewModel.MultipleReplications.SimulationAssemblyLinesUtilizationCI = $"<{(averageAssemblyLinesUtilizationCI.Item1*100):F2} ; {(averageAssemblyLinesUtilizationCI.Item2*100):F2}>";
            
            _viewModel.MultipleReplications.SimulationWorkersAUtilization = $"{averageWorkersGroupAUtilization * 100:F2}";
            _viewModel.MultipleReplications.SimulationWorkersAUtilizationCI = $"<{(averageWorkersGroupAUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupAUtilizationCI.Item2*100):F2}>";
            _viewModel.MultipleReplications.SimulationWorkersBUtilization = $"{averageWorkersGroupBUtilization * 100:F2}";
            _viewModel.MultipleReplications.SimulationWorkersBUtilizationCI = $"<{(averageWorkersGroupBUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupBUtilizationCI.Item2*100):F2}>";
            _viewModel.MultipleReplications.SimulationWorkersCUtilization = $"{averageWorkersGroupCUtilization * 100:F2}";
            _viewModel.MultipleReplications.SimulationWorkersCUtilizationCI = $"<{(averageWorkersGroupCUtilizationCI.Item1*100):F2} ; {(averageWorkersGroupCUtilizationCI.Item2*100):F2}>";
            
            for (int i = 0; i < allAssemblyLinesUtilization.Count; i++)
            {
                _viewModel.MultipleReplications.SimulationAllAssemblyLinesUtilization[i].Id = allAssemblyLinesUtilization[i].Id;
                _viewModel.MultipleReplications.SimulationAllAssemblyLinesUtilization[i].State = allAssemblyLinesUtilization[i].State;
                _viewModel.MultipleReplications.SimulationAllAssemblyLinesUtilization[i].Utilization = allAssemblyLinesUtilization[i].Utilization;
            }
            
            for (int i = 0; i < allWorkersUtilization.Count; i++)
            {
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].Id = allWorkersUtilization[i].Id;
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].State = allWorkersUtilization[i].State;
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].Utilization = allWorkersUtilization[i].Utilization;
                _viewModel.MultipleReplications.SimulationAllWorkersUtilization[i].Place = allWorkersUtilization[i].Place;
            }
        });
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
}
