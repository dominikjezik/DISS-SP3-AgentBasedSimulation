using Agents.ResourcesAgent;
using OSPABA;
using Agents.EnvironmentAgent;
using Agents.ModelAgent;
using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.Core.Utilities;

namespace Simulation
{
	public class MySimulation : OSPABA.Simulation
	{
		public SeedGenerator SeedGenerator { get; private set; }

		public Statistics AverageCustomersQueueWaitingTime { get; set; }

		public Statistics AverageCustomersQueueLength { get; set; }
		
		public MySimulation()
		{
			SeedGenerator = new SeedGenerator();
			Init();
		}

		override public void PrepareSimulation()
		{
			base.PrepareSimulation();
			// Create global statistcis
			AverageCustomersQueueWaitingTime = new Statistics();
			AverageCustomersQueueLength = new Statistics();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Reset entities, queues, local statistics, etc...
		}

		override public void ReplicationFinished()
		{
			// Collect local statistics into global, update UI, etc...
			base.ReplicationFinished();

			var averageCustomersQueueWaitingTime = ResourcesAgent.CustomersQueueWaitingTime.Mean;
			AverageCustomersQueueWaitingTime.AddValue(averageCustomersQueueWaitingTime);
			
			ResourcesAgent.CustomersQueue.RefreshStatistics();
			var averageCustomersQueueLength = ResourcesAgent.CustomersQueue.AverageQueueLength;
			AverageCustomersQueueLength.AddValue(averageCustomersQueueLength);
		}

		override public void SimulationFinished()
		{
			// Display simulation results
			base.SimulationFinished();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			ModelAgent = new ModelAgent(SimId.ModelAgent, this, null);
			EnvironmentAgent = new EnvironmentAgent(SimId.EnvironmentAgent, this, ModelAgent);
			ResourcesAgent = new ResourcesAgent(SimId.ResourcesAgent, this, ModelAgent);
		}
		public ModelAgent ModelAgent
		{ get; set; }
		public EnvironmentAgent EnvironmentAgent
		{ get; set; }
		public ResourcesAgent ResourcesAgent
		{ get; set; }
		//meta! tag="end"
	}
}