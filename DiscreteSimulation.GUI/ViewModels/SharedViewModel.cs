using System.Collections.Generic;
using System.Linq;
using DiscreteSimulation.FurnitureManufacturer.Simulation;

namespace DiscreteSimulation.GUI.ViewModels;

public class SharedViewModel : ViewModelBase
{
    private readonly FurnitureManufacturerSimulation _simulation = new();
    
    public FurnitureManufacturerSimulation Simulation => _simulation;
    
    private long _replications = 1;
    
    public long Replications
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
            
            Simulation.SimulationSpeed = SpeedOptions.ElementAt(SelectedSpeedIndex).Key;
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
        if (Simulation.IsSimulationPaused)
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
    
    
    
    
    
    private int _countOfAssemblyLines = 10;
    
    public int CountOfAssemblyLines
    {
        get => _countOfAssemblyLines;
        set
        {
            _countOfAssemblyLines = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupA = 2;
    
    public int CountOfWorkersGroupA
    {
        get => _countOfWorkersGroupA;
        set
        {
            _countOfWorkersGroupA = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupB = 2;
    
    public int CountOfWorkersGroupB
    {
        get => _countOfWorkersGroupB;
        set
        {
            _countOfWorkersGroupB = value;
            OnPropertyChanged();
        }
    }
    
    private int _countOfWorkersGroupC = 18;
    
    public int CountOfWorkersGroupC
    {
        get => _countOfWorkersGroupC;
        set
        {
            _countOfWorkersGroupC = value;
            OnPropertyChanged();
        }
    }
    
    private bool _enableWorkerLocationPreference = true;
    
    public bool EnableWorkerLocationPreference
    {
        get => _enableWorkerLocationPreference;
        set
        {
            _enableWorkerLocationPreference = value;
            OnPropertyChanged();
        }
    }
}
