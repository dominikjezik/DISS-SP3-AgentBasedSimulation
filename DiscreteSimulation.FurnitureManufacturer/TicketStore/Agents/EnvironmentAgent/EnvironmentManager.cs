using OSPABA;
using Simulation;
namespace Agents.EnvironmentAgent
{
	//meta! id="2"
	public class EnvironmentManager : OSPABA.Manager
	{
		public EnvironmentManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="CustomerArrivalScheduler", id="9", type="Finish"
		public void ProcessFinish(MessageForm message)
		{
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.CustomerArrival:
					// Prisiel novy zakazník, preto to oznamime agentovi modelu
					message.Addressee = MyAgent.Parent;
					Notice(message);
					// Po spracovani odosleme spravu naspat do planovaca prichodov, aby sa naplanoval prichod dalsieho zakaznika
					var newMessage = (MyMessage)message.CreateCopy();
					newMessage.Addressee = MyAgent.FindAssistant(SimId.CustomerArrivalScheduler);
					StartContinualAssistant(newMessage);
					break;
			}
		}

		//meta! sender="ModelAgent", id="16", type="Notice"
		public void ProcessInitialize(MessageForm message)
		{
			message.Addressee = MyAgent.FindAssistant(SimId.CustomerArrivalScheduler);
			StartContinualAssistant(message);
		}

		//meta! sender="ModelAgent", id="18", type="Notice"
		public void ProcessCustomerLeave(MessageForm message)
		{
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		public void Init()
		{
		}

		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.Finish:
				ProcessFinish(message);
			break;

			case Mc.Initialize:
				ProcessInitialize(message);
			break;

			case Mc.CustomerLeave:
				ProcessCustomerLeave(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new EnvironmentAgent MyAgent
		{
			get
			{
				return (EnvironmentAgent)base.MyAgent;
			}
		}
	}
}