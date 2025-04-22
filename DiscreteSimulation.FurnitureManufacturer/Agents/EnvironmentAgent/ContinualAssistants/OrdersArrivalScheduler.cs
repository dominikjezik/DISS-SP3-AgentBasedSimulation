using OSPABA;
using Simulation;
using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace Agents.EnvironmentAgent.ContinualAssistants
{
	//meta! id="52"
	public class OrdersArrivalScheduler : OSPABA.Scheduler
	{
		private int _ordersCounter = 0;
		
		private ExponentialDistributionGenerator _newOrderArrivalGenerator;
		
		private ContinuousUniformGenerator _furnitureTypeGenerator;
		
		private ContinuousUniformGenerator _needsToBeVarnishedGenerator;
		
		private DiscreteUniformGenerator _countOfOrderItemsGenerator;
		
		public OrdersArrivalScheduler(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var mySimulation = (MySimulation)mySim;
			
			_newOrderArrivalGenerator = new ExponentialDistributionGenerator(1.0 / (60 * 30), mySimulation.SeedGenerator.Next());
			_furnitureTypeGenerator = new ContinuousUniformGenerator(0, 1, mySimulation.SeedGenerator.Next());
			_needsToBeVarnishedGenerator = new ContinuousUniformGenerator(0, 1, mySimulation.SeedGenerator.Next());
			_countOfOrderItemsGenerator = new DiscreteUniformGenerator(1, 6, mySimulation.SeedGenerator.Next());
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			_ordersCounter = 0;
		}

		//meta! sender="EnvironmentAgent", id="53", type="Start"
		public void ProcessStart(MessageForm message)
		{
			// Scheduler bezi po dobu exponencialneho rozdelenia
			message.Code = Mc.OrderArrived;
			Hold(_newOrderArrivalGenerator.Next(), message);
		}

		public void ProcessOrderArrived(MessageForm message)
		{
			// Prisla nova objednavka do systemu, oznamujeme agentovi
			var myMessage = (MyMessage)message;
			var order = GenerateOrder();
			
			myMessage.Order = order;
			myMessage.Addressee = MyAgent;
			Notice(myMessage);
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
		override public void ProcessMessage(MessageForm message)
		{
			switch (message.Code)
			{
			case Mc.Start:
				ProcessStart(message);
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
		
		private Order GenerateOrder()
		{
			var order = new Order
			{
				Id = ++_ordersCounter,
				State = OrderState.Pending,
				ArrivalTime = MySim.CurrentTime,
				StartedWaitingTime = MySim.CurrentTime
			};
			
			var countOfOrderItems = _countOfOrderItemsGenerator.Next();
			var furnitureItems = new List<Furniture>(countOfOrderItems);
			
			for (int i = 0; i < countOfOrderItems; i++)
			{
				var item = GenerateFurniture();
				item.Id = i + 1;
				item.Order = order;
				furnitureItems.Add(item);
			}
			
			order.FurnitureItems = furnitureItems;
        
			return order;
		}

		private Furniture GenerateFurniture()
		{
			FurnitureType furnitureType;
        
			var furnitureTypeProbability = _furnitureTypeGenerator.Next();
        
			if (furnitureTypeProbability < 0.5)
			{
				furnitureType = FurnitureType.Desk;
			}
			else if (furnitureTypeProbability < 0.65)
			{
				furnitureType = FurnitureType.Chair;
			}
			else
			{
				furnitureType = FurnitureType.Closet;
			}
			
			var needsToBeVarnishedProbability = _needsToBeVarnishedGenerator.Next();

			var furniture = new Furniture
			{
				Type = furnitureType,
				NeedsToBeVarnished = needsToBeVarnishedProbability < 0.15,
				StartedWaitingTime = MySim.CurrentTime,
				CurrentOperationStep = FurnitureOperationStep.NotStarted,
				State = "Pending"
			};
			
			return furniture;
		}
	}
}