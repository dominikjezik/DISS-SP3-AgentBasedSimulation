using OSPABA;
using Simulation;
namespace Agents.EnvironmentAgent
{
	//meta! id="3"
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

		//meta! sender="ModelAgent", id="26", type="Notice"
		public void ProcessOrderFinished(MessageForm message)
		{
			// Spracovanie objednávky je hotové
			var myMessage = (MyMessage)message;
			
			MyAgent.ProcessingOrderTime.AddValue(MySim.CurrentTime - myMessage.Order.ArrivalTime);
		}

		//meta! sender="OrdersArrivalScheduler", id="53", type="Finish"
		public void ProcessFinish(MessageForm message)
		{
		}

		//meta! sender="ModelAgent", id="27", type="Notice"
		public void ProcessInitialize(MessageForm message)
		{
			message.Addressee = MyAgent.FindAssistant(SimId.OrdersArrivalScheduler);
			StartContinualAssistant(message);
		}
		
		public void ProcessOrderArrived(MessageForm message)
		{
			// Prisla nova objednavka do systemu, oznamujeme agentovi modelu
			message.Addressee = MyAgent.Parent;
			Notice(message);
			
			// Naplanujeme prichod dalsej objednavky
			var newMessage = (MyMessage)message.CreateCopy();
			newMessage.Addressee = MyAgent.FindAssistant(SimId.OrdersArrivalScheduler);
			StartContinualAssistant(newMessage);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.OrderArrived:
					ProcessOrderArrived(message);
					break;
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
			case Mc.OrderFinished:
				ProcessOrderFinished(message);
			break;

			case Mc.Finish:
				ProcessFinish(message);
			break;

			case Mc.Initialize:
				ProcessInitialize(message);
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