using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.GUI.ViewModels.Panels;

namespace DiscreteSimulation.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public SharedViewModel Shared { get; set; }
    
    public SingleReplicationPanelViewModel SingleReplication { get; set; }
    
    public MultipleReplicationsPanelViewModel MultipleReplications { get; set; }
    
    public FurnitureManufacturerSimulation Simulation => Shared.Simulation;

    public MainWindowViewModel()
    {
        Shared = new SharedViewModel();
        Shared.PropertyChanged += OnSharedPropertyChanged;
        SingleReplication = new SingleReplicationPanelViewModel(Shared);
        MultipleReplications = new MultipleReplicationsPanelViewModel(Shared);
    }
    
    private void OnSharedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Shared.IsStartSimulationButtonEnabled))
        {
            OnPropertyChanged(nameof(IsSpeedSelectorEnabled));
            OnPropertyChanged(nameof(IsDefaultSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsDecreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsIncreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsSpeedMaxButtonEnabled));
        }
        else if (e.PropertyName == nameof(Shared.SelectedSpeedIndex))
        {
            OnPropertyChanged(nameof(IsDefaultSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsDecreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsIncreaseSpeedButtonEnabled));
            OnPropertyChanged(nameof(IsSpeedMaxButtonEnabled));
            OnPropertyChanged(nameof(IsSpeedSelectorEnabled));
        }
    }
    
    #region SimulationControlButtons
    
    public bool IsDefaultSpeedButtonEnabled => Shared.SelectedSpeedIndex != 0 && (Shared.IsStartSimulationButtonEnabled || Shared.SelectedSpeedIndex != Shared.SpeedOptions.Count - 1);
    
    public bool IsDecreaseSpeedButtonEnabled => Shared.SelectedSpeedIndex > 0 && (Shared.IsStartSimulationButtonEnabled || Shared.SelectedSpeedIndex != Shared.SpeedOptions.Count - 1);
    
    public bool IsIncreaseSpeedButtonEnabled => Shared.SelectedSpeedIndex < Shared.SpeedOptions.Count - 1 && (Shared.IsStartSimulationButtonEnabled || Shared.SelectedSpeedIndex != Shared.SpeedOptions.Count - 1);
    
    public bool IsSpeedMaxButtonEnabled => Shared.SelectedSpeedIndex < Shared.SpeedOptions.Count - 1 && (Shared.IsStartSimulationButtonEnabled || Shared.SelectedSpeedIndex != Shared.SpeedOptions.Count - 1);
    

    public bool IsSpeedSelectorEnabled => Shared.IsStartSimulationButtonEnabled || Shared.SelectedSpeedIndex != Shared.SpeedOptions.Count - 1;
    
    public void DecreaseSpeed()
    {
        if (Shared.SelectedSpeedIndex == 0)
        {
            return;
        }
        
        Shared.SelectedSpeedIndex--;
    }

    public void IncreaseSpeed()
    {
        Shared.SelectedSpeedIndex++;
    }

    public void SetMaxSpeed()
    {
        Shared.SelectedSpeedIndex = Shared.SpeedOptions.Count - 1;
    }
    
    public void SetDefaultSpeed()
    {
        Shared.SelectedSpeedIndex = 0;
    }

    public void DisableButtonsForSimulationStart()
    {
        Shared.IsStartSimulationButtonEnabled = false;
        Shared.IsPauseResumeSimulationButtonEnabled = true;
    }

    public void EnableButtonsForSimulationEnd()
    {
        Shared.IsStartSimulationButtonEnabled = true;
        Shared.IsPauseResumeSimulationButtonEnabled = false;
        Shared.PauseResumeSimulationButtonText = "Pause";
    }
    
    #endregion
    
    #region MultipleReplicationsControls
    
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

    #endregion
    
}
