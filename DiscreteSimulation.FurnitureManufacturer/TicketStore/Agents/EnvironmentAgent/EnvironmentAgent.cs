using OSPABA;
using Simulation;
using Agents.EnvironmentAgent.ContinualAssistants;

namespace Agents.EnvironmentAgent
{
	//meta! id="2"
	public class EnvironmentAgent : OSPABA.Agent
	{
		public EnvironmentAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			AddOwnMessage(Mc.CustomerArrival);
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new EnvironmentManager(SimId.EnvironmentManager, MySim, this);
			new CustomerArrivalScheduler(SimId.CustomerArrivalScheduler, MySim, this);
			AddOwnMessage(Mc.Initialize);
			AddOwnMessage(Mc.CustomerLeave);
		}
		//meta! tag="end"
	}
}