using OSPABA;
using Simulation;
namespace Agents.ModelAgent
{
	//meta! id="2"
	public class ModelManager : OSPABA.Manager
	{
		public ModelManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="ManufacturerAgent", id="12", type="Response"
		public void ProcessProcessOrder(MessageForm message)
		{
			// Bola ukončená výroba objednávky, odosielame správu agentovi okolia
			message.Code = Mc.OrderFinished;
			message.Addressee = MySim.FindAgent(SimId.EnvironmentAgent);
			Notice(message);
		}

		//meta! sender="EnvironmentAgent", id="25", type="Notice"
		public void ProcessOrderArrived(MessageForm message)
		{
			// Prisla nova objednavka do systemu, poziadavku odosleme agentovi vyroby
			message.Code = Mc.ProcessOrder;
			message.Addressee = MySim.FindAgent(SimId.ManufacturerAgent);
			Request(message);
		}
		
		public void ProcessInitialize(MessageForm message)
		{
			// Ked nam pride inicializacna sprava, preposleme ju agentovi okolia
			message.Addressee = MySim.FindAgent(SimId.EnvironmentAgent);
			Notice(message);
		}
		
		public void ProcessOrderFinished(MessageForm message)
		{
			// Ked sa dokonci vyroba objednavky, oznamime to agentovi okolia
			message.Code = Mc.OrderFinished;
			message.Addressee = MySim.FindAgent(SimId.EnvironmentAgent);
			Notice(message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.Initialize:
					ProcessInitialize(message);
					break;
				case Mc.OrderFinished:
					ProcessOrderFinished(message);
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
			case Mc.ProcessOrder:
				ProcessProcessOrder(message);
			break;

			case Mc.OrderArrived:
				ProcessOrderArrived(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new ModelAgent MyAgent
		{
			get
			{
				return (ModelAgent)base.MyAgent;
			}
		}
	}
}