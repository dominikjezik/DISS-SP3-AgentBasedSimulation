using OSPABA;
using Simulation;
using Agents.EnvironmentAgent.ContinualAssistants;
using DiscreteSimulation.Core.Utilities;

namespace Agents.EnvironmentAgent
{
	//meta! id="3"
	public class EnvironmentAgent : OSPABA.Agent
	{
		public Statistics ProcessingOrderTime { get; private set; } = new();
		
		public EnvironmentAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			AddOwnMessage(Mc.OrderArrived);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			ProcessingOrderTime.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new EnvironmentManager(SimId.EnvironmentManager, MySim, this);
			new OrdersArrivalScheduler(SimId.OrdersArrivalScheduler, MySim, this);
			AddOwnMessage(Mc.OrderFinished);
			AddOwnMessage(Mc.Initialize);
		}
		//meta! tag="end"
	}
}