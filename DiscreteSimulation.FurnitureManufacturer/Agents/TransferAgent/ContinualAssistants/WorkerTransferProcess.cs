using OSPABA;
using Agents.TransferAgent;
using DiscreteSimulation.Core.Generators;
using Simulation;
namespace Agents.TransferAgent.ContinualAssistants
{
	//meta! id="55"
	public class WorkerTransferProcess : OSPABA.Process
	{
		private TriangularDistributionGenerator _arrivalTimeBetweenLineAndWarehouseGenerator;

		private TriangularDistributionGenerator _arrivalTimeBetweenTwoLinesGenerator;
		
		public WorkerTransferProcess(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var mySimulation = (MySimulation)mySim;
			
			_arrivalTimeBetweenLineAndWarehouseGenerator = new TriangularDistributionGenerator(60, 480, 120, mySimulation.SeedGenerator.Next());
			
			_arrivalTimeBetweenTwoLinesGenerator = new TriangularDistributionGenerator(120, 500, 150, mySimulation.SeedGenerator.Next());
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="TransferAgent", id="56", type="Start"
		public void ProcessStart(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			myMessage.Code = Mc.WorkerTransferFinished;
			
			// Zacina sa presun pracovnika
			if (myMessage.IsTransferBetweenLines)
			{
				// Presun medzi linkami
				myMessage.Worker.IsMovingToAssemblyLine = true;
				
				// Odstranime pracovnika z jeho aktualnej linky
				myMessage.Worker.CurrentAssemblyLine.IdleWorkers.Remove(myMessage.Worker);
				myMessage.Worker.CurrentAssemblyLine = null;
				
				Hold(_arrivalTimeBetweenTwoLinesGenerator.Next(), message);
			}
			else
			{
				if (myMessage.Worker.IsInWarehouse)
				{
					// Presun zo skladu na linku
					myMessage.Worker.IsMovingToAssemblyLine = true;
				}
				else
				{
					// Presun z linky do skladu
					myMessage.Worker.IsMovingToWarehouse = true;

					// Odstranime pracovnika z jeho aktualnej linky
					myMessage.Worker.CurrentAssemblyLine.IdleWorkers.Remove(myMessage.Worker);
					myMessage.Worker.CurrentAssemblyLine = null;
				}
				
				Hold(_arrivalTimeBetweenLineAndWarehouseGenerator.Next(), message);
			}
		}
		
		public void ProcessWorkerTransferFinished(MessageForm message)
		{
			var myMessage = (MyMessage)message;

			if (myMessage.IsTransferBetweenLines)
			{
				// Pracovnik sa presunul na novu linku
				myMessage.Worker.IsMovingToAssemblyLine = false;
				myMessage.Worker.CurrentAssemblyLine = myMessage.AssemblyLine;
				myMessage.AssemblyLine.CurrentWorker = myMessage.Worker;
			}
			else
			{
				if (myMessage.Worker.IsMovingToWarehouse)
				{
					// Pracovnik sa presunul do skladu
					myMessage.Worker.IsMovingToWarehouse = false;
					myMessage.Worker.IsInWarehouse = true;
				}
				else
				{
					// Pracovnik sa presunul na linku
					myMessage.Worker.IsMovingToAssemblyLine = false;
					myMessage.Worker.IsInWarehouse = false;
					myMessage.Worker.CurrentAssemblyLine = myMessage.AssemblyLine;
					myMessage.AssemblyLine.CurrentWorker = myMessage.Worker;
				}
			}
			
			message.Addressee = MyAgent;
			AssistantFinished(message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.WorkerTransferFinished:
					ProcessWorkerTransferFinished(message);
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
		public new TransferAgent MyAgent
		{
			get
			{
				return (TransferAgent)base.MyAgent;
			}
		}
	}
}