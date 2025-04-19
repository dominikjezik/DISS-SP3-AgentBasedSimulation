using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Furniture
{
    public int Id { get; set; }
    
    public Order Order { get; set; }
    
    public FurnitureType Type { get; set; }
    
    public AssemblyLine? CurrentAssemblyLine { get; set; }
    
    public Worker? CurrentWorker { get; set; }

    public string DisplayId => $"{Order.Id} [{Id}]";
    
    public double StartedWaitingTime { get; set; }
    
    public string State { get; set; }
        
    public FurnitureOperationStep CurrentOperationStep { get; set; } = FurnitureOperationStep.NotStarted;
    
    public double OperationStepEndTime { get; set; }
}
