using System;
using System.Collections.Generic;
using DiscreteSimulation.FurnitureManufacturer;
using Simulation;

namespace DiscreteSimulation.GUI.ViewModels;

public class SharedViewModel : ViewModelBase
{
    private readonly MySimulation _simulation = new();
    
    public MySimulation Simulation => _simulation;
    
    public event Action? SimulationStarted;
    
    public event Action<OSPABA.Simulation>? ReplicationEnded;


    public SharedViewModel()
    {
        _simulation.OnReplicationDidFinish(simulation => ReplicationEnded?.Invoke(simulation));
    }
    
    private int _replications = 10;
    
    public int Replications
    {
        get => _replications;
        set
        {
            if (value == _replications) return;
            _replications = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSingleReplication));
            OnPropertyChanged(nameof(IsMultipleReplications));
        }
    }
    
    public bool IsSingleReplication => Replications == 1;
    
    public bool IsMultipleReplications => !IsSingleReplication;
    
    private string _currentSimulationTime = "[Week 1 - Monday] 06:00:00";
    
    public string CurrentSimulationTime
    {
        get => _currentSimulationTime;
        set
        {
            _currentSimulationTime = value;
            OnPropertyChanged();
        }
    }
    
    private int _maxReplicationTime = 7_171_200;
    
    public int MaxReplicationTime
    {
        get => _maxReplicationTime;
        set
        {
            _maxReplicationTime = value;
            OnPropertyChanged();
        }
    }
    
    private int _selectedSpeedIndex = 0;

    public int SelectedSpeedIndex
    {
        get => _selectedSpeedIndex;
        set
        {
            _selectedSpeedIndex = value;
            OnPropertyChanged();
        }
    }
    
    public OrderedDictionary<double, string> SpeedOptions { get; set; } = new()
    {
        { 1.0, "x1" },
        { 2.0, "x2" },
        { 5.0, "x5" },
        { 10.0, "x10" },
        { 50.0, "x50" },
        { 100.0, "x100" },
        { 500.0, "x500" },
        { 1000.0, "x1000" },
        { 10000.0, "x10000" },
        { 100000.0, "x100000" },
        { double.PositiveInfinity, "MAX" }
    };
    
    
    private bool _isStartSimulationButtonEnabled = true;

    public bool IsStartSimulationButtonEnabled
    {
        get => _isStartSimulationButtonEnabled;
        set
        {
            _isStartSimulationButtonEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStopSimulationButtonEnabled));
        }
    }
    
    public bool IsStopSimulationButtonEnabled => !IsStartSimulationButtonEnabled;

    private bool _isPauseResumeSimulationButtonEnabled = false;
    
    public bool IsPauseResumeSimulationButtonEnabled
    {
        get => _isPauseResumeSimulationButtonEnabled;
        set
        {
            _isPauseResumeSimulationButtonEnabled = value;
            OnPropertyChanged();
        }
    }
    
    private string _pauseResumeSimulationButtonText = "Pause";
    
    public string PauseResumeSimulationButtonText
    {
        get => _pauseResumeSimulationButtonText;
        set
        {
            _pauseResumeSimulationButtonText = value;
            OnPropertyChanged();
        }
    }

    public void PauseResumeSimulation()
    {
        if (Simulation.IsPaused())
        {
            Simulation.ResumeSimulation();
            PauseResumeSimulationButtonText = "Pause";
        }
        else
        {
            Simulation.PauseSimulation();
            PauseResumeSimulationButtonText = "Resume";
        }
    }
    
    private string _selectedTimeUnits = "hours";
    
    public string SelectedTimeUnits
    {
        get => _selectedTimeUnits;
        set
        {
            _selectedTimeUnits = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfAssemblyLines = 80;
    
    public int CountOfAssemblyLines
    {
        get => _countOfAssemblyLines;
        set
        {
            _countOfAssemblyLines = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupA = 5;
    
    public int CountOfWorkersGroupA
    {
        get => _countOfWorkersGroupA;
        set
        {
            _countOfWorkersGroupA = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupB = 5;
    
    public int CountOfWorkersGroupB
    {
        get => _countOfWorkersGroupB;
        set
        {
            _countOfWorkersGroupB = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupC = 60;
    
    public int CountOfWorkersGroupC
    {
        get => _countOfWorkersGroupC;
        set
        {
            _countOfWorkersGroupC = value;
            OnPropertyChanged();
        }
    }
    
    private bool _enableWorkerLocationPreference = false;
    
    public bool EnableWorkerLocationPreference
    {
        get => _enableWorkerLocationPreference;
        set
        {
            _enableWorkerLocationPreference = value;
            OnPropertyChanged();
        }
    }
    
    private bool _enableRender95ConfidenceInterval = true;
    
    public bool EnableRender95ConfidenceInterval
    {
        get => _enableRender95ConfidenceInterval;
        set
        {
            _enableRender95ConfidenceInterval = value;
            OnPropertyChanged();
        }
    }
}
