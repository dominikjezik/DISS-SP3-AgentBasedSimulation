using DiscreteSimulation.FurnitureManufacturer.Entities;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
using OSPABA;
namespace Simulation
{
	public class MyMessage : OSPABA.MessageForm
	{
		public Order Order { get; set; }
		
		public Furniture Furniture { get; set; }
		
		public AssemblyLine AssemblyLine { get; set; }
		
		public Worker Worker { get; set; }
		
		public bool IsTransferBetweenLines { get; set; }

		public WorkerGroup[] RequestedWorkerType { get; set; }

		public bool NotifyIfWorkerIsAvailable { get; set; } = false;

		public MyMessage(OSPABA.Simulation mySim) :
			base(mySim)
		{
		}

		public MyMessage(MyMessage original) :
			base(original)
		{
			// copy() is called in superclass
		}

		override public MessageForm CreateCopy()
		{
			return new MyMessage(this);
		}

		override protected void Copy(MessageForm message)
		{
			base.Copy(message);
			MyMessage original = (MyMessage)message;
			
			// Copy attributes
			Order = original.Order;
			Furniture = original.Furniture;
			AssemblyLine = original.AssemblyLine;
			Worker = original.Worker;
			IsTransferBetweenLines = original.IsTransferBetweenLines;
			RequestedWorkerType = original.RequestedWorkerType;
			NotifyIfWorkerIsAvailable = original.NotifyIfWorkerIsAvailable;
		}
	}
}