using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DiscreteSimulation.FurnitureManufacturer.DTOs;

namespace DiscreteSimulation.GUI.ViewModels.Panels;

public class SingleReplicationPanelViewModel : ViewModelBase
{
    public SharedViewModel Shared { get; private set; }
    
    public SingleReplicationPanelViewModel(SharedViewModel shared)
    {
        Shared = shared;
        Shared.PropertyChanged += OnSharedPropertyChanged;
    }

    private void OnSharedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Shared.SelectedTimeUnits))
        {
            OnPropertyChanged(nameof(ReplicationOrderProcessingTime));
            OnPropertyChanged(nameof(ReplicationOrderItemProcessingTime));
            OnPropertyChanged(nameof(ReplicationPendingOrdersWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForLineWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerCWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerAWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerBWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerMixWaitingTime));
        }
        else if (e.PropertyName == nameof(Shared.SelectedSpeedIndex))
        {
            OnPropertyChanged(nameof(Delta));
            
            if (!double.IsInfinity(Shared.SpeedOptions.ElementAt(Shared.SelectedSpeedIndex).Key))
            {
                Shared.Simulation.SetSimSpeed(Delta, SleepTime);
            }
            else
            {
                Shared.Simulation.SetMaxSimSpeed();
            }
        }
        else if (e.PropertyName == nameof(Shared.IsAnimatorOn))
        {
            AnimatorRequestChanged?.Invoke();
        }
    }
    
    public event Action? AnimatorRequestChanged;

    public void ResetForSimulationStart()
    {
        Orders = new ObservableCollection<OrderDTO>();
        AssemblyLines = new ObservableCollection<AssemblyLineDTO>();
        WorkersGroupA = new ObservableCollection<WorkerDTO>();
        WorkersGroupB = new ObservableCollection<WorkerDTO>();
        WorkersGroupC = new ObservableCollection<WorkerDTO>();
        PendingOrdersQueue = new ObservableCollection<OrderDTO>();
        PendingItemsForWorkerAQueue = new ObservableCollection<FurnitureDTO>();
        PendingItemsForWorkerBQueue = new ObservableCollection<FurnitureDTO>();
        PendingItemsForWorkerMixQueue = new ObservableCollection<FurnitureDTO>();
        
        for (int i = 0; i < Shared.CountOfWorkersGroupA; i++)
        {
            WorkersGroupA.Add(new WorkerDTO());
        }
        
        for (int i = 0; i < Shared.CountOfWorkersGroupB; i++)
        {
            WorkersGroupB.Add(new WorkerDTO());
        }
        
        for (int i = 0; i < Shared.CountOfWorkersGroupC; i++)
        {
            WorkersGroupC.Add(new WorkerDTO());
        }
        
        for (int i = 0; i < Shared.CountOfAssemblyLines; i++)
        {
            AssemblyLines.Add(new AssemblyLineDTO());
        }
    }

    private double _simulationSpeed = 1.0;
    
    private double _sleepTime = 0.1;
    
    public double SleepTime
    {
        get => _sleepTime;
        set
        {
            _sleepTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Delta));
            
            if (!double.IsInfinity(SimulationSpeed))
            {
                Shared.Simulation.SetSimSpeed(Delta, SleepTime);
            }
            else
            {
                Shared.Simulation.SetMaxSimSpeed();
            }
        }
    }
    
    public double SimulationSpeed => Shared.SpeedOptions.ElementAt(Shared.SelectedSpeedIndex).Key;
    
    public double Delta => SimulationSpeed * SleepTime;
    
    #region OrderProcessingTime
    
    private string _replicationOrderProcessingTimeSeconds = "-";
    private string _replicationOrderProcessingTimeMinutes = "-";
    private string _replicationOrderProcessingTimeHours = "-";
    
    public string ReplicationOrderProcessingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationOrderProcessingTimeSeconds,
                "minutes" => _replicationOrderProcessingTimeMinutes,
                "hours" => _replicationOrderProcessingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationOrderProcessingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationOrderProcessingTimeSeconds = $"{averageTime:F2}";
        _replicationOrderProcessingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationOrderProcessingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationOrderProcessingTime));
    }
    
    
    private string _replicationOrderItemProcessingTimeSeconds = "-";
    private string _replicationOrderItemProcessingTimeMinutes = "-";
    private string _replicationOrderItemProcessingTimeHours = "-";
    
    public string ReplicationOrderItemProcessingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationOrderItemProcessingTimeSeconds,
                "minutes" => _replicationOrderItemProcessingTimeMinutes,
                "hours" => _replicationOrderItemProcessingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationOrderItemProcessingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationOrderItemProcessingTimeSeconds = $"{averageTime:F2}";
        _replicationOrderItemProcessingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationOrderItemProcessingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationOrderItemProcessingTime));
    }
    
    #endregion
    
    #region StatisticsQueuesLength
    
    private string _replicationPendingOrders = "-";
    
    public string ReplicationPendingOrders
    {
        get => _replicationPendingOrders;
        set
        {
            _replicationPendingOrders = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForLine = "-";
    
    public string ReplicationPendingItemsForLine
    {
        get => _replicationPendingItemsForLine;
        set
        {
            _replicationPendingItemsForLine = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForWorkerA = "-";
    
    public string ReplicationPendingItemsForWorkerA
    {
        get => _replicationPendingItemsForWorkerA;
        set
        {
            _replicationPendingItemsForWorkerA = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForWorkerC = "-";
    
    public string ReplicationPendingItemsForWorkerC
    {
        get => _replicationPendingItemsForWorkerC;
        set
        {
            _replicationPendingItemsForWorkerC = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForWorkerB = "-";
    
    public string ReplicationPendingItemsForWorkerB
    {
        get => _replicationPendingItemsForWorkerB;
        set
        {
            _replicationPendingItemsForWorkerB = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForWorkerMix = "-";
    
    public string ReplicationPendingItemsForWorkerMix
    {
        get => _replicationPendingItemsForWorkerMix;
        set
        {
            _replicationPendingItemsForWorkerMix = value;
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region StatisticsWaitingTimeQueues
    
    private string _replicationPendingOrdersWaitingTimeSeconds = "-";
    private string _replicationPendingOrdersWaitingTimeMinutes = "-";
    private string _replicationPendingOrdersWaitingTimeHours = "-";
    
    public string ReplicationPendingOrdersWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingOrdersWaitingTimeSeconds,
                "minutes" => _replicationPendingOrdersWaitingTimeMinutes,
                "hours" => _replicationPendingOrdersWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingOrdersWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingOrdersWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingOrdersWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingOrdersWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingOrdersWaitingTime));
    }
    
    private string _replicationPendingItemsForLineWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForLineWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForLineWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForLineWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForLineWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForLineWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForLineWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForLineWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForLineWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForLineWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForLineWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForLineWaitingTime));
    }
    
    private string _replicationPendingItemsForWorkerAWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForWorkerAWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForWorkerAWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForWorkerAWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForWorkerAWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForWorkerAWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForWorkerAWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForWorkerAWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForWorkerAWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForWorkerAWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForWorkerAWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerAWaitingTime));
    }
    
    private string _replicationPendingItemsForWorkerCWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForWorkerCWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForWorkerCWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForWorkerCWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForWorkerCWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForWorkerCWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForWorkerCWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForWorkerCWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForWorkerCWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForWorkerCWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForWorkerCWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerCWaitingTime));
    }
    
    private string _replicationPendingItemsForWorkerBWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForWorkerBWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForWorkerBWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForWorkerBWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForWorkerBWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForWorkerBWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForWorkerBWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForWorkerBWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForWorkerBWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForWorkerBWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForWorkerBWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerBWaitingTime));
    }
    
    private string _replicationPendingItemsForWorkerMixWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForWorkerMixWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForWorkerMixWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForWorkerMixWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForWorkerMixWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForWorkerMixWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForWorkerMixWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForWorkerMixWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForWorkerMixWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForWorkerMixWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForWorkerMixWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForWorkerMixWaitingTime));
    }
    
    #endregion
    
    #region StatisticsAssemblyLines
    
    private string _replicationAssemblyLinesUtilization = "-";
    
    public string ReplicationAssemblyLinesUtilization
    {
        get => _replicationAssemblyLinesUtilization;
        set
        {
            _replicationAssemblyLinesUtilization = value;
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region StatisticsWorkers
    
    private string _replicationWorkersGroupAUtilization = "-";
    
    public string ReplicationWorkersGroupAUtilization
    {
        get => _replicationWorkersGroupAUtilization;
        set
        {
            _replicationWorkersGroupAUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationWorkersGroupBUtilization = "-";
    
    public string ReplicationWorkersGroupBUtilization
    {
        get => _replicationWorkersGroupBUtilization;
        set
        {
            _replicationWorkersGroupBUtilization = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationWorkersGroupCUtilization = "-";
    
    public string ReplicationWorkersGroupCUtilization
    {
        get => _replicationWorkersGroupCUtilization;
        set
        {
            _replicationWorkersGroupCUtilization = value;
            OnPropertyChanged();
        }
    }
    
    #endregion
    
    #region OrdersAndAssemblyLines
    
    private ObservableCollection<OrderDTO> _orders = [];

    public ObservableCollection<OrderDTO> Orders
    {
        get => _orders;
        set {
            _orders = value;
            OnPropertyChanged();
        }
    }
    
    private OrderDTO? _selectedOrder;

    public OrderDTO? SelectedOrder
    {
        get => _selectedOrder;
        set {
            _selectedOrder = value;
            if (_selectedOrder != null)
            {
                SelectedOrderFurnitureItems = new ObservableCollection<FurnitureDTO>(_selectedOrder.FurnitureItems);
            }
            else
            {
                SelectedOrderFurnitureItems = new ObservableCollection<FurnitureDTO>();
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSelectedOrder));
            OnPropertyChanged(nameof(OrdersDataGridsRowSpan));
        }
    }
    
    private ObservableCollection<FurnitureDTO> _selectedOrderFurnitureItems = [];

    public ObservableCollection<FurnitureDTO> SelectedOrderFurnitureItems
    {
        get => _selectedOrderFurnitureItems;
        set {
            _selectedOrderFurnitureItems = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsSelectedOrder => _selectedOrder != null;
    
    public int OrdersDataGridsRowSpan => IsSelectedOrder ? 1 : 2;

    private ObservableCollection<AssemblyLineDTO> _assemblyLines = [];
    
    public ObservableCollection<AssemblyLineDTO> AssemblyLines
    {
        get => _assemblyLines;
        set {
            _assemblyLines = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region WorkersLists

    private ObservableCollection<WorkerDTO> _workersGroupA = [];
    
    public ObservableCollection<WorkerDTO> WorkersGroupA
    {
        get => _workersGroupA;
        set {
            _workersGroupA = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<WorkerDTO> _workersGroupB = [];
    
    public ObservableCollection<WorkerDTO> WorkersGroupB
    {
        get => _workersGroupB;
        set {
            _workersGroupB = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<WorkerDTO> _workersGroupC = [];
    
    public ObservableCollection<WorkerDTO> WorkersGroupC
    {
        get => _workersGroupC;
        set {
            _workersGroupC = value;
            OnPropertyChanged();
        }
    }

    #endregion
    
    #region Queues
    
    private string _pendingOrdersQueueCount = "0";

    public string PendingOrdersQueueCount
    {
        get => _pendingOrdersQueueCount;
        set
        {
            _pendingOrdersQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingOrdersQueueTabTitle));
        }
    }
    
    public string PendingOrdersQueueTabTitle => $"Orders ({PendingOrdersQueueCount})";
    
    
    private string _pendingItemsForLineQueueCount = "0";

    public string PendingItemsForLineQueueCount
    {
        get => _pendingItemsForLineQueueCount;
        set
        {
            _pendingItemsForLineQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingItemsForLineQueueTabTitle));
        }
    }
    
    public string PendingItemsForLineQueueTabTitle => $"for Line ({PendingItemsForLineQueueCount})";
    
    
    private string _pendingItemsForWorkerAQueueCount = "0";
    
    public string PendingItemsForWorkerAQueueCount
    {
        get => _pendingItemsForWorkerAQueueCount;
        set
        {
            _pendingItemsForWorkerAQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingForStainingQueueTabTitle));
        }
    }
    public string PendingForStainingQueueTabTitle => $"for A ({PendingItemsForWorkerAQueueCount})";
    
    
    private string _pendingItemsForWorkerCQueueCount = "0";
    
    public string PendingItemsForWorkerCQueueCount
    {
        get => _pendingItemsForWorkerCQueueCount;
        set
        {
            _pendingItemsForWorkerCQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingItemsForWorkerCQueueTabTitle));
        }
    }
    
    public string PendingItemsForWorkerCQueueTabTitle => $"for C ({PendingItemsForWorkerCQueueCount})";
    
    
    private string _pendingItemsForWorkerBQueueCount = "0";
    
    public string PendingItemsForWorkerBQueueCount
    {
        get => _pendingItemsForWorkerBQueueCount;
        set
        {
            _pendingItemsForWorkerBQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingItemsForWorkerBQueueTabTitle));
        }
    }
    
    public string PendingItemsForWorkerBQueueTabTitle => $"for B ({PendingItemsForWorkerBQueueCount})";

    
    private string _pendingItemsForWorkerMixQueueCount = "0";
    
    public string PendingItemsForWorkerMixQueueCount
    {
        get => _pendingItemsForWorkerMixQueueCount;
        set
        {
            _pendingItemsForWorkerMixQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingItemsForWorkerMixQueueTabTitle));
        }
    }
    
    public string PendingItemsForWorkerMixQueueTabTitle => $"for A/C ({PendingItemsForWorkerMixQueueCount})";
    
    private ObservableCollection<OrderDTO> _pendingOrdersQueue = [];

    public ObservableCollection<OrderDTO> PendingOrdersQueue
    {
        get => _pendingOrdersQueue;
        set {
            _pendingOrdersQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingItemsForLineQueue = [];

    public ObservableCollection<FurnitureDTO> PendingItemsForLineQueue
    {
        get => _pendingItemsForLineQueue;
        set {
            _pendingItemsForLineQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingItemsForWorkerAQueue = [];

    public ObservableCollection<FurnitureDTO> PendingItemsForWorkerAQueue
    {
        get => _pendingItemsForWorkerAQueue;
        set {
            _pendingItemsForWorkerAQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingItemsForWorkerCQueue = [];

    public ObservableCollection<FurnitureDTO> PendingItemsForWorkerCQueue
    {
        get => _pendingItemsForWorkerCQueue;
        set {
            _pendingItemsForWorkerCQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingItemsForWorkerBQueue = [];

    public ObservableCollection<FurnitureDTO> PendingItemsForWorkerBQueue
    {
        get => _pendingItemsForWorkerBQueue;
        set {
            _pendingItemsForWorkerBQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingItemsForWorkerMixQueue = [];

    public ObservableCollection<FurnitureDTO> PendingItemsForWorkerMixQueue
    {
        get => _pendingItemsForWorkerMixQueue;
        set {
            _pendingItemsForWorkerMixQueue = value;
            OnPropertyChanged();
        }
    }
    
    #endregion

}
