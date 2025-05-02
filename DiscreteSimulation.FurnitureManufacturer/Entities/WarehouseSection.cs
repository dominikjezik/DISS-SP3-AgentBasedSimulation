using OSPAnimator;
using Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class WarehouseSection
{
    private readonly MySimulation _simulation;
    
    public const int LineWidth = 154;
    
    public const int LineHeight = 94;
    
    public float SectionOriginX { get; private set; }
    
    public float SectionOriginY { get; private set; }
    
    public AnimImageItem AnimObjectSection { get; private set; }
    
    private AnimImageItem AnimObjectLine { get; set; }
    
    private AnimShapeItem AnimProgressBar { get; set; }
    
    private AnimShapeItem AnimProgressBarMask { get; set; }
    
    public PointF CurrentWorkerPosition { get; private set; }
    
    public PointF GatewayPosition { get; private set; }

    public WarehouseSection(OSPABA.Simulation simulation, float sectionOriginX, float sectionOriginY)
    {
        _simulation = (MySimulation)simulation;
        SectionOriginX = sectionOriginX;
        SectionOriginY = sectionOriginY;
        
        GatewayPosition = new PointF(
            SectionOriginX + 60 - Worker.WorkerWidth / 2.0f,
            SectionOriginY + (LineHeight / 2.0f) + 10 - Worker.WorkerWidth / 2.0f
        );
        
        CurrentWorkerPosition = new PointF(
            SectionOriginX + 160 - Worker.WorkerWidth / 2.0f,
            SectionOriginY + (LineHeight / 2.0f) + 10 - Worker.WorkerWidth / 2.0f
        );
        
        _simulation.AnimatorCreated += InitializeAnimationObjects;
        _simulation.AnimatorRemoved += RemoveAnimationObjects;
    }
    
    private void InitializeAnimationObjects()
    {
        AnimObjectSection = new AnimImageItem(Config.ImageWarehouseSection);
        AnimObjectSection.SetImageSize(302, 110);
        AnimObjectSection.SetPosition(SectionOriginX, SectionOriginY);
        
        AnimObjectLine = new AnimImageItem(Config.ImageWarehouseLine);
        AnimObjectLine.SetImageSize(LineWidth, LineHeight);
        AnimObjectLine.SetPosition(SectionOriginX + 125, SectionOriginY + 10);

        if (_simulation.AnimatorExists)
        {
            _simulation.Animator.Register(AnimObjectSection);
            _simulation.Animator.Register(AnimObjectLine);
        }
    }

    public void PlaceWorker(Worker worker)
    {
        worker.PlaceWorker(
            CurrentWorkerPosition.X,
            CurrentWorkerPosition.Y
        );
    }
    
    public void AnimatePreparationStep(double duration)
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        DisplayOperationStepProgressBar();
        
        AnimProgressBarMask.MoveTo(
            _simulation.CurrentTime, 
            duration,
            SectionOriginX + 125 + 10,
            SectionOriginY + 4 - 1
        );
    }
    
    private void DisplayOperationStepProgressBar()
    {
        AnimProgressBar = new AnimShapeItem(AnimShape.RECTANGLE, System.Windows.Media.Color.FromRgb(130, 237, 130), 10);
        AnimProgressBar.SetHeight(5);
        AnimProgressBar.SetWidth(LineWidth - 20);
        AnimProgressBar.ZIndex = -2;
        
        AnimProgressBarMask = new AnimShapeItem(AnimShape.RECTANGLE, System.Windows.Media.Color.FromRgb(255, 255, 255), 10);
        AnimProgressBarMask.SetHeight(7);
        AnimProgressBarMask.SetWidth(LineWidth - 20);
        AnimProgressBarMask.ZIndex = -1;
        
        AnimProgressBar.SetPosition(
            SectionOriginX + 125 + 10,
            SectionOriginY + 4
        );
        
        AnimProgressBarMask.SetPosition(
            SectionOriginX + 125 + 10 + (LineWidth - 20),
            SectionOriginY + 4 - 1
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
        if (AnimObjectSection != null)
        {
            AnimObjectSection.Remove();
        }
        
        if (AnimObjectLine != null)
        {
            AnimObjectLine.Remove();
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
