using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;
namespace Agents.WorkersAgent
{
	//meta! id="14"
	public class WorkersManager : OSPABA.Manager
	{
		public WorkersManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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
		
		//meta! sender="ManufacturerAgent", id="30", type="Request"
		public void ProcessRequestWorkerManufacturerAgent(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			
			// Ak pozadujeme pracovnika z jedinej skupiny
			if (myMessage.RequestedWorkerType.Length == 1)
			{
				if (myMessage.RequestedWorkerType[0] == WorkerGroup.GroupA)
				{
					myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupAAgent);
					myMessage.Code = Mc.RequestWorker;
					myMessage.Furniture.StartedWaitingTime = MySim.CurrentTime;
					Request(myMessage);
				}
				else if (myMessage.RequestedWorkerType[0] == WorkerGroup.GroupB)
				{
					myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupBAgent);
					myMessage.Code = Mc.RequestWorker;
					myMessage.Furniture.StartedWaitingTime = MySim.CurrentTime;
					Request(myMessage);
				}
				else if (myMessage.RequestedWorkerType[0] == WorkerGroup.GroupC)
				{
					myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupCAgent);
					myMessage.Code = Mc.RequestWorker;
					myMessage.Furniture.StartedWaitingTime = MySim.CurrentTime;
					Request(myMessage);
				}
				else
				{
					throw new ArgumentException("Invalid worker group type.");
				}
			}
			// Ak pozadujeme pracovnika, ktorý môže byť z rôznych skupín
			else if (myMessage.RequestedWorkerType.Length == 2)
			{
				// TODO
				throw new NotImplementedException();
			}
			else
			{
				throw new ArgumentException("Invalid worker group type.");
			}
		}

		//meta! sender="ManufacturerAgent", id="37", type="Notice"
		public void ProcessReleaseWorker(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			
			// Prebieha kontrola, ci je volny pracovnik z roznych skupin, nemozme teraz uvolnit pracovnika
			if (MyAgent.CurrentlyCheckingMixedWorkersRequests.Count > 0)
			{
				MyAgent.PendingReleases.Enqueue(myMessage);
			}
			else
			{
				if (myMessage.Worker.Group == WorkerGroup.GroupA)
				{
					myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupAAgent);
					Notice(myMessage);
				}
				else if (myMessage.Worker.Group == WorkerGroup.GroupB)
				{
					myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupBAgent);
					Notice(myMessage);
				}
				else if (myMessage.Worker.Group == WorkerGroup.GroupC)
				{
					myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupCAgent);
					Notice(myMessage);
				}
				else
				{
					throw new ArgumentException("Invalid worker group type.");
				}
			}
		}

		//meta! sender="WorkersGroupAAgent", id="41", type="Response"
		public void ProcessRequestWorkerWorkersGroupAAgent(MessageForm message)
		{
			ProcessRequestWorker(message, WorkerGroup.GroupA);
		}

		//meta! sender="WorkersGroupBAgent", id="43", type="Response"
		public void ProcessRequestWorkerWorkersGroupBAgent(MessageForm message)
		{
			ProcessRequestWorker(message, WorkerGroup.GroupB);
		}

		//meta! sender="WorkersGroupCAgent", id="42", type="Response"
		public void ProcessRequestWorkerWorkersGroupCAgent(MessageForm message)
		{
			ProcessRequestWorker(message, WorkerGroup.GroupC);
		}

		private void ProcessRequestWorker(MessageForm message, WorkerGroup workerGroup)
		{
			var myMessage = (MyMessage)message;
			
			// Prebieha pokus o pridelenie pracovnika poziadavke na konkretneho pracovnika
			// Je treba ale skontrolovat či v našich požiadavach na mix pracovníkov nie je prioritnejšia požiadavka
			if (MyAgent.CurrentlyCheckingMixedWorkersRequests.Count == 0)
			{
				// Nikto nevyžaduje pracovníka, môžeme spätne pridelit
				if (workerGroup == WorkerGroup.GroupA)
				{
					MyAgent.WorkersARequestsQueueWaitingTime.AddValue(MySim.CurrentTime - myMessage.Furniture.StartedWaitingTime);
				}
				else if (workerGroup == WorkerGroup.GroupB)
				{
					MyAgent.WorkersBRequestsQueueWaitingTime.AddValue(MySim.CurrentTime - myMessage.Furniture.StartedWaitingTime);
				}
				else if (workerGroup == WorkerGroup.GroupC)
				{
					MyAgent.WorkersCRequestsQueueWaitingTime.AddValue(MySim.CurrentTime - myMessage.Furniture.StartedWaitingTime);
				}
				
				Response(myMessage);
			}
			else
			{
				// Kontrola či je požiadavka prioritnejšia
				
				throw new NotImplementedException();
				
				// TODO: Nezabudnut na statistiky pre A, B, C a MIX
			}
		}

		//meta! sender="WorkersGroupAAgent", id="48", type="Response"
		public void ProcessRequestWorkerIfAvailableWorkersGroupAAgent(MessageForm message)
		{
			ProcessRequestWorkerIfAvailable(message);
		}
		
		//meta! sender="WorkersGroupBAgent", id="76", type="Response"
		public void ProcessRequestWorkerIfAvailableWorkersGroupBAgent(MessageForm message)
		{
			ProcessRequestWorkerIfAvailable(message);
		}

		//meta! sender="WorkersGroupCAgent", id="49", type="Response"
		public void ProcessRequestWorkerIfAvailableWorkersGroupCAgent(MessageForm message)
		{
			ProcessRequestWorkerIfAvailable(message);
		}

		private void ProcessRequestWorkerIfAvailable(MessageForm message)
		{
			// Prisla odpoved na poziadavku na pracovnika (pytali sme sa kvoli mix poziadavke)
			var myMessage = (MyMessage)message;
			var worker = myMessage.Worker;

			if (worker == null)
			{
				
			}
			else
			{
				
			}
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
			}
		}

		//meta! sender="WorkersGroupBAgent", id="77", type="Notice"
		public void ProcessWorkerIsAvailableWorkersGroupBAgent(MessageForm message)
		{
		}

		//meta! sender="WorkersGroupAAgent", id="74", type="Notice"
		public void ProcessWorkerIsAvailableWorkersGroupAAgent(MessageForm message)
		{
		}

		//meta! sender="WorkersGroupCAgent", id="75", type="Notice"
		public void ProcessWorkerIsAvailableWorkersGroupCAgent(MessageForm message)
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
			case Mc.RequestWorkerIfAvailable:
				switch (message.Sender.Id)
				{
				case SimId.WorkersGroupCAgent:
					ProcessRequestWorkerIfAvailableWorkersGroupCAgent(message);
				break;

				case SimId.WorkersGroupBAgent:
					ProcessRequestWorkerIfAvailableWorkersGroupBAgent(message);
				break;

				case SimId.WorkersGroupAAgent:
					ProcessRequestWorkerIfAvailableWorkersGroupAAgent(message);
				break;
				}
			break;

			case Mc.WorkerIsAvailable:
				switch (message.Sender.Id)
				{
				case SimId.WorkersGroupBAgent:
					ProcessWorkerIsAvailableWorkersGroupBAgent(message);
				break;

				case SimId.WorkersGroupAAgent:
					ProcessWorkerIsAvailableWorkersGroupAAgent(message);
				break;

				case SimId.WorkersGroupCAgent:
					ProcessWorkerIsAvailableWorkersGroupCAgent(message);
				break;
				}
			break;

			case Mc.RequestWorker:
				switch (message.Sender.Id)
				{
				case SimId.WorkersGroupBAgent:
					ProcessRequestWorkerWorkersGroupBAgent(message);
				break;

				case SimId.ManufacturerAgent:
					ProcessRequestWorkerManufacturerAgent(message);
				break;

				case SimId.WorkersGroupCAgent:
					ProcessRequestWorkerWorkersGroupCAgent(message);
				break;

				case SimId.WorkersGroupAAgent:
					ProcessRequestWorkerWorkersGroupAAgent(message);
				break;
				}
			break;

			case Mc.ReleaseWorker:
				ProcessReleaseWorker(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new WorkersAgent MyAgent
		{
			get
			{
				return (WorkersAgent)base.MyAgent;
			}
		}
	}
}