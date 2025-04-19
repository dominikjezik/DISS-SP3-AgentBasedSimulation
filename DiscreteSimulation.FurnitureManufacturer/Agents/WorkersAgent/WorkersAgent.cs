using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;

namespace Agents.WorkersAgent
{
	//meta! id="14"
	public class WorkersAgent : OSPABA.Agent
	{
		public EntitiesPriorityQueue<MyMessage> MixedWorkersRequestsQueue { get; private set; }

		public List<MyMessage> CurrentlyCheckingMixedWorkersRequests { get; set; } = new();
		
		public Queue<MyMessage> PendingReleases { get; set; } = new();
		
		public Statistics WorkersARequestsQueueWaitingTime { get; private set; } = new();
		
		public Statistics WorkersBRequestsQueueWaitingTime { get; private set; } = new();
		
		public Statistics WorkersCRequestsQueueWaitingTime { get; private set; } = new();
		
		public Statistics WorkersMixRequestsQueueWaitingTime { get; private set; } = new();
		
		public WorkersAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			MixedWorkersRequestsQueue = new EntitiesPriorityQueue<MyMessage>(WorkerUtilities.RequestsComparator, MySim);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			MixedWorkersRequestsQueue.Clear();
			CurrentlyCheckingMixedWorkersRequests.Clear();
			PendingReleases.Clear();
			WorkersARequestsQueueWaitingTime.Clear();
			WorkersBRequestsQueueWaitingTime.Clear();
			WorkersCRequestsQueueWaitingTime.Clear();
			WorkersMixRequestsQueueWaitingTime.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new WorkersManager(SimId.WorkersManager, MySim, this);
			AddOwnMessage(Mc.ReleaseWorker);
			AddOwnMessage(Mc.RequestWorkerIfAvailable);
			AddOwnMessage(Mc.WorkerIsAvailable);
			AddOwnMessage(Mc.RequestWorker);
		}
		//meta! tag="end"
	}
}