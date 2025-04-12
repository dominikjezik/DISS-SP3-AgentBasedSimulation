using OSPABA;
using Simulation;
using Agents.ResourcesAgent.ContinualAssistants;
using DiscreteSimulation.Core.Utilities;

namespace Agents.ResourcesAgent
{
	//meta! id="3"
	public class ResourcesAgent : OSPABA.Agent
	{
		public ABASimEntitiesQueue<MyMessage> CustomersQueue { get; private set; }
		
		public bool IsServiceFree { get; set; }
		
		public Statistics CustomersQueueWaitingTime { get; private set; }
		
		public ResourcesAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			AddOwnMessage(Mc.CustomerServiceEnded);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			CustomersQueue = new ABASimEntitiesQueue<MyMessage>(MySim);
			IsServiceFree = true;
			CustomersQueueWaitingTime = new Statistics();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ResourcesManager(SimId.ResourcesManager, MySim, this);
			new ServiceCustomerProcess(SimId.ServiceCustomerProcess, MySim, this);
			AddOwnMessage(Mc.ServiceCustomer);
		}
		//meta! tag="end"
	}
}