using System.Windows.Media;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPAnimator;
using Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Order
{
    public const int OrderWidth = 314;
    
    public const int OrderHeight = 94;
    
    public const int OrdersQueueXOrigin = 1600;
    
    public const int OrdersQueueYOrigin = 24;
    
    private readonly MySimulation _simulation;
    
    public int Id { get; set; }
    
    public OrderState State { get; set; } = OrderState.Pending;
    
    public List<Furniture> FurnitureItems { get; set; } = new();

    public int FinishedFurnitureItemsCount { get; set; } = 0;
    
    public double ArrivalTime { get; set; }
    
    public double StartedWaitingTime { get; set; }
    
    private int _orderPosition = 0;
    
    private AnimImageItem AnimOrderObject { get; set; }
    
    private AnimTextItem AnimOrderText { get; set; }
    
    public float OrderOriginX { get; private set; }
    
    public float OrderOriginY { get; private set; }

    
    public Order(OSPABA.Simulation simulation)
    {
        _simulation = (MySimulation)simulation;

        _simulation.AnimatorCreated += InitializeAnimationObjects;
        _simulation.AnimatorRemoved += RemoveAnimationObjects;
    }

    private void InitializeAnimationObjects()
    {
        if (State == OrderState.Completed)
        {
            return;
        }
        
        OrderOriginX = OrdersQueueXOrigin;
        OrderOriginY = OrdersQueueYOrigin + _orderPosition * (OrderHeight + 10);
        
        AnimOrderObject = new AnimImageItem(Config.ImageOrder);
        AnimOrderObject.SetImageSize(OrderWidth, OrderHeight);
        AnimOrderObject.SetPosition(OrderOriginX, OrderOriginY);
        
        AnimOrderText = new AnimTextItem(
            $"#{Id} - {State}",
            System.Windows.Media.Color.FromRgb(60, 60, 60),
            new Typeface("Arial"),
            16
        );
        AnimOrderText.SetPosition(
            OrderOriginX + 15, 
            OrderOriginY + 10
        );
        
        if (_simulation.AnimatorExists)
        {
            _simulation.Animator.Register(AnimOrderObject);
            _simulation.Animator.Register(AnimOrderText);
        }

        foreach (var furniture in FurnitureItems)
        {
            furniture.InitializeFurnitureItemInOrderAnimationObjects();
        }
    }

    public void SetOrderPosition(int position)
    {
        _orderPosition = position;

        if (!_simulation.AnimatorExists)
        {
            return;
        }
        
        InitializeAnimationObjects();
    }

    private void RemoveAnimationObjects()
    {
        if (AnimOrderObject != null)
        {
            AnimOrderObject.Remove();
        }
        
        if (AnimOrderText != null)
        {
            AnimOrderText.Remove();
        }
    }
}
