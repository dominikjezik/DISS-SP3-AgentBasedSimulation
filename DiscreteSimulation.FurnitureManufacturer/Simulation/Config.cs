namespace Simulation;

public class Config
{
    public static readonly Image ImageAssemblyLine = Image.FromFile(ImagePathAssemblyLine);
    public static readonly Image ImageWorkerA = Image.FromFile(ImagePathWorkerA);
    public static readonly Image ImageWorkerB = Image.FromFile(ImagePathWorkerB);
    public static readonly Image ImageWorkerC = Image.FromFile(ImagePathWorkerC);
    public static readonly Image ImageFurnitureItem = Image.FromFile(ImagePathFurnitureItem);
    public static readonly Image ImageWarehouseTop = Image.FromFile(ImagePathWarehouseTop);
    public static readonly Image ImageWarehouseSection = Image.FromFile(ImagePathWarehouseSection);
    public static readonly Image ImageWarehouseBottom = Image.FromFile(ImagePathWarehouseBottom);
    public static readonly Image ImageWarehouseLine = Image.FromFile(ImagePathWarehouseLine);
    public static readonly Image ImageOrder = Image.FromFile(ImagePathOrder);
    public static readonly Image ImageFurnitureItemPending = Image.FromFile(ImagePathFurnitureItemPending);
    public static readonly Image ImageFurnitureItemInProgress = Image.FromFile(ImagePathFurnitureItemInProgress);
    public static readonly Image ImageFurnitureItemCompleted = Image.FromFile(ImagePathFurnitureItemCompleted);
    
    private const string ImagePathAssemblyLine = "AnimatorImages/AssemblyLine.png";
    private const string ImagePathWorkerA = "AnimatorImages/WorkerA.png";
    private const string ImagePathWorkerB = "AnimatorImages/WorkerB.png";
    private const string ImagePathWorkerC = "AnimatorImages/WorkerC.png";
    private const string ImagePathFurnitureItem = "AnimatorImages/FurnitureItem.png";
    private const string ImagePathWarehouseTop = "AnimatorImages/WarehouseTop.png";
    private const string ImagePathWarehouseSection = "AnimatorImages/WarehouseSection.png";
    private const string ImagePathWarehouseBottom = "AnimatorImages/WarehouseBottom.png";
    private const string ImagePathWarehouseLine = "AnimatorImages/WarehouseLine.png";
    private const string ImagePathOrder = "AnimatorImages/Order.png";
    private const string ImagePathFurnitureItemPending = "AnimatorImages/FurnitureItemPending.png";
    private const string ImagePathFurnitureItemInProgress = "AnimatorImages/FurnitureItemInProgress.png";
    private const string ImagePathFurnitureItemCompleted = "AnimatorImages/FurnitureItemCompleted.png";
}
