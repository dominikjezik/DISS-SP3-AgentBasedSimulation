using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using OSPABA;
using Simulation;

namespace Agents.ManufacturerAgent
{
	//meta! id="4"
	public class ManufacturerAgent : OSPABA.Agent
	{
		public List<Order> AllOrders { get; private set; } = new();
		
		public LinkedList<MyMessage> UnfinishedOrders { get; private set; } = new();
		
		public EntitiesQueue<Order> PendingOrders { get; private set; }
		
		public Statistics PendingOrdersWaitingTime { get; private set; } = new();
		
		public Statistics ProcessingFurnitureTime { get; private set; } = new();
		
		public ManufacturerAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			PendingOrders = new EntitiesQueue<Order>(MySim);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			
			AllOrders.Clear();
			UnfinishedOrders.Clear();
			PendingOrders.Clear();
			PendingOrdersWaitingTime.Clear();
			ProcessingFurnitureTime.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ManufacturerManager(SimId.ManufacturerManager, MySim, this);
			AddOwnMessage(Mc.TransferWorker);
			AddOwnMessage(Mc.RequestAssemblyLine);
			AddOwnMessage(Mc.ExecuteOperationStep);
			AddOwnMessage(Mc.ProcessOrder);
			AddOwnMessage(Mc.RequestWorker);
		}
		//meta! tag="end"
	}
}