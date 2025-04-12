using System.Collections.ObjectModel;
using System.ComponentModel;
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
            OnPropertyChanged(nameof(ReplicationPendingOrdersWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForStainingWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForFoldingWaitingTime));
            OnPropertyChanged(nameof(ReplicationPendingItemsForFittingsWaitingTime));
        }
        else if (e.PropertyName == nameof(Shared.SelectedSpeedIndex))
        {
            OnPropertyChanged(nameof(Delta));
        }
    }

    public void ResetForSimulationStart()
    {
        Orders = new ObservableCollection<OrderDTO>();
        AssemblyLines = new ObservableCollection<AssemblyLineDTO>();
        WorkersGroupA = new ObservableCollection<WorkerDTO>();
        WorkersGroupB = new ObservableCollection<WorkerDTO>();
        WorkersGroupC = new ObservableCollection<WorkerDTO>();
        PendingOrdersQueue = new ObservableCollection<OrderDTO>();
        PendingForStainingQueue = new ObservableCollection<FurnitureDTO>();
        PendingForFoldingQueue = new ObservableCollection<FurnitureDTO>();
        PendingForFittingsQueue = new ObservableCollection<FurnitureDTO>();
        
        for (int i = 0; i < Shared.Simulation.CountOfWorkersGroupA; i++)
        {
            WorkersGroupA.Add(new WorkerDTO());
        }
        
        for (int i = 0; i < Shared.Simulation.CountOfWorkersGroupB; i++)
        {
            WorkersGroupB.Add(new WorkerDTO());
        }
        
        for (int i = 0; i < Shared.Simulation.CountOfWorkersGroupC; i++)
        {
            WorkersGroupC.Add(new WorkerDTO());
        }
    }
    
    public int SleepTime
    {
        get => Shared.Simulation.SleepTime;
        set
        {
            Shared.Simulation.SleepTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Delta));
        }
    }
    
    public double Delta => Shared.Simulation.Delta;
    
    #region CurrentSimulationTime
    
    private string _currentSimulationTime = "[Week 1 - Monday] 06:00:00";
    
    public string CurrentSimulationTime
    {
        get => _currentSimulationTime;
        set
        {
            _currentSimulationTime = value;
            OnPropertyChanged();
        }
    }
    
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
    
    private string _replicationPendingFurnitureItems = "-";
    
    public string ReplicationPendingFurnitureItems
    {
        get => _replicationPendingFurnitureItems;
        set
        {
            _replicationPendingFurnitureItems = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForStaining = "-";
    
    public string ReplicationPendingItemsForStaining
    {
        get => _replicationPendingItemsForStaining;
        set
        {
            _replicationPendingItemsForStaining = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForVarnishing = "-";
    
    public string ReplicationPendingItemsForVarnishing
    {
        get => _replicationPendingItemsForVarnishing;
        set
        {
            _replicationPendingItemsForVarnishing = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForFolding = "-";
    
    public string ReplicationPendingItemsForFolding
    {
        get => _replicationPendingItemsForFolding;
        set
        {
            _replicationPendingItemsForFolding = value;
            OnPropertyChanged();
        }
    }
    
    private string _replicationPendingItemsForFittings = "-";
    
    public string ReplicationPendingItemsForFittings
    {
        get => _replicationPendingItemsForFittings;
        set
        {
            _replicationPendingItemsForFittings = value;
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
    
    private string _replicationPendingFurnitureItemsWaitingTimeSeconds = "-";
    private string _replicationPendingFurnitureItemsWaitingTimeMinutes = "-";
    private string _replicationPendingFurnitureItemsWaitingTimeHours = "-";
    
    public string ReplicationPendingFurnitureItemsWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingFurnitureItemsWaitingTimeSeconds,
                "minutes" => _replicationPendingFurnitureItemsWaitingTimeMinutes,
                "hours" => _replicationPendingFurnitureItemsWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingFurnitureItemsWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingFurnitureItemsWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingFurnitureItemsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingFurnitureItemsWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingFurnitureItemsWaitingTime));
    }
    
    private string _replicationPendingItemsForStainingWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForStainingWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForStainingWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForStainingWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForStainingWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForStainingWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForStainingWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForStainingWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForStainingWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForStainingWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForStainingWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForStainingWaitingTime));
    }
    
    private string _replicationPendingItemsForVarnishingWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForVarnishingWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForVarnishingWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForVarnishingWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForVarnishingWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForVarnishingWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForVarnishingWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForVarnishingWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForVarnishingWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForVarnishingWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForVarnishingWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForVarnishingWaitingTime));
    }
    
    private string _replicationPendingItemsForFoldingWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForFoldingWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForFoldingWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForFoldingWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForFoldingWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForFoldingWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForFoldingWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForFoldingWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForFoldingWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForFoldingWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForFoldingWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForFoldingWaitingTime));
    }
    
    private string _replicationPendingItemsForFittingsWaitingTimeSeconds = "-";
    private string _replicationPendingItemsForFittingsWaitingTimeMinutes = "-";
    private string _replicationPendingItemsForFittingsWaitingTimeHours = "-";
    
    public string ReplicationPendingItemsForFittingsWaitingTime
    {
        get
        {
            return Shared.SelectedTimeUnits switch
            {
                "seconds" => _replicationPendingItemsForFittingsWaitingTimeSeconds,
                "minutes" => _replicationPendingItemsForFittingsWaitingTimeMinutes,
                "hours" => _replicationPendingItemsForFittingsWaitingTimeHours,
                _ => "-"
            };
        }
    }
    
    public void SetReplicationPendingItemsForFittingsWaitingTime(double averageTime)
    {
        if (double.IsNaN(averageTime))
        {
            averageTime = 0;
        }
        
        _replicationPendingItemsForFittingsWaitingTimeSeconds = $"{averageTime:F2}";
        _replicationPendingItemsForFittingsWaitingTimeMinutes = $"{averageTime / 60:F2}";
        _replicationPendingItemsForFittingsWaitingTimeHours = $"{averageTime / 3600:F2}";
        
        OnPropertyChanged(nameof(ReplicationPendingItemsForFittingsWaitingTime));
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
    
    
    private string _pendingFurnitureItemsQueueCount = "0";

    public string PendingFurnitureItemsQueueCount
    {
        get => _pendingFurnitureItemsQueueCount;
        set
        {
            _pendingFurnitureItemsQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingFurnitureItemsQueueTabTitle));
        }
    }
    
    public string PendingFurnitureItemsQueueTabTitle => $"Items ({PendingFurnitureItemsQueueCount})";
    
    
    private string _pendingForStainingQueueCount = "0";
    
    public string PendingForStainingQueueCount
    {
        get => _pendingForStainingQueueCount;
        set
        {
            _pendingForStainingQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingForStainingQueueTabTitle));
        }
    }
    public string PendingForStainingQueueTabTitle => $"for Staining ({PendingForStainingQueueCount})";
    
    
    private string _pendingForVarnishingQueueCount = "0";
    
    public string PendingForVarnishingQueueCount
    {
        get => _pendingForVarnishingQueueCount;
        set
        {
            _pendingForVarnishingQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingForVarnishingQueueTabTitle));
        }
    }
    
    public string PendingForVarnishingQueueTabTitle => $"for Varnishing ({PendingForVarnishingQueueCount})";
    
    
    private string _pendingForFoldingQueueCount = "0";
    
    public string PendingForFoldingQueueCount
    {
        get => _pendingForFoldingQueueCount;
        set
        {
            _pendingForFoldingQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingForFoldingQueueTabTitle));
        }
    }
    
    public string PendingForFoldingQueueTabTitle => $"for Folding ({PendingForFoldingQueueCount})";

    
    private string _pendingForFittingsQueueCount = "0";
    
    public string PendingForFittingsQueueCount
    {
        get => _pendingForFittingsQueueCount;
        set
        {
            _pendingForFittingsQueueCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PendingForFittingsQueueTabTitle));
        }
    }
    
    public string PendingForFittingsQueueTabTitle => $"for Fittings ({PendingForFittingsQueueCount})";
    
    private ObservableCollection<OrderDTO> _pendingOrdersQueue = [];

    public ObservableCollection<OrderDTO> PendingOrdersQueue
    {
        get => _pendingOrdersQueue;
        set {
            _pendingOrdersQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingFurnitureItemsQueue = [];

    public ObservableCollection<FurnitureDTO> PendingFurnitureItemsQueue
    {
        get => _pendingFurnitureItemsQueue;
        set {
            _pendingFurnitureItemsQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingForStainingQueue = [];

    public ObservableCollection<FurnitureDTO> PendingForStainingQueue
    {
        get => _pendingForStainingQueue;
        set {
            _pendingForStainingQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingForVarnishingQueue = [];

    public ObservableCollection<FurnitureDTO> PendingForVarnishingQueue
    {
        get => _pendingForVarnishingQueue;
        set {
            _pendingForVarnishingQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingForFoldingQueue = [];

    public ObservableCollection<FurnitureDTO> PendingForFoldingQueue
    {
        get => _pendingForFoldingQueue;
        set {
            _pendingForFoldingQueue = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<FurnitureDTO> _pendingForFittingsQueue = [];

    public ObservableCollection<FurnitureDTO> PendingForFittingsQueue
    {
        get => _pendingForFittingsQueue;
        set {
            _pendingForFittingsQueue = value;
            OnPropertyChanged();
        }
    }
    
    #endregion

}
