using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
using Simulation;
namespace Agents.ManufacturerAgent
{
	//meta! id="4"
	public class ManufacturerManager : OSPABA.Manager
	{
		public ManufacturerManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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
		
		//meta! sender="ModelAgent", id="12", type="Request"
		public void ProcessProcessOrder(MessageForm message)
		{
			// Prisla zadost na spracovanie novej objednavky
			var myMessage = (MyMessage)message;
			var newOrder = myMessage.Order;
			
			MyAgent.AllOrders.Add(newOrder);
			MyAgent.UnfinishedOrders.AddFirst(myMessage);
			MyAgent.PendingOrders.Enqueue(newOrder);
			
			foreach (var furnitureItem in newOrder.FurnitureItems)
			{
				// Pre zacatie spracovania polozky objednavky je potrebne najskor ziskat volnu linku
				var itemMessage = new MyMessage(MySim)
				{
					Addressee = MySim.FindAgent(SimId.AssemblyLinesAgent),
					Code = Mc.RequestAssemblyLine,
					Furniture = furnitureItem
				};
				
				Request(itemMessage);
			}
		}
		
		//meta! sender="AssemblyLinesAgent", id="63", type="Response"
		public void ProcessRequestAssemblyLine(MessageForm message)
		{
			// Bola pridelena volna linka pre polozku objednavky
			var myMessage = (MyMessage)message;
			
			var assemblyLine = myMessage.AssemblyLine;
			assemblyLine.CurrentFurniture = myMessage.Furniture;
			
			// Pre zacatie spracovania je potrebne este ziskat volneho pracovnika A
			myMessage.RequestedWorkerType = [WorkerGroup.GroupA];
			myMessage.Code = Mc.RequestWorker;
			myMessage.Addressee = MySim.FindAgent(SimId.WorkersAgent);
			
			Request(myMessage);
		}
		
		//meta! sender="WorkersAgent", id="30", type="Response"
		public void ProcessRequestWorker(MessageForm message)
		{
			// Pre polozku objednavky bol prideleny pracovnik
			var myMessage = (MyMessage)message;
			myMessage.Worker.CurrentFurniture = myMessage.Furniture;
			myMessage.Furniture.CurrentWorker = myMessage.Worker;
			
			SelectFurnitureOperationStep((MyMessage)message);
		}
		
		//meta! sender="TransferAgent", id="31", type="Response"
		public void ProcessTransferWorker(MessageForm message)
		{
			// Pracovnik bol presunuty na linku/do skladu
			SelectFurnitureOperationStep((MyMessage)message);
		}

		private void SelectFurnitureOperationStep(MyMessage myMessage)
		{
			var furniture = myMessage.Furniture;

			switch (furniture.CurrentOperationStep)
			{
				case FurnitureOperationStep.NotStarted:
					StartFurniturePreparation(myMessage);
					break;
				case FurnitureOperationStep.MaterialPrepared:
					StartCutting(myMessage);
					break;
				case FurnitureOperationStep.Cut:
				case FurnitureOperationStep.Stained:
				case FurnitureOperationStep.Varnished:
				case FurnitureOperationStep.Folded:
					StartOperationStep(myMessage);
					break;
				default:
					throw new Exception($"Unexpected furniture step {furniture.CurrentOperationStep}");
			}
		}
		
		private void StartFurniturePreparation(MyMessage myMessage)
		{
			var furniture = myMessage.Furniture;
			var worker = furniture.CurrentWorker;
			
			// Kontrola či je to prvá položka objednávky, ktorej bola pridelena linka
			if (furniture.Order.State == OrderState.Pending)
			{
				var orderFromQueue = MyAgent.PendingOrders.Dequeue();
				
				if (orderFromQueue != furniture.Order)
				{
					throw new Exception($"Unexpected order in queue {orderFromQueue.Id} != {furniture.Order.Id}");
				}

				furniture.Order.State = OrderState.InProgress;
				
				MyAgent.PendingOrdersWaitingTime.AddValue(MySim.CurrentTime - furniture.Order.ArrivalTime);
			}
			
			if (!worker.IsInWarehouse)
			{
				// Ak sa pracovnik nenachadza v sklade tak ho tam premiestnime
				myMessage.Addressee = MySim.FindAgent(SimId.TransferAgent);
				myMessage.Code = Mc.TransferWorker;
				myMessage.IsTransferBetweenLines = false;
				myMessage.Worker = worker;
				Request(myMessage);
			}
			else
			{
				// Pracovnik sa nachadza v sklade, moze zacat pripravovat material
				myMessage.Addressee = MySim.FindAgent(SimId.OperationAgent);
				myMessage.Code = Mc.ExecuteOperationStep;
				Request(myMessage);
			}
		}
		
