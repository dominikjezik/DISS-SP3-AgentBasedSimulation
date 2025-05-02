using System.Drawing;
using OSPABA;
using Agents.TransferAgent;
using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.FurnitureManufacturer.Utilities;
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
				
				var transferDuration = _arrivalTimeBetweenTwoLinesGenerator.Next();
				
				RunAnimationOnWorkerTransferBetweenLines(myMessage, transferDuration);
				
				// Odstranime pracovnika z jeho aktualnej linky
				myMessage.Worker.CurrentAssemblyLine.IdleWorkers.Remove(myMessage.Worker);
				myMessage.Worker.CurrentAssemblyLine = null;
				
				Hold(transferDuration, message);
			}
			else
			{
				var transferDuration = _arrivalTimeBetweenLineAndWarehouseGenerator.Next();
				
				if (myMessage.Worker.IsInWarehouse)
				{
					// Presun zo skladu na linku
					myMessage.Worker.IsMovingToAssemblyLine = true;
					
					RunAnimationOnWorkerTransferFromWarehouseToLine(myMessage, transferDuration);
				}
				else
				{
					// Presun z linky do skladu
					myMessage.Worker.IsMovingToWarehouse = true;
					
					RunAnimationOnWorkerTransferToWarehouse(myMessage, transferDuration);

					// Odstranime pracovnika z jeho aktualnej linky
					myMessage.Worker.CurrentAssemblyLine.IdleWorkers.Remove(myMessage.Worker);
					myMessage.Worker.CurrentAssemblyLine = null;
				}
				
				Hold(transferDuration, message);
			}
		}

		private void RunAnimationOnWorkerTransferFromWarehouseToLine(MyMessage message, double transferDuration)
		{
			var destinationAssemblyLine = message.AssemblyLine;

			PointF [] transferPath;
			
			if (message.Worker.Group == WorkerGroup.GroupA)
			{
				transferPath = [
					message.Warehouse.WarehouseSections[message.Worker.Id - 1].CurrentWorkerPosition,
					message.Warehouse.WarehouseSections[message.Worker.Id - 1].GatewayPosition,
					message.Warehouse.GatewayPosition,
					message.Warehouse.CrossroadPosition,
					destinationAssemblyLine.CrossroadPosition,
					destinationAssemblyLine.GatewayPosition,
					destinationAssemblyLine.EntrancePosition,
					destinationAssemblyLine.CurrentWorkerPosition
				];
			}
			else if (message.Worker.Group == WorkerGroup.GroupB)
			{
				transferPath = [
					message.Warehouse.WorkersGroupBIdlePosition,
					message.Warehouse.GatewayPositionIdleBCWorkers,
					message.Warehouse.GatewayPosition,
					message.Warehouse.CrossroadPosition,
					destinationAssemblyLine.CrossroadPosition,
					destinationAssemblyLine.GatewayPosition,
					destinationAssemblyLine.EntrancePosition,
					destinationAssemblyLine.CurrentWorkerPosition
				];
			}
			else
			{
				transferPath = [
					message.Warehouse.WorkersGroupCIdlePosition,
					message.Warehouse.GatewayPositionIdleBCWorkers,
					message.Warehouse.GatewayPosition,
					message.Warehouse.CrossroadPosition,
					destinationAssemblyLine.CrossroadPosition,
					destinationAssemblyLine.GatewayPosition,
					destinationAssemblyLine.EntrancePosition,
					destinationAssemblyLine.CurrentWorkerPosition
				];
			}
			
			message.Worker.AnimateTransfer(MySim.CurrentTime, transferDuration, transferPath);
		}
		
		private void RunAnimationOnWorkerTransferToWarehouse(MyMessage message, double transferDuration)
		{
			var currentAssemblyLine = message.Worker.CurrentAssemblyLine;
			
			var transferPath = new [] {
				currentAssemblyLine.GatewayPosition,
				currentAssemblyLine.CrossroadPosition,
				message.Warehouse.CrossroadPosition,
				message.Warehouse.GatewayPosition,
				message.Warehouse.WarehouseSections[message.Worker.Id - 1].GatewayPosition,
				message.Warehouse.WarehouseSections[message.Worker.Id - 1].CurrentWorkerPosition
			};
			
			message.Worker.AnimateTransfer(MySim.CurrentTime, transferDuration, transferPath);
		}

		private void RunAnimationOnWorkerTransferBetweenLines(MyMessage message, double transferDuration)
		{
			var currentAssemblyLine = message.Worker.CurrentAssemblyLine;
			var destinationAssemblyLine = message.AssemblyLine;
			
			PointF[] transferPath;
			
			// Ak je predchadzajuca a nova linka na rovnakej urovni, nemuime ísť na crossroad
			if (currentAssemblyLine.GatewayPosition == destinationAssemblyLine.GatewayPosition)
			{
				transferPath = [
					currentAssemblyLine.GatewayPosition,
					destinationAssemblyLine.GatewayPosition,
					destinationAssemblyLine.EntrancePosition,
					destinationAssemblyLine.CurrentWorkerPosition
				];
			}
			else
			{
				transferPath = [
					currentAssemblyLine.GatewayPosition,
					currentAssemblyLine.CrossroadPosition,
					destinationAssemblyLine.CrossroadPosition,
					destinationAssemblyLine.GatewayPosition,
					destinationAssemblyLine.EntrancePosition,
					destinationAssemblyLine.CurrentWorkerPosition
				];
			}
			
			message.Worker.AnimateTransfer(MySim.CurrentTime, transferDuration, transferPath);
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

				myMessage.Worker.PlaceWorker(myMessage.AssemblyLine.CurrentWorkerPosition);
			}
			else
			{
				if (myMessage.Worker.IsMovingToWarehouse)
				{
					// Pracovnik sa presunul do skladu
					myMessage.Worker.IsMovingToWarehouse = false;
					myMessage.Worker.IsInWarehouse = true;
					
					myMessage.Worker.PlaceWorker(myMessage.Warehouse.WarehouseSections[myMessage.Worker.Id - 1].CurrentWorkerPosition);
				}
				else
				{
					// Pracovnik sa presunul na linku
					myMessage.Worker.IsMovingToAssemblyLine = false;
					myMessage.Worker.IsInWarehouse = false;
					myMessage.Worker.CurrentAssemblyLine = myMessage.AssemblyLine;
					myMessage.AssemblyLine.CurrentWorker = myMessage.Worker;
					
					myMessage.Worker.PlaceWorker(myMessage.AssemblyLine.CurrentWorkerPosition);
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