using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.DTOs;

public class WorkerDTO : INotifyPropertyChanged, IUpdatable<WorkerDTO>
{
    private string _id;
    private bool _isBusy;
    private string _place;
    private string _furniture;
    private string _state;
    private string _utilization;

    public string Id
    {
        get => _id;
        set
        {
            if (value == _id) return;
            _id = value;
            OnPropertyChanged(nameof(Id));
        }
    }
    
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (value == _isBusy) return;
            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(StatusColor));
        }
    }
    
    public string StatusColor
    {
        get
        {
            if (IsBusy)
            {
                return "Red";
            }
            
            return "Green";
        }
    }

    public string Place
    {
        get => _place;
        set
        {
            if (value == _place) return;
            _place = value;
            OnPropertyChanged(nameof(Place));
        }
    }

    public string Furniture
    {
        get => _furniture;
        set
        {
            if (value == _furniture) return;
            _furniture = value;
            OnPropertyChanged(nameof(Furniture));
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

    public void Update(Worker worker)
    {
        Id = worker.DisplayId;
        
        if (worker.IsMovingToWarehouse)
        {
            Place = "Moving to warehouse";
        }
        else if (worker.IsMovingToAssemblyLine)
        {
            Place = "Moving to assembly line";
        }
        else if (worker.IsInWarehouse)
        {
            Place = "Warehouse";
        }
        else if (worker.CurrentAssemblyLine != null)
        {
            Place = $"Assembly line {worker.CurrentAssemblyLine.Id}";
        }
        else
        {
            Place = "???";
        }
        
        if (worker.CurrentFurniture == null)
        {
            Furniture = string.Empty;
            State = "Idle";
        }
        else
        {
            Furniture = worker.CurrentFurniture.DisplayId;
            State = worker.CurrentFurniture.CurrentOperationStep.ToString();
        }
        
        Utilization = worker.Utilization.ToString("0.00%");
        
        IsBusy = worker.IsBusy;
    }
    
    public void Update(WorkerDTO workerDTO)
    {
        Id = workerDTO.Id;
        Place = workerDTO.Place;
        Furniture = workerDTO.Furniture;
        State = workerDTO.State;
        Utilization = workerDTO.Utilization;
        IsBusy = workerDTO.IsBusy;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public static class WorkerDTOExtensions
{
    public static WorkerDTO ToDTO(this Worker worker)
    {
        var dto = new WorkerDTO();
        dto.Update(worker);
        return dto;
    }
    
    public static WorkerDTO ToUtilizationDTO(this Worker worker, double mean, (double, double) utilization)
    {
        var dto = new WorkerDTO();
        dto.Update(worker);
        
        if (worker.Group == WorkerGroup.GroupA)
        {
            dto.State = "Worker group A";
        } 
        else if (worker.Group == WorkerGroup.GroupB)
        {
            dto.State = "Worker group B";
        }
        else
        {
            dto.State = "Worker group C";
        }
        
        dto.Utilization = $"{(mean*100):F2}%";
        dto.Place = $"<{(utilization.Item1*100):F2} ; {(utilization.Item2*100):F2}>";
        
        return dto;
    }
}
