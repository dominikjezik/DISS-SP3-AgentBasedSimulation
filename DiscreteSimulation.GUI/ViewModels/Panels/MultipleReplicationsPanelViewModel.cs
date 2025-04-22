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
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderItemTime));
            OnPropertyChanged(nameof(SimulationCurrentProcessingOrderItemTimeCI));
            OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingOrdersWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationPendingItemsForLineWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingItemsForLineWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerCWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerCWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerAWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerAWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerBWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerBWaitingTimeCI));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerMixWaitingTime));
            OnPropertyChanged(nameof(SimulationPendingItemsForWorkerMixWaitingTimeCI));
        }
    }

    public void ResetForSimulationStart()
    {
        SimulationAllAssemblyLinesUtilization = new ObservableCollection<AssemblyLineDTO>();
        
        for (int i = 0; i < Shared.CountOfAssemblyLines; i++)
        {
            SimulationAllAssemblyLinesUtilization.Add(new AssemblyLineDTO());
        }
        
        SimulationAllWorkersUtilization = new ObservableCollection<WorkerDTO>();

        for (int i = 0; i < Shared.CountOfWorkersGroupA + Shared.CountOfWorkersGroupB + Shared.CountOfWorkersGroupC; i++)
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
    
    private string _simulationCurrentProcessingOrderItemTimeSeconds = "-";
    private string _simulationCurrentProcessingOrderItemTimeMinutes = "-";
    private string _simulationCurrentProcessingOrderItemTimeHours = "-";
    
    public string SimulationCurrentProcessingOrderItemTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationCurrentProcessingOrderItemTimeSeconds,
                "minutes" => _simulationCurrentProcessingOrderItemTimeMinutes,
                "hours" => _simulationCurrentProcessingOrderItemTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationCurrentProcessingOrderItemTimeSecondsCI = "-";
    private string _simulationCurrentProcessingOrderItemTimeMinutesCI = "-";
    private string _simulationCurrentProcessingOrderItemTimeHoursCI = "-";
    
    public string SimulationCurrentProcessingOrderItemTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationCurrentProcessingOrderItemTimeSecondsCI,
                "minutes" => _simulationCurrentProcessingOrderItemTimeMinutesCI,
                "hours" => _simulationCurrentProcessingOrderItemTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationCurrentProcessingOrderItemTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationCurrentProcessingOrderItemTimeSeconds = $"{averageTime:F2}";
        _simulationCurrentProcessingOrderItemTimeMinutes = $"{averageTime / 60:F2}";
        _simulationCurrentProcessingOrderItemTimeHours = $"{averageTime / 3600:F2}";
        
        _simulationCurrentProcessingOrderItemTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationCurrentProcessingOrderItemTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationCurrentProcessingOrderItemTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationCurrentProcessingOrderItemTime));
        OnPropertyChanged(nameof(SimulationCurrentProcessingOrderItemTimeCI));
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
    
    private string _simulationPendingItemsForLine = "-";
    
    public string SimulationPendingItemsForLine
    {
        get => _simulationPendingItemsForLine;
        set
        {
            _simulationPendingItemsForLine = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForLineCi = "-";
    
    public string SimulationPendingItemsForLineCI
    {
        get => _simulationPendingItemsForLineCi;
        set
        {
            _simulationPendingItemsForLineCi = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerA = "-";
    
    public string SimulationPendingItemsForWorkerA
    {
        get => _simulationPendingItemsForWorkerA;
        set
        {
            _simulationPendingItemsForWorkerA = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerAci = "-";
    
    public string SimulationPendingItemsForWorkerACI
    {
        get => _simulationPendingItemsForWorkerAci;
        set
        {
            _simulationPendingItemsForWorkerAci = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerC = "-";
    
    public string SimulationPendingItemsForWorkerC
    {
        get => _simulationPendingItemsForWorkerC;
        set
        {
            _simulationPendingItemsForWorkerC = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerCci = "-";
    
    public string SimulationPendingItemsForWorkerCCI
    {
        get => _simulationPendingItemsForWorkerCci;
        set
        {
            _simulationPendingItemsForWorkerCci = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerB = "-";
    
    public string SimulationPendingItemsForWorkerB
    {
        get => _simulationPendingItemsForWorkerB;
        set
        {
            _simulationPendingItemsForWorkerB = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerBci = "-";
    
    public string SimulationPendingItemsForWorkerBCI
    {
        get => _simulationPendingItemsForWorkerBci;
        set
        {
            _simulationPendingItemsForWorkerBci = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerMix = "-";
    
    public string SimulationPendingItemsForWorkerMix
    {
        get => _simulationPendingItemsForWorkerMix;
        set
        {
            _simulationPendingItemsForWorkerMix = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationPendingItemsForWorkerMixCi = "-";
    
    public string SimulationPendingItemsForWorkerMixCI
    {
        get => _simulationPendingItemsForWorkerMixCi;
        set
        {
            _simulationPendingItemsForWorkerMixCi = value;
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
    
    private string _simulationPendingItemsForLineWaitingTimeSeconds = "-";
    private string _simulationPendingItemsForLineWaitingTimeMinutes = "-";
    private string _simulationPendingItemsForLineWaitingTimeHours = "-";
    
    public string SimulationPendingItemsForLineWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForLineWaitingTimeSeconds,
                "minutes" => _simulationPendingItemsForLineWaitingTimeMinutes,
                "hours" => _simulationPendingItemsForLineWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationPendingItemsForLineWaitingTimeSecondsCI = "-";
    private string _simulationPendingItemsForLineWaitingTimeMinutesCI = "-";
    private string _simulationPendingItemsForLineWaitingTimeHoursCI = "-";
    
    public string SimulationPendingItemsForLineWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForLineWaitingTimeSecondsCI,
                "minutes" => _simulationPendingItemsForLineWaitingTimeMinutesCI,
                "hours" => _simulationPendingItemsForLineWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationPendingItemsForLineWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingItemsForLineWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingItemsForLineWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingItemsForLineWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationPendingItemsForLineWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingItemsForLineWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingItemsForLineWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingItemsForLineWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingItemsForLineWaitingTimeCI));
    }
    
    
    private string _simulationPendingItemsForWorkerAWaitingTimeSeconds = "-";
    private string _simulationPendingItemsForWorkerAWaitingTimeMinutes = "-";
    private string _simulationPendingItemsForWorkerAWaitingTimeHours = "-";
    
    public string SimulationPendingItemsForWorkerAWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerAWaitingTimeSeconds,
                "minutes" => _simulationPendingItemsForWorkerAWaitingTimeMinutes,
                "hours" => _simulationPendingItemsForWorkerAWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationPendingItemsForWorkerAWaitingTimeSecondsCI = "-";
    private string _simulationPendingItemsForWorkerAWaitingTimeMinutesCI = "-";
    private string _simulationPendingItemsForWorkerAWaitingTimeHoursCI = "-";
    
    public string SimulationPendingItemsForWorkerAWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerAWaitingTimeSecondsCI,
                "minutes" => _simulationPendingItemsForWorkerAWaitingTimeMinutesCI,
                "hours" => _simulationPendingItemsForWorkerAWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationPendingItemsForWorkerAWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingItemsForWorkerAWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingItemsForWorkerAWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingItemsForWorkerAWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationPendingItemsForWorkerAWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingItemsForWorkerAWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingItemsForWorkerAWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerAWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerAWaitingTimeCI));
    }
    
    
    private string _simulationPendingItemsForWorkerCWaitingTimeSeconds = "-";
    private string _simulationPendingItemsForWorkerCWaitingTimeMinutes = "-";
    private string _simulationPendingItemsForWorkerCWaitingTimeHours = "-";
    
    public string SimulationPendingItemsForWorkerCWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerCWaitingTimeSeconds,
                "minutes" => _simulationPendingItemsForWorkerCWaitingTimeMinutes,
                "hours" => _simulationPendingItemsForWorkerCWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    private string _simulationPendingItemsForWorkerCWaitingTimeSecondsCI = "-";
    private string _simulationPendingItemsForWorkerCWaitingTimeMinutesCI = "-";
    private string _simulationPendingItemsForWorkerCWaitingTimeHoursCI = "-";
    
    public string SimulationPendingItemsForWorkerCWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerCWaitingTimeSecondsCI,
                "minutes" => _simulationPendingItemsForWorkerCWaitingTimeMinutesCI,
                "hours" => _simulationPendingItemsForWorkerCWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }
    
    public void SetSimulationPendingItemsForWorkerCWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingItemsForWorkerCWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingItemsForWorkerCWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingItemsForWorkerCWaitingTimeHours = $"{averageTime / 3600:F2}";
           
        _simulationPendingItemsForWorkerCWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingItemsForWorkerCWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingItemsForWorkerCWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerCWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerCWaitingTimeCI));
    }
    
    
    private string _simulationPendingItemsForWorkerBWaitingTimeSeconds = "-";
    private string _simulationPendingItemsForWorkerBWaitingTimeMinutes = "-";
    private string _simulationPendingItemsForWorkerBWaitingTimeHours = "-";

    public string SimulationPendingItemsForWorkerBWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerBWaitingTimeSeconds,
                "minutes" => _simulationPendingItemsForWorkerBWaitingTimeMinutes,
                "hours" => _simulationPendingItemsForWorkerBWaitingTimeHours,
                _ => "-"
            };
        }
    }

    private string _simulationPendingItemsForWorkerBWaitingTimeSecondsCI = "-";
    private string _simulationPendingItemsForWorkerBWaitingTimeMinutesCI = "-";
    private string _simulationPendingItemsForWorkerBWaitingTimeHoursCI = "-";

    public string SimulationPendingItemsForWorkerBWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerBWaitingTimeSecondsCI,
                "minutes" => _simulationPendingItemsForWorkerBWaitingTimeMinutesCI,
                "hours" => _simulationPendingItemsForWorkerBWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationPendingItemsForWorkerBWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingItemsForWorkerBWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingItemsForWorkerBWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingItemsForWorkerBWaitingTimeHours = $"{averageTime / 3600:F2}";
            
        _simulationPendingItemsForWorkerBWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingItemsForWorkerBWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingItemsForWorkerBWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerBWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerBWaitingTimeCI));
    }
    
    
    private string _simulationPendingItemsForWorkerMixWaitingTimeSeconds = "-";
    private string _simulationPendingItemsForWorkerMixWaitingTimeMinutes = "-";
    private string _simulationPendingItemsForWorkerMixWaitingTimeHours = "-";

    public string SimulationPendingItemsForWorkerMixWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerMixWaitingTimeSeconds,
                "minutes" => _simulationPendingItemsForWorkerMixWaitingTimeMinutes,
                "hours" => _simulationPendingItemsForWorkerMixWaitingTimeHours,
                _ => "-"
            };
        }
    }

    private string _simulationPendingItemsForWorkerMixWaitingTimeSecondsCI = "-";
    private string _simulationPendingItemsForWorkerMixWaitingTimeMinutesCI = "-";
    private string _simulationPendingItemsForWorkerMixWaitingTimeHoursCI = "-";

    public string SimulationPendingItemsForWorkerMixWaitingTimeCI
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _simulationPendingItemsForWorkerMixWaitingTimeSecondsCI,
                "minutes" => _simulationPendingItemsForWorkerMixWaitingTimeMinutesCI,
                "hours" => _simulationPendingItemsForWorkerMixWaitingTimeHoursCI,
                _ => "-"
            };
        }
    }

    public void SetSimulationPendingItemsForWorkerMixWaitingTime(double averageTime, double ciLowerTime, double ciUpperTime)
    {
        _simulationPendingItemsForWorkerMixWaitingTimeSeconds = $"{averageTime:F2}";
        _simulationPendingItemsForWorkerMixWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _simulationPendingItemsForWorkerMixWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        _simulationPendingItemsForWorkerMixWaitingTimeSecondsCI = $"<{ciLowerTime:F2} ; {ciUpperTime:F2}>";
        _simulationPendingItemsForWorkerMixWaitingTimeMinutesCI = $"<{ciLowerTime / 60:F2} ; {ciUpperTime / 60:F2}>";
        _simulationPendingItemsForWorkerMixWaitingTimeHoursCI = $"<{ciLowerTime / 3600:F2} ; {ciUpperTime / 3600:F2}>";

        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerMixWaitingTime));
        OnPropertyChanged(nameof(SimulationPendingItemsForWorkerMixWaitingTimeCI));
    }
    
    
    private string _simulationAssemblyLinesUtilization = "-";
    
    public string SimulationAssemblyLinesUtilization
    {
        get => _simulationAssemblyLinesUtilization;
        set
        {
            _simulationAssemblyLinesUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _simulationAssemblyLinesUtilizationCI = "-";
    
    public string SimulationAssemblyLinesUtilizationCI
    {
        get => _simulationAssemblyLinesUtilizationCI;
        set
        {
            _simulationAssemblyLinesUtilizationCI = value;
            OnPropertyChanged();
        }
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
    
    private ObservableCollection<AssemblyLineDTO> _simulationAllAssemblyLinesUtilization = [];
    
    public ObservableCollection<AssemblyLineDTO> SimulationAllAssemblyLinesUtilization
    {
        get => _simulationAllAssemblyLinesUtilization;
        set {
            _simulationAllAssemblyLinesUtilization = value;
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
