using OSPABA;
using Simulation;
using Agents.OperationAgent.ContinualAssistants;

namespace Agents.OperationAgent
{
	//meta! id="15"
	public class OperationAgent : OSPABA.Agent
	{
		public OperationAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			AddOwnMessage(Mc.OperationStepFinished);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new OperationManager(SimId.OperationManager, MySim, this);
			new ExecuteOperationStepProcess(SimId.ExecuteOperationStepProcess, MySim, this);
			AddOwnMessage(Mc.ExecuteOperationStep);
		}
		//meta! tag="end"
	}
}