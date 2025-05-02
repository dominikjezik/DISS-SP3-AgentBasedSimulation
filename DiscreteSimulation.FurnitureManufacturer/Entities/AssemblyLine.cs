using System.Drawing;
using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPAnimator;
using Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class AssemblyLine
{
    private readonly MySimulation _simulation;
    
    private WeightedStatistics _utilizationStatistics = new();
    
    private double _lastChangeInUtilizationTime = 0;
    
    private Furniture? _currentFurniture;

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
    
    #region Animation
    
    public const int NumberOfAssemblyLinesInRow = 4;
    public const int AssemblyLineWidth = 154;
    public const int AssemblyLineHeight = 94;
    public const int XGapBetweenAssemblyLines = 80;
    public const int YGapBetweenAssemblyLines = 200;
    
    public int Row => (Id - 1) / NumberOfAssemblyLinesInRow;
    
    public int Column => (Id - 1) % NumberOfAssemblyLinesInRow;
    
    public float AssemblyLineOriginX => Column * (AssemblyLineWidth + XGapBetweenAssemblyLines);
    
    public float AssemblyLineOriginY => Row * (AssemblyLineHeight + YGapBetweenAssemblyLines);
    
    private AnimTextItem AnimTextTitle { get; set; }
    
    private AnimImageItem AnimObject { get; set; }
    
    private AnimShapeItem AnimProgressBar { get; set; }
    
    private AnimShapeItem AnimProgressBarMask { get; set; }
    
    public PointF CurrentWorkerPosition { get; private set; }
    
    public PointF IdleWorkersAPosition { get; private set; }
    
    public PointF IdleWorkersBPosition { get; private set; }
    
    public PointF IdleWorkersCPosition { get; private set; }
    
    public PointF EntrancePosition { get; private set; }
    
    public PointF GatewayPosition { get; private set; }
    
    public PointF CrossroadPosition { get; private set; }
    
    #endregion
    
    public AssemblyLine(OSPABA.Simulation simulation, int id)
    {
        _simulation = (MySimulation)simulation;
        Id = id;
        
        CurrentWorkerPosition = new PointF(
            AssemblyLineOriginX + AssemblyLineWidth / 2.0f + 36 - Worker.WorkerWidth / 2.0f, 
            AssemblyLineOriginY + 24 + AssemblyLineHeight / 2.0f - 12 - Worker.WorkerWidth / 2.0f
        );
        
        IdleWorkersAPosition = new PointF(
            AssemblyLineOriginX + 30 - Worker.WorkerWidth / 2.0f, 
            AssemblyLineOriginY + 24 + AssemblyLineHeight + 30 - Worker.WorkerWidth / 2.0f
        );
        
        IdleWorkersBPosition = new PointF(
            AssemblyLineOriginX + 75 - Worker.WorkerWidth / 2.0f, 
            AssemblyLineOriginY + 24 + AssemblyLineHeight + 30 - Worker.WorkerWidth / 2.0f
        );
        
        IdleWorkersCPosition = new PointF(
            AssemblyLineOriginX + 120 - Worker.WorkerWidth / 2.0f, 
            AssemblyLineOriginY + 24 + AssemblyLineHeight + 30 - Worker.WorkerWidth / 2.0f
        );
        
        EntrancePosition  = new PointF(
            AssemblyLineOriginX + AssemblyLineWidth / 2.0f + 110 - Worker.WorkerWidth / 2.0f, 
            AssemblyLineOriginY + 24 + AssemblyLineHeight / 2.0f - 12 - Worker.WorkerWidth / 2.0f
        );
        
        GatewayPosition = new PointF(
            AssemblyLineOriginX + AssemblyLineWidth / 2.0f + 110 - Worker.WorkerWidth / 2.0f, 
            AssemblyLineOriginY + 24 + AssemblyLineHeight + 70 - Worker.WorkerWidth / 2.0f
        );
        
        CrossroadPosition = new PointF(
            (NumberOfAssemblyLinesInRow) * (AssemblyLineWidth + XGapBetweenAssemblyLines) + AssemblyLineWidth / 2.0f - Worker.WorkerWidth / 2.0f, 
            AssemblyLineOriginY + 24 + AssemblyLineHeight + 70 - Worker.WorkerWidth / 2.0f
        );
        
        _simulation.AnimatorCreated += InitializeAnimationObjects;
        _simulation.AnimatorRemoved += RemoveAnimationObjects;
    }

    private void InitializeAnimationObjects()
    {
        AnimTextTitle = new AnimTextItem($"Assembly line no. {Id}");
        AnimTextTitle.Size = 14;
        AnimTextTitle.SetPosition(AssemblyLineOriginX + 10, AssemblyLineOriginY);
        
        AnimObject = new AnimImageItem(Config.ImageAssemblyLine);
        AnimObject.SetImageSize(AssemblyLineWidth, AssemblyLineHeight);
        AnimObject.SetPosition(
            AssemblyLineOriginX, 
            AssemblyLineOriginY + 24
        );
        
        if (_simulation.AnimatorExists)
        {
            _simulation.Animator.Register(AnimTextTitle);
            _simulation.Animator.Register(AnimObject);
        }
    }
    
    public void RefreshStatistics()
    {
        var timeInterval = _simulation.CurrentTime - _lastChangeInUtilizationTime;
        
        _utilizationStatistics.AddValue(CurrentFurniture != null ? 1 : 0, timeInterval);
        
        _lastChangeInUtilizationTime = _simulation.CurrentTime;
    }
    
    public void SetCurrentWorkerToIdle(Worker worker)
    {
        worker.CurrentAssemblyLine.IdleWorkers.Add(worker);
        worker.CurrentAssemblyLine.CurrentWorker = null;
        
        if (worker.Group == WorkerGroup.GroupA)
        {
            worker.PlaceWorker(IdleWorkersAPosition.X, IdleWorkersAPosition.Y);
        }
        else if (worker.Group == WorkerGroup.GroupB)
        {
            worker.PlaceWorker(IdleWorkersBPosition.X, IdleWorkersBPosition.Y);
        }
        else if (worker.Group == WorkerGroup.GroupC)
        {
            worker.PlaceWorker(IdleWorkersCPosition.X, IdleWorkersCPosition.Y);
        }
    }

    public void AnimateOperationStep(double duration)
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        DisplayOperationStepProgressBar();
        
        AnimProgressBarMask.MoveTo(
            _simulation.CurrentTime, 
            duration,
            AssemblyLineOriginX + 10,
            AssemblyLineOriginY + 18
        );
    }
    
    private void DisplayOperationStepProgressBar()
    {
        var mod = Id % NumberOfAssemblyLinesInRow;
        
        if (mod == 0) 
        {
            mod = NumberOfAssemblyLinesInRow;
        }
        
        AnimProgressBar = new AnimShapeItem(AnimShape.RECTANGLE, System.Windows.Media.Color.FromRgb(130, 237, 130), 10);
        AnimProgressBar.SetHeight(5);
        AnimProgressBar.SetWidth(AssemblyLineWidth - 20);
        AnimProgressBar.ZIndex = mod * 2 - 1;
        
        AnimProgressBarMask = new AnimShapeItem(AnimShape.RECTANGLE, System.Windows.Media.Color.FromRgb(255, 255, 255), 10);
        AnimProgressBarMask.SetHeight(7);
        AnimProgressBarMask.SetWidth(AssemblyLineWidth - 20);
        AnimProgressBarMask.ZIndex = mod * 2;
        
        AnimProgressBar.SetPosition(
            AssemblyLineOriginX + 10,
            AssemblyLineOriginY + 18
        );
        
        AnimProgressBarMask.SetPosition(
            AssemblyLineOriginX + 10 + (AssemblyLineWidth - 20),
            AssemblyLineOriginY + 18 - 1
        );
        
        _simulation.Animator.Register(AnimProgressBar);
        _simulation.Animator.Register(AnimProgressBarMask);
    }
    
    public void HideOperationStepProgressBar()
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }

        if (AnimProgressBar != null)
        {
            AnimProgressBar.Remove();
        }
        
        if (AnimProgressBarMask != null)
        {
            AnimProgressBarMask.Remove();
        }
    }

    private void RemoveAnimationObjects()
    {
        if (AnimTextTitle != null)
        {
            AnimTextTitle.Remove();
        }
        
        if (AnimObject != null)
        {
            AnimObject.Remove();
        }
        
        if (AnimProgressBar != null)
        {
            AnimProgressBar.Remove();
        }
        
        if (AnimProgressBarMask != null)
        {
            AnimProgressBarMask.Remove();
        }
    }
}
