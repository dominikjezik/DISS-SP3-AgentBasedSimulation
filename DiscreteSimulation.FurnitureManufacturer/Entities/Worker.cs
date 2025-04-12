using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Simulation;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Worker
{
    private readonly FurnitureManufacturerSimulation _furnitureManufacturerSimulation;
    
    private WeightedStatistics _utilizationStatistics = new();
    
    private double _lastChangeInUtilizationTime = 0;
    
    private Furniture? _currentFurniture;

    public Worker(FurnitureManufacturerSimulation furnitureManufacturerSimulation)
    {
        _furnitureManufacturerSimulation = furnitureManufacturerSimulation;
    }
    
    public int Id { get; set; }
    
    public WorkerGroup Group { get; set; }
    
    public Furniture? CurrentFurniture
    {
        get => _currentFurniture;
        set {
            RefreshStatistics();
            _currentFurniture = value;
        }
    }

    public string DisplayId
    {
        get
        {
            if (Group == WorkerGroup.GroupA)
            {
                return $"A{Id}";
            }

            if (Group == WorkerGroup.GroupB)
            {
                return $"B{Id}";
            }
            
            if (Group == WorkerGroup.GroupC)
            {
                return $"C{Id}";
            }

            return Id.ToString();
        }
    }
    
    public bool IsBusy => CurrentFurniture != null;

    public bool IsInWarehouse { get; set; }
    
    public bool IsMovingToWarehouse { get; set; }
    
    public AssemblyLine? CurrentAssemblyLine { get; set; }
    
    public bool IsMovingToAssemblyLine { get; set; }
    
    public double Utilization => double.IsNaN(_utilizationStatistics.Mean) ? 0 : _utilizationStatistics.Mean;
    
    public void RefreshStatistics()
    {
        var timeInterval = _furnitureManufacturerSimulation.SimulationTime - _lastChangeInUtilizationTime;
        
        _utilizationStatistics.AddValue(CurrentFurniture != null ? 1 : 0, timeInterval);
        
        _lastChangeInUtilizationTime = _furnitureManufacturerSimulation.SimulationTime;
    }
}
