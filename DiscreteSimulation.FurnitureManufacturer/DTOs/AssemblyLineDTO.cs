using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.Entities;

namespace DiscreteSimulation.FurnitureManufacturer.DTOs;

public class AssemblyLineDTO : INotifyPropertyChanged, IUpdatable<AssemblyLineDTO>
{
    private int _id;
    private string _statusColor;
    private string _currentOrder;
    private string _currentWorker;
    private string _state;
    private string _utilization;

    public int Id
    {
        get => _id;
        set
        {
            if (value == _id) return;
            _id = value;
            OnPropertyChanged(nameof(Id));
        }
    }

    public string StatusColor
    {
        get => _statusColor;
        set
        {
            if (value == _statusColor) return;
            _statusColor = value;
            OnPropertyChanged(nameof(StatusColor));
        }
    }

    public string CurrentOrder
    {
        get => _currentOrder;
        set
        {
            if (value == _currentOrder) return;
            _currentOrder = value;
            OnPropertyChanged(nameof(CurrentOrder));
        }
    }

    public string CurrentWorker
    {
        get => _currentWorker;
        set
        {
            if (value == _currentWorker) return;
            _currentWorker = value;
            OnPropertyChanged(nameof(CurrentWorker));
        }
    }

    public string State
    {
        get => _state;
        set
        {
            if (value == _state) return;
            _state = value;
            OnPropertyChanged(nameof(State));
        }
    }
    
    public string Utilization
    {
        get => _utilization;
        set
        {
            if (value == _utilization) return;
            _utilization = value;
            OnPropertyChanged(nameof(Utilization));
        }
    }

    public void Update(AssemblyLine assemblyLine)
    {
        Id = assemblyLine.Id;

        if (assemblyLine.CurrentFurniture == null)
        {
            CurrentOrder = "Free";
            StatusColor = "Green";
        }
        else
        {
            CurrentOrder = assemblyLine.CurrentFurniture.DisplayId;
            StatusColor = assemblyLine.ContainsFurniture ? "Red" : "Orange";
        }
        
        State = assemblyLine.CurrentFurniture?.CurrentOperationStep.ToString() ?? string.Empty;
        CurrentWorker = assemblyLine.CurrentWorker?.DisplayId ?? string.Empty;

        if (assemblyLine.IdleWorkers.Count > 0)
        {
            CurrentWorker += "(";
        }
        
        foreach (var idleWorker in assemblyLine.IdleWorkers)
        {
            CurrentWorker += $"{idleWorker.DisplayId}, ";
        }

        if (assemblyLine.IdleWorkers.Count > 0)
        {
            CurrentWorker = CurrentWorker.Remove(CurrentWorker.Length - 2);
            CurrentWorker += ")";
        }
        
        Utilization = assemblyLine.Utilization.ToString("0.00%");
    }
    
    public void Update(AssemblyLineDTO assemblyLineDTO)
    {
        Id = assemblyLineDTO.Id;
        CurrentOrder = assemblyLineDTO.CurrentOrder;
        CurrentWorker = assemblyLineDTO.CurrentWorker;
        State = assemblyLineDTO.State;
        StatusColor = assemblyLineDTO.StatusColor;
        Utilization = assemblyLineDTO.Utilization;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public static class AssemblyLineDTOExtensions
{
    public static AssemblyLineDTO ToDTO(this AssemblyLine assemblyLine)
    {
        var assemblyLineDTO = new AssemblyLineDTO();
        assemblyLineDTO.Update(assemblyLine);
        return assemblyLineDTO;
    }
    
    public static AssemblyLineDTO ToUtilizationDTO(this AssemblyLine assemblyLine, double mean, (double, double) utilization)
    {
        var dto = new AssemblyLineDTO();
        dto.Update(assemblyLine);
        
        dto.Utilization = $"{(mean*100):F2}%";
        dto.State = $"<{(utilization.Item1*100):F2} ; {(utilization.Item2*100):F2}>";
        
        return dto;
    }
}
