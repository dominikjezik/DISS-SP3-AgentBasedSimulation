using System.Windows.Media;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPAnimator;
using Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Furniture
{
    private readonly MySimulation _simulation;
    
    public int Id { get; set; }

    public Order Order { get; set; }

    public FurnitureType Type { get; set; }
    
    public bool NeedsToBeVarnished { get; set; }
    
    public AssemblyLine? CurrentAssemblyLine { get; set; }
    
    public Worker? CurrentWorker { get; set; }

    public string DisplayId => $"{Order.Id} [{Id}]";
    
    public double StartedWaitingTime { get; set; }
    
    private FurnitureOperationStep _currentOperationStep = FurnitureOperationStep.NotStarted;

    public FurnitureOperationStep CurrentOperationStep
    {
        get => _currentOperationStep;
        set
        {
            _currentOperationStep = value;
            UpdateFurnitureStateAnimationText();
            UpdateFurnitureItemInOrderState();
        }
    }

    public double OperationStepEndTime { get; set; }
    
    #region Animation
    
    public const int FurnitureWidth = 68;
    
    private float _furnitureOriginX;
    
    private float _furnitureOriginY;

    private AnimImageItem AnimObject { get; set; }
    
    private AnimTextItem AnimFurnitureIdText { get; set; }
    
    private AnimTextItem AnimFurnitureTypeText { get; set; }
    
    private AnimTextItem AnimFurnitureStateText { get; set; }
    
    public const int FurnitureItemInOrderWidth = 54;
    
    #endregion
    
    #region AnimationFurnitureItemInOrder
    
    private AnimImageItem AnimFurnitureItemInOrderObject { get; set; }
    
    #endregion

    public Furniture(OSPABA.Simulation simulation)
    {
        _simulation = (MySimulation)simulation;

        _simulation.AnimatorCreated += InitializeAnimationObjects;
        _simulation.AnimatorRemoved += RemoveAnimationObjects;
    }

    private void InitializeAnimationObjects()
    {
        if (CurrentOperationStep != FurnitureOperationStep.MaterialPrepared && CurrentOperationStep != FurnitureOperationStep.NotStarted)
        {
            InitializeFurnitureAnimationObjects();
        }
    }

    private void InitializeFurnitureAnimationObjects()
    {
        if (CurrentAssemblyLine != null)
        {
            _furnitureOriginX = CurrentAssemblyLine.Column * (AssemblyLine.AssemblyLineWidth + AssemblyLine.XGapBetweenAssemblyLines) + 12;
            _furnitureOriginY = CurrentAssemblyLine.Row * (AssemblyLine.AssemblyLineHeight + AssemblyLine.YGapBetweenAssemblyLines) + 24 + 12;
        }
        else if (CurrentWorker != null && CurrentWorker.WarehouseSection != null)
        {
            _furnitureOriginX = CurrentWorker.WarehouseSection.SectionOriginX + 198;
            _furnitureOriginY = CurrentWorker.WarehouseSection.SectionOriginY + 22;
        }
        else
        {
            return;
        }

        AnimObject = new AnimImageItem(Config.ImageFurnitureItem);
        AnimObject.SetImageSize(FurnitureWidth, FurnitureWidth);
        AnimObject.SetPosition(_furnitureOriginX, _furnitureOriginY);
        
        AnimFurnitureIdText = new AnimTextItem(
            DisplayId,
            System.Windows.Media.Color.FromRgb(60, 60, 60),
            new Typeface("Arial"),
            16
        );
        AnimFurnitureIdText.SetPosition(
            _furnitureOriginX + 10, 
            _furnitureOriginY + 15
        );
        
        AnimFurnitureTypeText = new AnimTextItem(
            Type.ToString(),
            System.Windows.Media.Color.FromRgb(60, 60, 60),
            new Typeface("Arial"),
            16
        );
        AnimFurnitureTypeText.SetPosition(
            _furnitureOriginX + (Type.ToString().Length < 6 ? 14 : 10), 
            _furnitureOriginY + 35
        );
        
        DisplayFurnitureStateAnimationText();

        if (_simulation.AnimatorExists)
        {
            _simulation.Animator.Register(AnimObject);
            _simulation.Animator.Register(AnimFurnitureIdText);
            _simulation.Animator.Register(AnimFurnitureTypeText);
            _simulation.Animator.Register(AnimFurnitureStateText);
        }
    }

    public void DisplayFurnitureAnimationObject()
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }

        InitializeFurnitureAnimationObjects();
    }

    public void UpdateFurnitureStateAnimationText()
    {
        if (_simulation.AnimatorExists && AnimFurnitureStateText != null)
        {
            AnimFurnitureStateText.Remove();
            
            DisplayFurnitureStateAnimationText();
            
            _simulation.Animator.Register(AnimFurnitureStateText);
        }
    }
    
    public void RemoveFurnitureAnimationObject()
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        if (AnimObject != null)
        {
            _simulation.Animator.Remove(AnimObject);
        }
            
        if (AnimFurnitureIdText != null)
        {
            _simulation.Animator.Remove(AnimFurnitureIdText);
        }
            
        if (AnimFurnitureTypeText != null)
        {
            _simulation.Animator.Remove(AnimFurnitureTypeText);
        }
            
        if (AnimFurnitureStateText != null)
        {
            _simulation.Animator.Remove(AnimFurnitureStateText);
        }
    }

    private void DisplayFurnitureStateAnimationText()
    {
        var text = CurrentOperationStep.ToString();

        if (text == FurnitureOperationStep.AssemblingFittings.ToString())
        {
            text = "Fittings";
        }
        else if (text == FurnitureOperationStep.MaterialPreparationInWarehouse.ToString())
        {
            text = string.Empty;
        }
        
        AnimFurnitureStateText = new AnimTextItem(
            text, 
            System.Windows.Media.Color.FromRgb(60, 60, 60), 
            new Typeface("Arial"), 
            16
        );
        
        var offset = 8 - text.Length;
        
        AnimFurnitureStateText.SetPosition(
            _furnitureOriginX + 72 + offset * 2, 
            _furnitureOriginY + 45
        );
    }
    
    private void RemoveAnimationObjects()
    {
        if (AnimObject != null)
        {
            AnimObject.Remove();
        }
        
        if (AnimFurnitureIdText != null)
        {
            AnimFurnitureIdText.Remove();
        }
        
        if (AnimFurnitureTypeText != null)
        {
            AnimFurnitureTypeText.Remove();
        }
        
        if (AnimFurnitureStateText != null)
        {
            AnimFurnitureStateText.Remove();
        }
    }

    private void UpdateFurnitureItemInOrderState()
    {
        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        if (CurrentOperationStep == FurnitureOperationStep.MaterialPreparationInWarehouse || CurrentOperationStep == FurnitureOperationStep.Completed)
        {
            if (CurrentOperationStep == FurnitureOperationStep.MaterialPreparationInWarehouse)
            {
                AnimFurnitureItemInOrderObject = new AnimImageItem(Config.ImageFurnitureItemInProgress);
            }
            else if (CurrentOperationStep == FurnitureOperationStep.Completed)
            {
                AnimFurnitureItemInOrderObject = new AnimImageItem(Config.ImageFurnitureItemCompleted);
            }
        
            AnimFurnitureItemInOrderObject.SetImageSize(FurnitureItemInOrderWidth, FurnitureItemInOrderWidth);
            AnimFurnitureItemInOrderObject.SetPosition(
                Order.OrderOriginX + (Id - 1) * (FurnitureItemInOrderWidth + 5) + 12,
                Order.OrderOriginY + 30
            );
            AnimFurnitureItemInOrderObject.ZIndex = 2;

            if (_simulation.AnimatorExists)
            {
                _simulation.Animator.Register(AnimFurnitureItemInOrderObject);
            }
        }
    }

    public void InitializeFurnitureItemInOrderAnimationObjects()
    {
        if (!_simulation.AnimatorExists || Order == null)
        {
            return;
        }
        
        if (CurrentOperationStep == FurnitureOperationStep.NotStarted)
        {
            AnimFurnitureItemInOrderObject = new AnimImageItem(Config.ImageFurnitureItemPending);
        }
        else if (CurrentOperationStep == FurnitureOperationStep.Completed)
        {
            AnimFurnitureItemInOrderObject = new AnimImageItem(Config.ImageFurnitureItemCompleted);
        }
        else
        {
            AnimFurnitureItemInOrderObject = new AnimImageItem(Config.ImageFurnitureItemInProgress);
        }
        
        AnimFurnitureItemInOrderObject.SetImageSize(FurnitureItemInOrderWidth, FurnitureItemInOrderWidth);
        AnimFurnitureItemInOrderObject.SetPosition(
            Order.OrderOriginX + (Id - 1) * (FurnitureItemInOrderWidth + 5) + 12,
            Order.OrderOriginY + 30
        );
        AnimFurnitureItemInOrderObject.ZIndex = 2;

        if (_simulation.AnimatorExists)
        {
            _simulation.Animator.Register(AnimFurnitureItemInOrderObject);
        }
    }
    
}
