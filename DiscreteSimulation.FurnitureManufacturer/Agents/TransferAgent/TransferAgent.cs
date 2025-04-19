using OSPABA;
using Simulation;
using Agents.TransferAgent.ContinualAssistants;

namespace Agents.TransferAgent
{
	//meta! id="13"
	public class TransferAgent : OSPABA.Agent
	{
		public TransferAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			AddOwnMessage(Mc.WorkerTransferFinished);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new TransferManager(SimId.TransferManager, MySim, this);
			new WorkerTransferProcess(SimId.WorkerTransferProcess, MySim, this);
			AddOwnMessage(Mc.TransferWorker);
		}
		//meta! tag="end"
	}
}