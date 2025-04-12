using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.DTOs;

public class FurnitureDTO : INotifyPropertyChanged, IUpdatable<FurnitureDTO>
{
    private string _id;
    private string _displayId;
    private FurnitureType _type;
    private string _state;
    private string _waitingTime;
    private string _worker;
    
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
    
    public string DisplayId
    {
        get => _displayId;
        set
        {
            if (value == _displayId) return;
            _displayId = value;
            OnPropertyChanged(nameof(DisplayId));
        }
    }
    
    public FurnitureType Type
    {
        get => _type;
        set
        {
            if (value == _type) return;
            _type = value;
            OnPropertyChanged(nameof(Type));
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
    
    public string WaitingTime
    {
        get => _waitingTime;
        set
        {
            if (value.Equals(_waitingTime)) return;
            _waitingTime = value;
            OnPropertyChanged(nameof(WaitingTime));
        }
    }
    
    public string Worker
    {
        get => _worker;
        set
        {
            if (value == _worker) return;
            _worker = value;
            OnPropertyChanged(nameof(Worker));
        }
    }
    
    public void Update(Furniture furniture, double currentSimulationTime)
    {
        Id = furniture.Id.ToString();
        DisplayId = furniture.DisplayId;
        Type = furniture.Type;
        State = furniture.State;
        WaitingTime = (currentSimulationTime - furniture.StartedWaitingTime).FormatToSimulationTime(timeOnly: true);
        Worker = furniture?.CurrentWorker?.DisplayId ?? string.Empty;
    }
    
    public void Update(FurnitureDTO furnitureDTO)
    {
        Id = furnitureDTO.Id;
        DisplayId = furnitureDTO.DisplayId;
        Type = furnitureDTO.Type;
        State = furnitureDTO.State;
        WaitingTime = furnitureDTO.WaitingTime;
        Worker = furnitureDTO.Worker;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public static class FurnitureDTOExtensions
{
    public static FurnitureDTO ToDTO(this Furniture furniture, double currentSimulationTime)
    {
        var furnitureDTO = new FurnitureDTO();
        furnitureDTO.Update(furniture, currentSimulationTime);
        return furnitureDTO;
    }
}
