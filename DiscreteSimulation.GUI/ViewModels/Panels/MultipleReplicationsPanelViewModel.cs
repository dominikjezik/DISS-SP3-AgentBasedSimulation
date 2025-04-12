using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.DTOs;

namespace DiscreteSimulation.GUI.ViewModels.Panels;

public class MultipleReplicationsPanelViewModel : ViewModelBase
{
    public SharedViewModel Shared { get; private set; }
    
    public MultipleReplicationsPanelViewModel(SharedViewModel shared)
    {
        Shared = shared;
        Shared.PropertyChanged += OnSharedPropertyChanged;
    }

    private void OnSharedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Shared.Replications))
        {
            OnPropertyChanged(nameof(RenderOffset));
            
            if (Shared.Replications < RenderPoints)
            {
                RenderPoints = Shared.Replications;
            }
        }
        else if (e.PropertyName == nameof(Shared.SelectedTimeUnits))
        {
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
            OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationItemsForStainingWaitingTime));
            OnPropertyChanged(nameof(SimulationItemsForStainingWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationItemsForFoldingWaitingTime));
            OnPropertyChanged(nameof(SimulationItemsForFoldingWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationItemsForFittingsWaitingTime));
            OnPropertyChanged(nameof(SimulationItemsForFittingsWaitingTimeCI));
        }
    }

    public void ResetForSimulationStart()
    {
        SimulationAllWorkersUtilization = new ObservableCollection<WorkerDTO>();
        
        for (int i = 0; i < Shared.Simulation.CountOfWorkersGroupA + Shared.Simulation.CountOfWorkersGroupB + Shared.Simulation.CountOfWorkersGroupC; i++)
        {
            SimulationAllWorkersUtilization.Add(new WorkerDTO());
        }
    }
    
    private string _currentReplication = "_";

    public string CurrentReplication
    {
        get => _currentReplication;
        set
        {
            _currentReplication = value;
            OnPropertyChanged();
        }
    }
    
    private long _skipFirstNReplicationsInPercent = 0;
    
    public long SkipFirstNReplicationsInPercent
    {
        get => _skipFirstNReplicationsInPercent;
        set
        {
            if (value == _skipFirstNReplicationsInPercent) return;
            _skipFirstNReplicationsInPercent = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
        }
    }

    private long _renderPoints = 1000;

    public long RenderPoints
    {
        get => _renderPoints;
        set
        {
            _renderPoints = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
        }
    }

    public long RenderOffset
    {
        get
        {
            var skipFirst = Math.Floor(Shared.Replications * (SkipFirstNReplicationsInPercent / 100.0));
            var renderOffset = (Shared.Replications - skipFirst) / RenderPoints;
            var result = Convert.ToInt64(Math.Floor(renderOffset)) - 1;
            return result >= 0 ? result : 0;
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeSeconds = "-";
    private string _simulationCurrentProcessingOrderTimeMinutes = "-";
    private string _simulationCurrentProcessingOrderTimeHours = "-";
    
    public string SimulationCurrentProcessingOrderTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationCurrentProcessingOrderTimeSeconds,
                "minutes" => _simulationCurrentProcessingOrderTimeMinutes,
                "hours" => _simulationCurrentProcessingOrderTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationCurrentProcessingOrderTimeSecondsCI = "-";
    private string _simulationCurrentProcessingOrderTimeMinutesCI = "-";
    private string _simulationCurrentProcessingOrderTimeHoursCI = "-";
    
    public string SimulationCurrentProcessingOrderTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationCurrentProcessingOrderTimeSecondsCI,
                "minutes" => _simulationCurrentProcessingOrderTimeMinutesCI,
                "hours" => _simulationCurrentProcessingOrderTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationCurrentProcessingOrderTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationCurrentProcessingOrderTimeSeconds = $"{averageTime:F2}";
        _simulationCurrentProcessingOrderTimeMinutes = $"{averageTime / 60:F2}";
        _simulationCurrentProcessingOrderTimeHours = $"{averageTime / 3600:F2}";
        
        _simulationCurrentProcessingOrderTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationCurrentProcessingOrderTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationCurrentProcessingOrderTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTime));
        OnPropertyChanged(nameof(SimulationCurrentProcessingOrderTimeCI));
    }
    
    private string _simulationPendingOrders = "-";
    
    public string SimulationPendingOrders
    {
        get => _simulationPendingOrders;
        set
        {
            _simulationPendingOrders = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingOrdersCI = "-";
    
    public string SimulationPendingOrdersCI
    {
        get => _simulationPendingOrdersCI;
        set
        {
            _simulationPendingOrdersCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingFurnitureItems = "-";
    
    public string SimulationPendingFurnitureItems
    {
        get => _simulationPendingFurnitureItems;
        set
        {
            _simulationPendingFurnitureItems = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingFurnitureItemsCI = "-";
    
    public string SimulationPendingFurnitureItemsCI
    {
        get => _simulationPendingFurnitureItemsCI;
        set
        {
            _simulationPendingFurnitureItemsCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForStaining = "-";
    
    public string SimulationPendingItemsForStaining
    {
        get => _simulationPendingItemsForStaining;
        set
        {
            _simulationPendingItemsForStaining = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForVarnishing = "-";
    
    public string SimulationPendingItemsForVarnishing
    {
        get => _simulationPendingItemsForVarnishing;
        set
        {
            _simulationPendingItemsForVarnishing = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForVarnishingCI = "-";
    
    public string SimulationPendingItemsForVarnishingCI
    {
        get => _simulationPendingItemsForVarnishingCI;
        set
        {
            _simulationPendingItemsForVarnishingCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForStainingCI = "-";
    
    public string SimulationPendingItemsForStainingCI
    {
        get => _simulationPendingItemsForStainingCI;
        set
        {
            _simulationPendingItemsForStainingCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForFolding = "-";
    
    public string SimulationPendingItemsForFolding
    {
        get => _simulationPendingItemsForFolding;
        set
        {
            _simulationPendingItemsForFolding = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForFoldingCI = "-";
    
    public string SimulationPendingItemsForFoldingCI
    {
        get => _simulationPendingItemsForFoldingCI;
        set
        {
            _simulationPendingItemsForFoldingCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForFittings = "-";
    
    public string SimulationPendingItemsForFittings
    {
        get => _simulationPendingItemsForFittings;
        set
        {
            _simulationPendingItemsForFittings = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForFittingsCI = "-";
    
    public string SimulationPendingItemsForFittingsCI
    {
        get => _simulationPendingItemsForFittingsCI;
        set
        {
            _simulationPendingItemsForFittingsCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingOrdersWaitingTimeSeconds = "-";
    private string _simulationPendingOrdersWaitingTimeMinutes = "-";
    private string _simulationPendingOrdersWaitingTimeHours = "-";
    
    public string SimulationPendingOrdersWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingOrdersWaitingTimeSeconds,
                "minutes" => _simulationPendingOrdersWaitingTimeMinutes,
                "hours" => _simulationPendingOrdersWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationPendingOrdersWaitingTimeSecondsCI = "-";
    private string _simulationPendingOrdersWaitingTimeMinutesCI = "-";
    private string _simulationPendingOrdersWaitingTimeHoursCI = "-";
    
    public string SimulationPendingOrdersWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingOrdersWaitingTimeSecondsCI,
                "minutes" => _simulationPendingOrdersWaitingTimeMinutesCI,
                "hours" => _simulationPendingOrdersWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationPendingOrdersWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingOrdersWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingOrdersWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingOrdersWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationPendingOrdersWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingOrdersWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingOrdersWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTimeCI));
    }
    
    private string _simulationPendingFurnitureItemsWaitingTimeSeconds = "-";
    private string _simulationPendingFurnitureItemsWaitingTimeMinutes = "-";
    private string _simulationPendingFurnitureItemsWaitingTimeHours = "-";
    
    public string SimulationPendingFurnitureItemsWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingFurnitureItemsWaitingTimeSeconds,
                "minutes" => _simulationPendingFurnitureItemsWaitingTimeMinutes,
                "hours" => _simulationPendingFurnitureItemsWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationPendingFurnitureItemsWaitingTimeSecondsCI = "-";
    private string _simulationPendingFurnitureItemsWaitingTimeMinutesCI = "-";
    private string _simulationPendingFurnitureItemsWaitingTimeHoursCI = "-";
    
    public string SimulationPendingFurnitureItemsWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingFurnitureItemsWaitingTimeSecondsCI,
                "minutes" => _simulationPendingFurnitureItemsWaitingTimeMinutesCI,
                "hours" => _simulationPendingFurnitureItemsWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationPendingFurnitureItemsWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingFurnitureItemsWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingFurnitureItemsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingFurnitureItemsWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationPendingFurnitureItemsWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingFurnitureItemsWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingFurnitureItemsWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingFurnitureItemsWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingFurnitureItemsWaitingTimeCI));
    }
    
    
    private string _simulationItemsForStainingWaitingTimeSeconds = "-";
    private string _simulationItemsForStainingWaitingTimeMinutes = "-";
    private string _simulationItemsForStainingWaitingTimeHours = "-";
    
    public string SimulationItemsForStainingWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForStainingWaitingTimeSeconds,
                "minutes" => _simulationItemsForStainingWaitingTimeMinutes,
                "hours" => _simulationItemsForStainingWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationItemsForStainingWaitingTimeSecondsCI = "-";
    private string _simulationItemsForStainingWaitingTimeMinutesCI = "-";
    private string _simulationItemsForStainingWaitingTimeHoursCI = "-";
    
    public string SimulationItemsForStainingWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForStainingWaitingTimeSecondsCI,
                "minutes" => _simulationItemsForStainingWaitingTimeMinutesCI,
                "hours" => _simulationItemsForStainingWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationItemsForStainingWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationItemsForStainingWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationItemsForStainingWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationItemsForStainingWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationItemsForStainingWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationItemsForStainingWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationItemsForStainingWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationItemsForStainingWaitingTime));
        OnPropertyChanged(nameof(SimulationItemsForStainingWaitingTimeCI));
    }
    
    
    private string _simulationItemsForVarnishingWaitingTimeSeconds = "-";
    private string _simulationItemsForVarnishingWaitingTimeMinutes = "-";
    private string _simulationItemsForVarnishingWaitingTimeHours = "-";
    
    public string SimulationItemsForVarnishingWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForVarnishingWaitingTimeSeconds,
                "minutes" => _simulationItemsForVarnishingWaitingTimeMinutes,
                "hours" => _simulationItemsForVarnishingWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationItemsForVarnishingWaitingTimeSecondsCI = "-";
    private string _simulationItemsForVarnishingWaitingTimeMinutesCI = "-";
    private string _simulationItemsForVarnishingWaitingTimeHoursCI = "-";
    
    public string SimulationItemsForVarnishingWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForVarnishingWaitingTimeSecondsCI,
                "minutes" => _simulationItemsForVarnishingWaitingTimeMinutesCI,
                "hours" => _simulationItemsForVarnishingWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationItemsForVarnishingWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationItemsForVarnishingWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationItemsForVarnishingWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationItemsForVarnishingWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationItemsForVarnishingWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationItemsForVarnishingWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationItemsForVarnishingWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationItemsForVarnishingWaitingTime));
        OnPropertyChanged(nameof(SimulationItemsForVarnishingWaitingTimeCI));
    }
    
    
    private string _simulationItemsForFoldingWaitingTimeSeconds = "-";
    private string _simulationItemsForFoldingWaitingTimeMinutes = "-";
    private string _simulationItemsForFoldingWaitingTimeHours = "-";

    public string SimulationItemsForFoldingWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForFoldingWaitingTimeSeconds,
                "minutes" => _simulationItemsForFoldingWaitingTimeMinutes,
                "hours" => _simulationItemsForFoldingWaitingTimeHours,
                _ => "-"
            };
        }
    }

    private string _simulationItemsForFoldingWaitingTimeSecondsCI = "-";
    private string _simulationItemsForFoldingWaitingTimeMinutesCI = "-";
    private string _simulationItemsForFoldingWaitingTimeHoursCI = "-";

    public string SimulationItemsForFoldingWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForFoldingWaitingTimeSecondsCI,
                "minutes" => _simulationItemsForFoldingWaitingTimeMinutesCI,
                "hours" => _simulationItemsForFoldingWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationItemsForFoldingWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationItemsForFoldingWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationItemsForFoldingWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationItemsForFoldingWaitingTimeHours = $"{averageTime / 3600:F2}";
            
        _simulationItemsForFoldingWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationItemsForFoldingWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationItemsForFoldingWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationItemsForFoldingWaitingTime));
        OnPropertyChanged(nameof(SimulationItemsForFoldingWaitingTimeCI));
    }
    
    
    private string _simulationItemsForFittingsWaitingTimeSeconds = "-";
    private string _simulationItemsForFittingsWaitingTimeMinutes = "-";
    private string _simulationItemsForFittingsWaitingTimeHours = "-";

    public string SimulationItemsForFittingsWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForFittingsWaitingTimeSeconds,
                "minutes" => _simulationItemsForFittingsWaitingTimeMinutes,
                "hours" => _simulationItemsForFittingsWaitingTimeHours,
                _ => "-"
            };
        }
    }

    private string _simulationItemsForFittingsWaitingTimeSecondsCI = "-";
    private string _simulationItemsForFittingsWaitingTimeMinutesCI = "-";
    private string _simulationItemsForFittingsWaitingTimeHoursCI = "-";

    public string SimulationItemsForFittingsWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationItemsForFittingsWaitingTimeSecondsCI,
                "minutes" => _simulationItemsForFittingsWaitingTimeMinutesCI,
                "hours" => _simulationItemsForFittingsWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationItemsForFittingsWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationItemsForFittingsWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationItemsForFittingsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationItemsForFittingsWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        _simulationItemsForFittingsWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationItemsForFittingsWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationItemsForFittingsWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationItemsForFittingsWaitingTime));
        OnPropertyChanged(nameof(SimulationItemsForFittingsWaitingTimeCI));
    }
    
    
    private string _simulationWorkersAUtilization = "-";
    
    public string SimulationWorkersAUtilization
    {
        get => _simulationWorkersAUtilization;
        set
        {
            _simulationWorkersAUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersAUtilizationCI = "-";
    
    public string SimulationWorkersAUtilizationCI
    {
        get => _simulationWorkersAUtilizationCI;
        set
        {
            _simulationWorkersAUtilizationCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersBUtilization = "-";
    
    public string SimulationWorkersBUtilization
    {
        get => _simulationWorkersBUtilization;
        set
        {
            _simulationWorkersBUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersBUtilizationCI = "-";
    
    public string SimulationWorkersBUtilizationCI
    {
        get => _simulationWorkersBUtilizationCI;
        set
        {
            _simulationWorkersBUtilizationCI = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersCUtilization = "-";
    
    public string SimulationWorkersCUtilization
    {
        get => _simulationWorkersBUtilization;
        set
        {
            _simulationWorkersBUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationWorkersCUtilizationCI = "-";
    
    public string SimulationWorkersCUtilizationCI
    {
        get => _simulationWorkersCUtilizationCI;
        set
        {
            _simulationWorkersCUtilizationCI = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<WorkerDTO> _simulationAllWorkersUtilization = [];
    
    public ObservableCollection<WorkerDTO> SimulationAllWorkersUtilization
    {
        get => _simulationAllWorkersUtilization;
        set {
            _simulationAllWorkersUtilization = value;
            OnPropertyChanged();
        }
    }
}
