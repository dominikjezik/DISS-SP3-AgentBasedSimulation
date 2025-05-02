using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;

namespace Agents.WorkersGroupBAgent
{
	//meta! id="8"
	public class WorkersGroupBAgent : OSPABA.Agent
	{
		public Worker[] Workers { get; private set; }
		
		public LinkedList<Worker> AvailableWorkers  { get; private set; } = new();

		public EntitiesPriorityQueue<MyMessage> WorkersRequestsQueue { get; private set; }

		public WorkersGroupBAgent(int id, OSPABA.Simulation mySim, Agent parent) :
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
			ResetWorkers(mySimulation.CountOfWorkersGroupB);
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new WorkersGroupBManager(SimId.WorkersGroupBManager, MySim, this);
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
				var worker = new Worker(MySim, i + 1, WorkerGroup.GroupB);
				
				Workers[i] = worker;
				AvailableWorkers.AddLast(worker);
			}
		}
	}
}