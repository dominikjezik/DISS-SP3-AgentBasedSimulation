using System.ComponentModel;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.DTOs;

public class OrderDTO : INotifyPropertyChanged, IUpdatable<OrderDTO>
{
    private int _id;
    private string _countOfFurnitureItems;
    private string _state;
    private string _arrivalTime;
    private string _waitingTime;

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
    
    public List<FurnitureDTO> FurnitureItems { get; set; } = new();

    public string CountOfFurnitureItems
    {
        get => _countOfFurnitureItems;
        set
        {
            if (value == _countOfFurnitureItems) return;
            _countOfFurnitureItems = value;
            OnPropertyChanged(nameof(CountOfFurnitureItems));
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

    public string ArrivalTime
    {
        get => _arrivalTime;
        set
        {
            if (value.Equals(_arrivalTime)) return;
            _arrivalTime = value;
            OnPropertyChanged(nameof(ArrivalTime));
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

    public void Update(Order order, double currentSimulationTime)
    {
        Id = order.Id;
        FurnitureItems = order.FurnitureItems.Select(f => f.ToDTO(currentSimulationTime)).ToList();
        CountOfFurnitureItems = $"{order.FinishedFurnitureItemsCount}/{order.FurnitureItems.Count}";
        State = order.State.ToString();
        ArrivalTime = order.ArrivalTime.ToString("F2");
        WaitingTime = (currentSimulationTime - order.StartedWaitingTime).FormatToSimulationTime(timeOnly: true);
    }
    
    public void Update(OrderDTO orderDTO)
    {
        Id = orderDTO.Id;
        FurnitureItems = orderDTO.FurnitureItems;
        CountOfFurnitureItems = orderDTO.CountOfFurnitureItems;
        State = orderDTO.State;
        ArrivalTime = orderDTO.ArrivalTime.FormatToSimulationTime(shortFormat: true);
        WaitingTime = orderDTO.WaitingTime;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public static class OrderDTOExtensions
{
    public static OrderDTO ToDTO(this Order order, double currentSimulationTime)
    {
        var orderDTO = new OrderDTO();
        orderDTO.Update(order, currentSimulationTime);
        return orderDTO;
    }
}
