using DiscreteSimulation.FurnitureManufacturer.Entities;
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
				myMessage.NotifyIfWorkerIsAvailable = false;
				
				MyAgent.AvailableWorkers.AddLast(worker);
				
				Notice(myMessage);
			}
			else
			{
				MyAgent.AvailableWorkers.AddLast(worker);
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
				var worker = GetAvailableWorker(myMessage.Furniture?.CurrentAssemblyLine);
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
				var worker = GetAvailableWorker(myMessage.Furniture?.CurrentAssemblyLine);
				myMessage.Worker = worker;
				Response(myMessage);
			}
			else
			{
				myMessage.Worker = null;
				Response(myMessage);
			}
		}
		
		private Worker GetAvailableWorker(AssemblyLine? preferredAssemblyLine = null)
		{
			var mySimulation = (MySimulation)MySim;
			
			// Ak je vypnuté preferovanie voľných pracovníkov na základe ich polohy,
			// vrátime prvého voľného pracovníka
			if (!mySimulation.EnableWorkerLocationPreference)
			{
				var firstWorker = MyAgent.AvailableWorkers.First.Value;
				MyAgent.AvailableWorkers.RemoveFirst();
				return firstWorker;
			}
			
			
			LinkedListNode<Worker>? availableWorker = null;
			LinkedListNode<Worker>? availableWorkerFromWarehouse = null;

			// Preferujeme najskôr pracovníka, ktorý sa už na danej linke nachádza,
			// inak pracovníka, ktorý je v sklade (E(X) príchodu zo skladu je menší
			// ako E(X) príchodu z inej linky) inak iný voľný pracovník
			var node = MyAgent.AvailableWorkers.First;
			
			while (node != null)
			{
				if (availableWorker == null)
				{
					availableWorker = node;
				}
			
				if (preferredAssemblyLine != null && node.Value.CurrentAssemblyLine == preferredAssemblyLine)
				{
					var worker = node.Value;
					MyAgent.AvailableWorkers.Remove(node);
					return worker;
				}
			
				if (node.Value.IsInWarehouse && availableWorkerFromWarehouse == null)
				{
					availableWorkerFromWarehouse = node;
				}
				
				node = node.Next;
			}
			
			if (availableWorkerFromWarehouse != null)
			{
				var worker = availableWorkerFromWarehouse.Value;
				MyAgent.AvailableWorkers.Remove(availableWorkerFromWarehouse);
				return worker;
			}
			
			if (availableWorker != null)
			{
				var worker = availableWorker.Value;
				MyAgent.AvailableWorkers.Remove(availableWorker);
				return worker;
			}
			
			throw new Exception("No available worker found");
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