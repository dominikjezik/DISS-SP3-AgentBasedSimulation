using OSPABA;
using Simulation;
namespace Agents.ResourcesAgent
{
	//meta! id="3"
	public class ResourcesManager : OSPABA.Manager
	{
		public ResourcesManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="ServiceCustomerProcess", id="14", type="Finish"
		public void ProcessFinish(MessageForm message)
		{
			// Proces obsluhy zakazníka skončil, oznámime to agentovi modelu
			message.Code = Mc.CustomerServiceEnded;
			Response(message);
			
			// Ak je front zakazníkov neprázdny, začneme s obsluhou ďalšieho zakazníka
			if (MyAgent.CustomersQueue.Count > 0)
			{
				message = MyAgent.CustomersQueue.Dequeue();
				
				MyAgent.CustomersQueueWaitingTime.AddValue(MySim.CurrentTime - ((MyMessage)message).StartOfWaiting);
				
				message.Addressee = MyAgent.FindAssistant(SimId.ServiceCustomerProcess);
				StartContinualAssistant(message);
			}
			else
			{
				MyAgent.IsServiceFree = true;
			}
		}

		//meta! sender="ModelAgent", id="11", type="Request"
		public void ProcessServiceCustomer(MessageForm message)
		{
			// Prišiel novy zakaznik do systemu, ak je oblsuha volna mozeme obsluzit inak ide do fronty
			if (MyAgent.IsServiceFree)
			{
				MyAgent.IsServiceFree = false;
				MyAgent.CustomersQueueWaitingTime.AddValue(0);
				
				// Spustenie procesu obsluhy zakazníka
				message.Addressee = MyAgent.FindAssistant(SimId.ServiceCustomerProcess);
				StartContinualAssistant(message);
			}
			else
			{
				((MyMessage)message).StartOfWaiting = MySim.CurrentTime;
				MyAgent.CustomersQueue.Enqueue((MyMessage)message);
			}
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
			case Mc.ServiceCustomer:
				ProcessServiceCustomer(message);
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
		public new ResourcesAgent MyAgent
		{
			get
			{
				return (ResourcesAgent)base.MyAgent;
			}
		}
	}
}