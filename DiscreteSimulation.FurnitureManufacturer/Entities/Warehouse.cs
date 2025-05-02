using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPAnimator;
using Simulation;

namespace DiscreteSimulation.FurnitureManufacturer.Entities;

public class Warehouse
{
	private MySimulation _simulation;
	
	public float WarehouseOriginX => (AssemblyLine.NumberOfAssemblyLinesInRow + 1) * (AssemblyLine.AssemblyLineWidth + AssemblyLine.XGapBetweenAssemblyLines);
	
	public float WarehouseOriginY => 0;
	
	public AnimTextItem AnimTextTitle { get; private set; }
	
	public AnimImageItem AnimObjectTop { get; private set; }
	
	public WarehouseSection[] WarehouseSections { get; private set; }
	
	public AnimImageItem AnimObjectBottom { get; private set; }
	
	public PointF WorkersGroupBIdlePosition { get; private set; }
	
	public PointF WorkersGroupCIdlePosition { get; private set; }

	public PointF GatewayPositionIdleBCWorkers { get; private set; }

    public PointF GatewayPosition { get; private set; }
    
    public PointF CrossroadPosition { get; private set; }
    
    public Warehouse(OSPABA.Simulation simulation)
    {
	    _simulation = (MySimulation)simulation;
	    
	    WorkersGroupBIdlePosition = new PointF(
		    WarehouseOriginX + 170 - Worker.WorkerWidth / 2.0f,
		    WarehouseOriginY + 24 + 12 + _simulation.CountOfWorkersGroupA * 110 + 30 - Worker.WorkerWidth / 2.0f
	    );
		
	    WorkersGroupCIdlePosition = new PointF(
		    WarehouseOriginX + 230 - Worker.WorkerWidth / 2.0f,
		    WarehouseOriginY + 24 + 12 + _simulation.CountOfWorkersGroupA * 110 + 30 - Worker.WorkerWidth / 2.0f
	    );

	    GatewayPositionIdleBCWorkers = new PointF(
		    WarehouseOriginX + 60 - Worker.WorkerWidth / 2.0f,
		    WarehouseOriginY + 24 + 12 + _simulation.CountOfWorkersGroupA * 110 + 30 - Worker.WorkerWidth / 2.0f
	    );
	    
	    GatewayPosition = new PointF(
		    WarehouseOriginX + 60 - Worker.WorkerWidth / 2.0f,
		    WarehouseOriginY + 24 + 69 - Worker.WorkerWidth / 2.0f
	    );
        
	    CrossroadPosition = new PointF(
		    (AssemblyLine.NumberOfAssemblyLinesInRow) * (AssemblyLine.AssemblyLineWidth + AssemblyLine.XGapBetweenAssemblyLines) + AssemblyLine.AssemblyLineWidth / 2.0f - Worker.WorkerWidth / 2.0f,
		    WarehouseOriginY + 24 + 69 - Worker.WorkerWidth / 2.0f
	    );
	    
	    // Pre každého workera typu A sa vytvorí sekcia v sklade
	    WarehouseSections = new WarehouseSection[_simulation.CountOfWorkersGroupA];
	    
	    for (int i = 0; i < _simulation.CountOfWorkersGroupA; i++)
	    {
		    WarehouseSections[i] = new WarehouseSection(
			    _simulation, 
			    WarehouseOriginX, 
			    WarehouseOriginY + 24 + 12 + i * 110
		    );
			
		    var section = WarehouseSections[_simulation.WorkersGroupAAgent.Workers[i].Id - 1];
		    _simulation.WorkersGroupAAgent.Workers[i].WarehouseSection = section;
	    }
	    
	    _simulation.AnimatorCreated += InitializeAnimationObjects;
	    _simulation.AnimatorRemoved += RemoveAnimationObjects;
    }

    private void InitializeAnimationObjects()
    {
	    AnimTextTitle = new AnimTextItem("Warehouse");
	    AnimTextTitle.Size = 14;
	    AnimTextTitle.SetPosition(WarehouseOriginX, WarehouseOriginY);

	    AnimObjectTop = new AnimImageItem(Config.ImageWarehouseTop);
	    AnimObjectTop.SetImageSize(302, 12);
	    AnimObjectTop.SetPosition(WarehouseOriginX, WarehouseOriginY + 24);
		
		AnimObjectBottom = new AnimImageItem(Config.ImageWarehouseBottom);
		AnimObjectBottom.SetImageSize(302, 73);
		AnimObjectBottom.SetPosition(WarehouseOriginX, WarehouseOriginY + 24 + 12 + _simulation.CountOfWorkersGroupA * 110);

		if (_simulation.AnimatorExists)
		{
			_simulation.Animator.Register(AnimTextTitle);
			_simulation.Animator.Register(AnimObjectTop);
			_simulation.Animator.Register(AnimObjectBottom);
		}
    }
    
    private void RemoveAnimationObjects()
	{
		AnimTextTitle.Remove();
		AnimObjectTop.Remove();
		AnimObjectBottom.Remove();
	}
}
