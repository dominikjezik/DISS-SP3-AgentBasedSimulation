using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;
namespace Agents.TransferAgent
{
	//meta! id="13"
	public class TransferManager : OSPABA.Manager
	{
		public TransferManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
			base(id, mySim, myAgent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication

			if (PetriNet != null)
			{
				PetriNet.Clear();
			}
		}

		//meta! sender="ManufacturerAgent", id="31", type="Request"
		public void ProcessTransferWorker(MessageForm message)
		{
			// Pracovnika je potrebne premiestnit bud do skladu alebo na vyrobnu linku
			message.Addressee = MyAgent.FindAssistant(SimId.WorkerTransferProcess);
			StartContinualAssistant(message);
		}

		//meta! sender="WorkerTransferProcess", id="56", type="Finish"
		public void ProcessFinish(MessageForm message)
		{
			// Pracovnik bol premiestnený na pozadovane miesto
			message.Code = Mc.TransferWorker;
			Response(message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			}
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		public void Init()
		{
		}

		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.TransferWorker:
				ProcessTransferWorker(message);
			break;

			case Mc.Finish:
				ProcessFinish(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new TransferAgent MyAgent
		{
			get
			{
				return (TransferAgent)base.MyAgent;
			}
		}
	}
}