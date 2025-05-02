using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;

namespace Agents.WorkersGroupAAgent
{
	//meta! id="7"
	public class WorkersGroupAAgent : OSPABA.Agent
	{
		public Worker[] Workers { get; private set; }
		
		public LinkedList<Worker> AvailableWorkers  { get; private set; } = new();

		public EntitiesPriorityQueue<MyMessage> WorkersRequestsQueue { get; private set; }
		
		public WorkersGroupAAgent(int id, OSPABA.Simulation mySim, Agent parent) :
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
			ResetWorkers(mySimulation.CountOfWorkersGroupA);
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new WorkersGroupAManager(SimId.WorkersGroupAManager, MySim, this);
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
				var worker = new Worker(MySim, i + 1, WorkerGroup.GroupA);
				Workers[i] = worker;
				AvailableWorkers.AddLast(worker);
			}
		}
	}
}