		private void StartCutting(MyMessage myMessage)
		{
			// Pracovnik uz je premiestneny na linke moze zacat rezanie
			var furniture = myMessage.Furniture;
			furniture.CurrentAssemblyLine = furniture.CurrentWorker.CurrentAssemblyLine;
			furniture.CurrentAssemblyLine.ContainsFurniture = true;
			
			myMessage.Addressee = MySim.FindAgent(SimId.OperationAgent);
			myMessage.Code = Mc.ExecuteOperationStep;
			Request(myMessage);
		}
		
		private void StartOperationStep(MyMessage myMessage)
		{
			var furniture = myMessage.Furniture;
			var worker = furniture.CurrentWorker;
			
			// Ak sa pracovnik nenachadza na linke kde je narezany nabytok tak ho tam premiestnime
			if (worker.IsInWarehouse)
			{
				myMessage.Addressee = MySim.FindAgent(SimId.TransferAgent);
				myMessage.Code = Mc.TransferWorker;
				myMessage.IsTransferBetweenLines = false;
				myMessage.Worker = worker;
				Request(myMessage);
			}
			else if (worker.CurrentAssemblyLine != myMessage.AssemblyLine)
			{
				myMessage.Addressee = MySim.FindAgent(SimId.TransferAgent);
				myMessage.Code = Mc.TransferWorker;
				myMessage.IsTransferBetweenLines = true;
				myMessage.Worker = worker;
				myMessage.AssemblyLine = furniture.CurrentAssemblyLine;
				Request(myMessage);
			}
			else
			{
				// Ak sa pracovnik nachadza na linke kde je narezany nabytok moze zacat morenie
				worker.CurrentAssemblyLine.IdleWorkers.Remove(worker);
				worker.CurrentAssemblyLine.CurrentWorker = worker;
				
				myMessage.Addressee = MySim.FindAgent(SimId.OperationAgent);
				myMessage.Code = Mc.ExecuteOperationStep;
				Request(myMessage);
			}
		}
		
