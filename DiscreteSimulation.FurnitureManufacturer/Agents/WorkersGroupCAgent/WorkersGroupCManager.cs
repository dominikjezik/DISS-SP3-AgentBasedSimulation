using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;
namespace Agents.WorkersGroupCAgent
{
	//meta! id="9"
	public class WorkersGroupCManager : OSPABA.Manager
	{
		public WorkersGroupCManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="WorkersAgent", id="47", type="Notice"
		public void ProcessReleaseWorker(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			var worker = myMessage.Worker;
			
			if (MyAgent.WorkersRequestsQueue.Count > 0)
			{
				myMessage = MyAgent.WorkersRequestsQueue.Dequeue();
				myMessage.Worker = worker;

				Response(myMessage);
			}
			else if (myMessage.NotifyIfWorkerIsAvailable)
			{
				// Je nastavený príznak pre notifikovanie parent agenta, pokial je k dispozícii volny pracovnik
				myMessage.Addressee = MyAgent.Parent;
				myMessage.Code = Mc.WorkerIsAvailable;
				
				Response(myMessage);
			}
			else
			{
				MyAgent.AvailableWorkers.Enqueue(worker, worker.Id);
			}
		}

		//meta! sender="WorkersAgent", id="42", type="Request"
		public void ProcessRequestWorker(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			
			// Do spravy je pribaleny pracovnik, cize ide o uvolnenie a zaroven o opätovné vyžiadanie
			if (myMessage.Worker != null)
			{
				if (MyAgent.WorkersRequestsQueue.Count == 0)
				{
					// Nikto nevyzaduje pracovnika, mozeme spätne pridelit
					Response(myMessage);
					return;
				}
				
				var mostPriorityRequest = MyAgent.WorkersRequestsQueue.Peek();
				
				if (WorkerUtilities.RequestsComparator.Compare(myMessage, mostPriorityRequest) <= 0)
				{
					// Spätne pridelíme pracovnika
					Response(myMessage);
				}
				else
				{
					// Našla sa prioritnejšia požiadavka, uprednostníme ju
					var worker = myMessage.Worker;
					myMessage.Worker = null;
					
					MyAgent.WorkersRequestsQueue.Enqueue(myMessage);
					
					myMessage = MyAgent.WorkersRequestsQueue.Dequeue();
					myMessage.Worker = worker;
					
					Response(myMessage);
				}
				
				return;
			}
			
			if (MyAgent.AvailableWorkers.Count > 0)
			{
				var worker = MyAgent.AvailableWorkers.Dequeue();
				myMessage.Worker = worker;
				
				Response(myMessage);
			}
			else
			{
				MyAgent.WorkersRequestsQueue.Enqueue(myMessage);
			}
		}

		//meta! sender="WorkersAgent", id="49", type="Request"
		public void ProcessRequestWorkerIfAvailable(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			
			if (MyAgent.AvailableWorkers.Count > 0)
			{
				var worker = MyAgent.AvailableWorkers.Dequeue();
				myMessage.Worker = worker;
				
				Response(myMessage);
			}
			else
			{
				myMessage.Worker = null;
				Request(myMessage);
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
			case Mc.RequestWorker:
				ProcessRequestWorker(message);
			break;

			case Mc.ReleaseWorker:
				ProcessReleaseWorker(message);
			break;

			case Mc.RequestWorkerIfAvailable:
				ProcessRequestWorkerIfAvailable(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new WorkersGroupCAgent MyAgent
		{
			get
			{
				return (WorkersGroupCAgent)base.MyAgent;
			}
		}
	}
}