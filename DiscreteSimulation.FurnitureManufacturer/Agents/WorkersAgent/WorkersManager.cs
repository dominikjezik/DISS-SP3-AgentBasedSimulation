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
				// Zaradíme požiadavku CurrentlyCheckingMixedWorkersRequests
				MyAgent.CurrentlyCheckingMixedWorkersRequests.Add(myMessage);
				
				// Teraz sa pre túto požiadavku skontroluje, či je voľný pracovník z niektorej jej požadovaných skupín
				myMessage.Code = Mc.RequestWorkerIfAvailable;
				myMessage.Furniture.StartedWaitingTime = MySim.CurrentTime;
				myMessage.MixWorkerRequestedIndex = 0;

				TryRequestFromNextGroup(myMessage);
			}
			else
			{
				throw new ArgumentException("Invalid number of worker types.");
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
				if (MyAgent.MixedWorkersRequestsQueue.Count > 0)
				{
					myMessage.NotifyIfWorkerIsAvailable = true;
				}
				
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
			
			// Je treba skontrolovať či nemáme prioritnejšiu požiadavku na mix pracovníkov
			if (MyAgent.MixedWorkersRequestsQueue.Count == 0 || MyAgent.CurrentlyCheckingMixedWorkersRequests.Count > 0)
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
				var mixedWorkerRequest = MyAgent.MixedWorkersRequestsQueue.Peek();
				
				if (mixedWorkerRequest.RequestedWorkerType.Contains(myMessage.Worker.Group) && WorkerUtilities.RequestsComparator.Compare(mixedWorkerRequest, myMessage) == -1)
				{
					// Mixed požiadavka je prioritnejšia
					var mixedWorkerRequestDequeued = MyAgent.MixedWorkersRequestsQueue.Dequeue();
					mixedWorkerRequestDequeued.Worker = myMessage.Worker;
					mixedWorkerRequestDequeued.Code = Mc.RequestWorker;
					myMessage.Worker = null;
					
					MyAgent.WorkersMixRequestsQueueWaitingTime.AddValue(MySim.CurrentTime - mixedWorkerRequestDequeued.Furniture.StartedWaitingTime);
					
					Response(mixedWorkerRequestDequeued);
					
					// Pridáme späť do fronty aktuálnu požiadavku
					if (workerGroup == WorkerGroup.GroupA)
					{
						myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupAAgent);
						myMessage.Code = Mc.RequestWorker;
						Request(myMessage);
					}
					else if (workerGroup == WorkerGroup.GroupB)
					{
						myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupBAgent);
						myMessage.Code = Mc.RequestWorker;
						Request(myMessage);
					}
					else if (workerGroup == WorkerGroup.GroupC)
					{
						myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupCAgent);
						myMessage.Code = Mc.RequestWorker;
						Request(myMessage);
					}
					else
					{
						throw new ArgumentException("Invalid worker group type.");
					}
				}
				else
				{
					// Mixed požiadavka nie je prioritnejšia, môžeme spätne pridelit
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

			if (myMessage.Worker != null)
			{
				// Dostali sme priradeného pracovníka, môžeme odoslať response
				MyAgent.WorkersMixRequestsQueueWaitingTime.AddValue(MySim.CurrentTime - myMessage.Furniture.StartedWaitingTime);
				myMessage.MixWorkerRequestedIndex = -1;
				
				// Odstránime zo zoznamu aktuálne kontrolovaných požiadaviek
				MyAgent.CurrentlyCheckingMixedWorkersRequests.Remove(myMessage);
				
				myMessage.Code = Mc.RequestWorker;
				Response(myMessage);
			}
			else
			{
				// Nedostali sme priradeného pracovníka, ak sme ešte neskontrolovali všetky prípustné skupiny,
				// tak pokračujeme v pýtaní sa nasledujúcej skupiny
				myMessage.MixWorkerRequestedIndex++;
				
				if (myMessage.MixWorkerRequestedIndex < myMessage.RequestedWorkerType.Length)
				{
					TryRequestFromNextGroup(myMessage);
				}
				else
				{
					// Skontrolovali sme všetky prípustné skupiny, nemáme priradeného pracovníka
					myMessage.MixWorkerRequestedIndex = -1;
					
					// Odstránime zo zoznamu aktuálne kontrolovaných požiadaviek
					MyAgent.CurrentlyCheckingMixedWorkersRequests.Remove(myMessage);
					
					// Pridáme do fronty mix požiadaviek
					MyAgent.MixedWorkersRequestsQueue.Enqueue(myMessage);
				}
			}
			
			// Ak aktuálne nekontrolujeme žiadne mix požiadavky, tak spracujeme releasy
			if (MyAgent.CurrentlyCheckingMixedWorkersRequests.Count == 0)
			{
				while (MyAgent.PendingReleases.Count > 0)
				{
					var releaseMessage = MyAgent.PendingReleases.Dequeue();
					
					if (MyAgent.MixedWorkersRequestsQueue.Count > 0)
					{
						releaseMessage.NotifyIfWorkerIsAvailable = true;
					}
					
					ProcessReleaseWorker(releaseMessage);
				}
			}
		}

		private void TryRequestFromNextGroup(MyMessage myMessage)
		{
			if (myMessage.RequestedWorkerType[myMessage.MixWorkerRequestedIndex] == WorkerGroup.GroupC)
			{
				myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupCAgent);
				Request(myMessage);
			}
			else if (myMessage.RequestedWorkerType[myMessage.MixWorkerRequestedIndex] == WorkerGroup.GroupA)
			{
				myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupAAgent);
				Request(myMessage);
			}
			else if (myMessage.RequestedWorkerType[myMessage.MixWorkerRequestedIndex] == WorkerGroup.GroupB)
			{
				myMessage.Addressee = MySim.FindAgent(SimId.WorkersGroupBAgent);
				Request(myMessage);
			}
			else
			{
				throw new ArgumentException("Invalid worker group type.");
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
			ProcessWorkerIsAvailable(message, WorkerGroup.GroupA);
		}

		//meta! sender="WorkersGroupAAgent", id="74", type="Notice"
		public void ProcessWorkerIsAvailableWorkersGroupAAgent(MessageForm message)
		{
			ProcessWorkerIsAvailable(message, WorkerGroup.GroupB);
		}

		//meta! sender="WorkersGroupCAgent", id="75", type="Notice"
		public void ProcessWorkerIsAvailableWorkersGroupCAgent(MessageForm message)
		{
			ProcessWorkerIsAvailable(message, WorkerGroup.GroupC);
		}

		private void ProcessWorkerIsAvailable(MessageForm message, WorkerGroup workerGroup)
		{
			MyMessage? requestToProcess = null;
			var workers = new List<MyMessage>();

			// Je to napísané takto všeobecne, ak by vyžadovalo viac krát rôzne kombinácie (napr. B/C, A/B)
			// možno riešiť cez samostatné fronty pre každú možnú kombináciu
			while (requestToProcess == null && MyAgent.MixedWorkersRequestsQueue.Count > 0)
			{
				requestToProcess = MyAgent.MixedWorkersRequestsQueue.Dequeue();

				if (!requestToProcess.RequestedWorkerType.Contains(workerGroup))
				{
					workers.Add(requestToProcess);
					requestToProcess = null;
				}
			}
			
			workers.ForEach(r => MyAgent.MixedWorkersRequestsQueue.Enqueue(r));

			if (requestToProcess == null)
			{
				return;
			}
			
			// Zaradíme požiadavku CurrentlyCheckingMixedWorkersRequests
			MyAgent.CurrentlyCheckingMixedWorkersRequests.Add(requestToProcess);
			
			// Teraz sa opätovne pre túto požiadavku skontroluje, či je voľný pracovník z niektorej jej požadovaných skupín (mal by byť...)
			requestToProcess.Code = Mc.RequestWorkerIfAvailable;
			requestToProcess.Furniture.StartedWaitingTime = MySim.CurrentTime;
			requestToProcess.MixWorkerRequestedIndex = 0;

			TryRequestFromNextGroup(requestToProcess);
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