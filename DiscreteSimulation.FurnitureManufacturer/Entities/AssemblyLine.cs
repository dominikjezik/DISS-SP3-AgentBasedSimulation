using DiscreteSimulation.Core.Utilities;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class AssemblyLine
{
    private readonly OSPABA.Simulation _simulation;
    
    private WeightedStatistics _utilizationStatistics = new();
    
    private double _lastChangeInUtilizationTime = 0;
    
    private Furniture? _currentFurniture;

    public AssemblyLine(OSPABA.Simulation simulation)
    {
        _simulation = simulation;
    }
    
    public int Id { get; set; }
    
    public bool ContainsFurniture { get; set; } = false;

    public Furniture? CurrentFurniture
    {
        get => _currentFurniture;
        set
        {
            RefreshStatistics();
            _currentFurniture = value;
        }
    }

    public Worker? CurrentWorker { get; set; }
    
    public List<Worker> IdleWorkers { get; set; } = new();
    
    public double Utilization => double.IsNaN(_utilizationStatistics.Mean) ? 0 : _utilizationStatistics.Mean;
    
    public void RefreshStatistics()
    {
        var timeInterval = _simulation.CurrentTime - _lastChangeInUtilizationTime;
        
        _utilizationStatistics.AddValue(CurrentFurniture != null ? 1 : 0, timeInterval);
        
        _lastChangeInUtilizationTime = _simulation.CurrentTime;
    }
}
