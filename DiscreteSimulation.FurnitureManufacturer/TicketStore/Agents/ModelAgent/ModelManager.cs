using OSPABA;
using Simulation;
namespace Agents.ModelAgent
{
	//meta! id="1"
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

		//meta! sender="EnvironmentAgent", id="10", type="Notice"
		public void ProcessCustomerArrival(MessageForm message)
		{
			// Prisiel novy zakaznik do systemu, posielame spravu agentovi obsluhy
			message.Code = Mc.ServiceCustomer;
			message.Addressee = MySim.FindAgent(SimId.ResourcesAgent);
			Request(message);
		}

		//meta! sender="ResourcesAgent", id="11", type="Response"
		public void ProcessServiceCustomer(MessageForm message)
		{
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.Initialize:
					// Ked nam pride inicializacna sprava, preposleme ju agentovi okolia
					message.Addressee = MySim.FindAgent(SimId.EnvironmentAgent);
					Notice(message);
					break;
				case Mc.CustomerServiceEnded:
					// Ked sa dokonci obsluha zakaznika, oznamime to agentovi okolia
					message.Code = Mc.CustomerLeave;
					message.Addressee = MySim.FindAgent(SimId.EnvironmentAgent);
					Notice(message);
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
			case Mc.ServiceCustomer:
				ProcessServiceCustomer(message);
			break;

			case Mc.CustomerArrival:
				ProcessCustomerArrival(message);
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