using OSPABA;
using Simulation;
namespace Agents.OperationAgent
{
	//meta! id="15"
	public class OperationManager : OSPABA.Manager
	{
		public OperationManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="ManufacturerAgent", id="34", type="Request"
		public void ProcessExecuteOperationStep(MessageForm message)
		{
			// Pracovnik moze zacat vykonavat vyrobny krok
			message.Addressee = MyAgent.FindAssistant(SimId.ExecuteOperationStepProcess);
			StartContinualAssistant(message);
		}

		//meta! sender="ExecuteOperationStepProcess", id="59", type="Finish"
		public void ProcessFinish(MessageForm message)
		{
			// Bol dokonceny vyrobny krok
			message.Code = Mc.ExecuteOperationStep;
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
			case Mc.ExecuteOperationStep:
				ProcessExecuteOperationStep(message);
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
		public new OperationAgent MyAgent
		{
			get
			{
				return (OperationAgent)base.MyAgent;
			}
		}
	}
}