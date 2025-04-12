namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Order
{
    public int Id { get; set; }
    
    public List<Furniture> FurnitureItems { get; set; } = new();
    
    public int FurnitureItemsCount => FurnitureItems.Count;

    public int FinishedFurnitureItemsCount { get; set; } = 0;
    
    public double ArrivalTime { get; set; }
    
    public double StartedWaitingTime { get; set; }
    
    public string State { get; set; }
}
