using OSPABA;

namespace DiscreteSimulation.Core.Utilities;

public class EntitiesPriorityQueue<TEntity>
{
    private Simulation _simulation;

    private PriorityQueue<TEntity, TEntity> _queue;
    private WeightedStatistics _weightedStatistics = new();
    private double _lastChangeInQueueTime = 0;
    
    public PriorityQueue<TEntity, TEntity> OriginalQueue => _queue;
    
    public int Count => _queue.Count;
    
    public double AverageQueueLength => double.IsNaN(_weightedStatistics.Mean) ? 0 : _weightedStatistics.Mean;
    
    public EntitiesPriorityQueue(IComparer<TEntity> comparator, Simulation simulation)
    {
        _simulation = simulation;
        _queue = new PriorityQueue<TEntity, TEntity>(comparator);
    }
    
    public void Enqueue(TEntity entity)
    {
        RefreshStatistics();
        _queue.Enqueue(entity, entity);
    }
    
    public TEntity Dequeue()
    {
        RefreshStatistics();
        return _queue.Dequeue();
    }
    
    public TEntity Peek()
    {
        return _queue.Peek();
    }
    
    public void Clear()
    {
        _queue.Clear();
        _weightedStatistics.Clear();
        _lastChangeInQueueTime = 0;
    }

    public void RefreshStatistics()
    {
        var timeInterval = _simulation.CurrentTime - _lastChangeInQueueTime;
        var queueLength = _queue.Count;
        
        _weightedStatistics.AddValue(queueLength, timeInterval);
        
        _lastChangeInQueueTime = _simulation.CurrentTime;
    }
}
