using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Order
{
    public int Id { get; set; }
    
    public OrderState State { get; set; } = OrderState.Pending;
    
    public List<Furniture> FurnitureItems { get; set; } = new();

    public int FinishedFurnitureItemsCount { get; set; } = 0;
    
    public double ArrivalTime { get; set; }
    
    public double StartedWaitingTime { get; set; }
}
