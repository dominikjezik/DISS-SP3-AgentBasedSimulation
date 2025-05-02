using System.Windows.Media;
using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPAnimator;
using Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Worker
{
    public const int WorkerWidth = 44;
    
    private readonly MySimulation _simulation;
    
    private WeightedStatistics _utilizationStatistics = new();
    
    private double _lastChangeInUtilizationTime = 0;
    
    private Furniture? _currentFurniture;
    
    private AnimTextItem AnimTextTitle { get; set; }
    
    private AnimImageItem AnimObject { get; set; }

    private int _textOffsetX;
    
    public WarehouseSection? WarehouseSection { get; set; }
    
    public int Id { get; private set; }
    
    public WorkerGroup Group { get; private set; }
    
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

    public bool IsInWarehouse { get; set; } = true;
    
    public bool IsMovingToWarehouse { get; set; }
    
    public AssemblyLine? CurrentAssemblyLine { get; set; }
    
    public bool IsMovingToAssemblyLine { get; set; }
    
    public double Utilization => double.IsNaN(_utilizationStatistics.Mean) ? 0 : _utilizationStatistics.Mean;
    
    public Worker(OSPABA.Simulation simulation, int id, WorkerGroup group)
    {
        _simulation = (MySimulation)simulation;
        Id = id;
        Group = group;
        
        _simulation.AnimatorCreated += InitializeAnimationObjects;
        _simulation.AnimatorRemoved += RemoveAnimationObjects;
    }

    private void InitializeAnimationObjects()
    {
        int zIndex = 10;
        
        if (Group == WorkerGroup.GroupA)
        {
            AnimObject = new AnimImageItem(Config.ImageWorkerA);
            zIndex += 2 * Id;
        }
        else if (Group == WorkerGroup.GroupB)
        {
            AnimObject = new AnimImageItem(Config.ImageWorkerB);
            zIndex += _simulation.CountOfWorkersGroupB * 2 + 2 * Id;
        }
        else
        {
            AnimObject = new AnimImageItem(Config.ImageWorkerC);
            zIndex += _simulation.CountOfWorkersGroupB * 2 + _simulation.CountOfWorkersGroupC * 2 + 2 * Id;
        }
        
        var workerPosition = GetCurrentWorkerPositionForAnimationObjects();
        AnimObject.SetPosition(workerPosition);
        AnimObject.SetImageSize(WorkerWidth, WorkerWidth);
        AnimObject.ZIndex = zIndex;
        
        AnimTextTitle = new AnimTextItem(
            DisplayId,
            System.Windows.Media.Color.FromRgb(60, 60, 60),
            new Typeface("Arial"),
            14
        );
        
        _textOffsetX = DisplayId.Length > 2 ? 12 : 9;
        
        AnimTextTitle.SetPosition(
            workerPosition.X + WorkerWidth / 2.0f - _textOffsetX,
            workerPosition.Y + WorkerWidth / 2.0f - 9
        );
        AnimTextTitle.ZIndex = zIndex + 1;

        if (_simulation.AnimatorExists)
        {
            _simulation.Animator.Register(AnimObject);
            _simulation.Animator.Register(AnimTextTitle);
        }
    }

    private PointF GetCurrentWorkerPositionForAnimationObjects()
    {
        if (CurrentAssemblyLine != null)
        {
            if (CurrentAssemblyLine.CurrentWorker == this)
            {
                return CurrentAssemblyLine.CurrentWorkerPosition;
            }
            else
            {
                if (Group == WorkerGroup.GroupA)
                {
                    return CurrentAssemblyLine.IdleWorkersAPosition;
                }

                if (Group == WorkerGroup.GroupB)
                {
                    return CurrentAssemblyLine.IdleWorkersBPosition;
                }

                return CurrentAssemblyLine.IdleWorkersCPosition;
            }
        }
        
        if (IsInWarehouse && !IsMovingToAssemblyLine)
        {
            if (Group == WorkerGroup.GroupA)
            {
                return _simulation.ManufacturerAgent.Warehouse.WarehouseSections[Id - 1].CurrentWorkerPosition;
            }

            if (Group == WorkerGroup.GroupB)
            {
                return _simulation.ManufacturerAgent.Warehouse.WorkersGroupBIdlePosition;
            }

            return _simulation.ManufacturerAgent.Warehouse.WorkersGroupCIdlePosition;
        }
        
        // V tomto prípade sa worker momentálne presúva
        return new PointF(_simulation.ManufacturerAgent.Warehouse.WarehouseOriginX + 300, _simulation.ManufacturerAgent.Warehouse.WarehouseOriginY);
    }
    
    public void RefreshStatistics()
    {
        var timeInterval = _simulation.CurrentTime - _lastChangeInUtilizationTime;
        
        _utilizationStatistics.AddValue(CurrentFurniture != null ? 1 : 0, timeInterval);
        
        _lastChangeInUtilizationTime = _simulation.CurrentTime;
    }
    
    public void AnimateTransfer(double startTime, double duration, PointF[] path)
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        AnimObject.StartAnim(startTime, duration, path);
        
        var textPath = new PointF[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            textPath[i] = new PointF(path[i].X + WorkerWidth / 2.0f - _textOffsetX, path[i].Y + WorkerWidth / 2.0f - 9);
        }
        
        AnimTextTitle.StartAnim(startTime, duration, textPath);
    }

    public void PlaceWorker(float x, float y)
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        AnimObject.StartAnim(
            _simulation.CurrentTime, 
            0, 
            new[] {new PointF(x, y)}
        );
        
        AnimTextTitle.StartAnim(
            _simulation.CurrentTime, 
            0, 
            new[] {new PointF(x + WorkerWidth / 2.0f - _textOffsetX, y + WorkerWidth / 2.0f - 9)}
        );
    }
    
    public void PlaceWorker(PointF position)
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        AnimObject.StartAnim(
            _simulation.CurrentTime, 
            0, 
            new[] {position}
        );
        
        AnimTextTitle.StartAnim(
            _simulation.CurrentTime, 
            0, 
            new[] {new PointF(position.X + WorkerWidth / 2.0f - _textOffsetX, position.Y + WorkerWidth / 2.0f - 9)}
        );
    }
    
    private void RemoveAnimationObjects()
    {
        if (AnimObject != null)
        {
            AnimObject.Remove();
        }
        
        if (AnimTextTitle != null)
        {
            AnimTextTitle.Remove();
        }
    }
}