		//meta! sender="OperationAgent", id="34", type="Response"
		public void ProcessExecuteOperationStep(MessageForm message)
		{
			// Bol spracovany krok vyroby
			var myMessage = (MyMessage)message;
			var furniture = myMessage.Furniture;
			var worker = furniture.CurrentWorker;
			
			// Pokial to bola priprava v sklade tak pracovnika neuvolnujeme
			if (furniture.CurrentOperationStep == FurnitureOperationStep.MaterialPrepared)
			{
				// Presuvame pracovnika s pripravenym materialom na vyrobnu linku
				myMessage.Addressee = MySim.FindAgent(SimId.TransferAgent);
				myMessage.Code = Mc.TransferWorker;
				myMessage.IsTransferBetweenLines = false;
				
				Request(myMessage);

				return;
			}
			
			furniture.CurrentWorker = null;
			worker.CurrentFurniture = null;
			worker.CurrentAssemblyLine.IdleWorkers.Add(worker);
			worker.CurrentAssemblyLine.CurrentWorker = null;
			
			// Pokial bolo prave dokoncene Morenie vykonane pracovnikom C
			// nasledujuca cinnost je Lakovanie (ak to nabytok vyžaduje) tiez vykonava pracovnik C
			// Preto pracovnika C neuvolnime štandardne ale vykonáme na neho požiadavku s pripradnym uvolnením
			if (furniture.CurrentOperationStep == FurnitureOperationStep.Stained && furniture.NeedsToBeVarnished)
			{
				// Opätovné vyžiadanie toho istého pracovníka C, pričom worker ostáva v správe
				myMessage.Addressee = MySim.FindAgent(SimId.WorkersAgent);
				myMessage.Code = Mc.RequestWorker;
				myMessage.RequestedWorkerType = [WorkerGroup.GroupC];
				
				Request(myMessage);

				return;
			}
			
			// Bol spracovany krok vyroby, uvolnime pracovnika
			var messageCopy = (MyMessage)myMessage.CreateCopy();
			
			messageCopy.Addressee = MySim.FindAgent(SimId.WorkersAgent);
			messageCopy.Code = Mc.ReleaseWorker;
			messageCopy.Worker = worker;
			Notice(messageCopy);
			
			// Kontrola ci nebola polozka objednavky dokoncena
			if (furniture.CurrentOperationStep == FurnitureOperationStep.Completed)
			{
				// Uvolnenie vyrobnej linky
				myMessage.Addressee = MySim.FindAgent(SimId.AssemblyLinesAgent);
				myMessage.Code = Mc.ReleaseAssemblyLine;
				myMessage.AssemblyLine = furniture.CurrentAssemblyLine;
				
				furniture.CurrentAssemblyLine.ContainsFurniture = false;
				furniture.CurrentAssemblyLine.CurrentFurniture = null;
				furniture.CurrentAssemblyLine = null;
				
				MyAgent.ProcessingFurnitureTime.AddValue(MySim.CurrentTime - furniture.Order.ArrivalTime);
				
				Notice(myMessage);

				furniture.Order.FinishedFurnitureItemsCount++;
				
				// Kontrola či boli dokončené všetky kusy z objednávky
				if (furniture.Order.FinishedFurnitureItemsCount == furniture.Order.FurnitureItems.Count)
				{
					furniture.Order.State = OrderState.Completed;
					
					// Odobratie objednávky z frontu nedokončených objednávok
					MyMessage? orderMessage = null;
					var node = MyAgent.UnfinishedOrders.First;
					
					while (node != null)
					{
						if (node.Value.Order == furniture.Order)
						{
							orderMessage = node.Value;
							MyAgent.UnfinishedOrders.Remove(node);
							break;
						}
						
						node = node.Next;
					}
					
					if (orderMessage == null)
					{
						throw new Exception($"Order {furniture.Order.Id} not found in unfinished orders");
					}
					
					Response(orderMessage);
				}
				
				return;
			}
			
			// Zacneme spracovanie dalsieho kroku, treba vyziadat volneho pracovnika
			if (furniture.CurrentOperationStep == FurnitureOperationStep.Cut)
			{
				myMessage.RequestedWorkerType = [WorkerGroup.GroupC];
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Stained || furniture.CurrentOperationStep == FurnitureOperationStep.Varnished)
			{
				myMessage.RequestedWorkerType = [WorkerGroup.GroupB];
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Folded)
			{
				myMessage.RequestedWorkerType = [WorkerGroup.GroupC, WorkerGroup.GroupA];
			}
			else
			{
				throw new Exception($"Unexpected furniture step {furniture.CurrentOperationStep}");
			}
			
			myMessage.Addressee = MySim.FindAgent(SimId.WorkersAgent);
			myMessage.Code = Mc.RequestWorker;
			myMessage.Worker = null;
			Request(myMessage);
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
			case Mc.TransferWorker:
				ProcessTransferWorker(message);
			break;

			case Mc.ExecuteOperationStep:
				ProcessExecuteOperationStep(message);
			break;

			case Mc.ProcessOrder:
				ProcessProcessOrder(message);
			break;

			case Mc.RequestWorker:
				ProcessRequestWorker(message);
			break;

			case Mc.RequestAssemblyLine:
				ProcessRequestAssemblyLine(message);
			break;

			default:
				ProcessDefault(message);
			break;
			}
		}
		//meta! tag="end"
		public new ManufacturerAgent MyAgent
		{
			get
			{
				return (ManufacturerAgent)base.MyAgent;
			}
		}
	}
}