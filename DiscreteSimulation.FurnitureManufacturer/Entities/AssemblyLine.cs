namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class AssemblyLine
{
    public int Id { get; set; }
    
    public Furniture? CurrentFurniture { get; set; }
    
    public Worker? CurrentWorker { get; set; }
    
    public List<Worker> IdleWorkers { get; set; } = new();
}
