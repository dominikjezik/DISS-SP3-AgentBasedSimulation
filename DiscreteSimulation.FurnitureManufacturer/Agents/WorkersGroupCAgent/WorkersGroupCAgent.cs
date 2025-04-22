using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;

namespace Agents.WorkersGroupCAgent
{
	//meta! id="9"
	public class WorkersGroupCAgent : OSPABA.Agent
	{
		public Worker[] Workers { get; private set; }
		
		public LinkedList<Worker> AvailableWorkers  { get; private set; } = new();
		
		public EntitiesPriorityQueue<MyMessage> WorkersRequestsQueue { get; private set; }
		
		public WorkersGroupCAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			WorkersRequestsQueue = new EntitiesPriorityQueue<MyMessage>(WorkerUtilities.RequestsComparator, MySim);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			var mySimulation = (MySimulation)MySim;
			
			WorkersRequestsQueue.Clear();
			ResetWorkers(mySimulation.CountOfWorkersGroupC);
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new WorkersGroupCManager(SimId.WorkersGroupCManager, MySim, this);
			AddOwnMessage(Mc.ReleaseWorker);
			AddOwnMessage(Mc.RequestWorkerIfAvailable);
			AddOwnMessage(Mc.RequestWorker);
		}
		//meta! tag="end"
		
		private void ResetWorkers(int count)
		{
			Workers = new Worker[count];
			AvailableWorkers.Clear();
			
			for (int i = 0; i < count; i++)
			{
				var worker = new Worker(MySim)
				{
					Id = i + 1,
					Group = WorkerGroup.GroupC,
					IsInWarehouse = true
				};
				
				Workers[i] = worker;
				AvailableWorkers.AddLast(worker);
			}
		}
	}
}