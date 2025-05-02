using OSPABA;
using Simulation;
using Agents.OperationAgent;
using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.FurnitureManufacturer.Utilities;

namespace Agents.OperationAgent.ContinualAssistants
{
	//meta! id="58"
	public class ExecuteOperationStepProcess : OSPABA.Process
	{
		private TriangularDistributionGenerator _materialPreparationTimeGenerator;

		private EmpiricalProbabilityGenerator _cuttingDeskTimeGenerator;
		private ContinuousUniformGenerator _cuttingChairTimeGenerator;
		private ContinuousUniformGenerator _cuttingClosetTimeGenerator;
		
		private ContinuousUniformGenerator _stainingDeskTimeGenerator;
		private ContinuousUniformGenerator _stainingChairTimeGenerator;
		private ContinuousUniformGenerator _stainingClosetTimeGenerator;
		
		private EmpiricalProbabilityGenerator _varnishingDeskTimeGenerator;
		private ContinuousUniformGenerator _varnishingChairTimeGenerator;
		private ContinuousUniformGenerator _varnishingClosetTimeGenerator;
		
		private ContinuousUniformGenerator _foldingDeskTimeGenerator;
		private ContinuousUniformGenerator _foldingChairTimeGenerator;
		private ContinuousUniformGenerator _foldingClosetTimeGenerator;
		
		private ContinuousUniformGenerator _assemblyOfFittingsOnClosetTimeGenerator;
		
		public ExecuteOperationStepProcess(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var mySimulation = (MySimulation)mySim;

			_materialPreparationTimeGenerator = new TriangularDistributionGenerator(300, 900, 500, mySimulation.SeedGenerator.Next());
			
			_cuttingDeskTimeGenerator = new EmpiricalProbabilityGenerator(
				isDiscrete: false,
				[
					new EmpiricalProbabilityTableItem(10 * 60, 25 * 60, 0.6),
					new EmpiricalProbabilityTableItem(25 * 60, 50 * 60, 0.4),
				],
				mySimulation.SeedGenerator
			);
			_cuttingChairTimeGenerator = new ContinuousUniformGenerator(12 * 60, 16 * 60, mySimulation.SeedGenerator.Next());
			_cuttingClosetTimeGenerator = new ContinuousUniformGenerator(15 * 60, 80 * 60, mySimulation.SeedGenerator.Next());
			
			_stainingDeskTimeGenerator = new ContinuousUniformGenerator(100 * 60, 480 * 60, mySimulation.SeedGenerator.Next());
			_stainingChairTimeGenerator = new ContinuousUniformGenerator(90 * 60, 400 * 60, mySimulation.SeedGenerator.Next());
			_stainingClosetTimeGenerator = new ContinuousUniformGenerator(300 * 60, 600 * 60, mySimulation.SeedGenerator.Next());
			
			_varnishingDeskTimeGenerator = new EmpiricalProbabilityGenerator(
				isDiscrete: false,
				[
					new EmpiricalProbabilityTableItem(50 * 60, 70 * 60, 0.1),
					new EmpiricalProbabilityTableItem(70 * 60, 150 * 60, 0.6),
					new EmpiricalProbabilityTableItem(150 * 60, 200 * 60, 0.3),
				],
				mySimulation.SeedGenerator
			);
			_varnishingChairTimeGenerator = new ContinuousUniformGenerator(40 * 60, 200 * 60, mySimulation.SeedGenerator.Next());
			_varnishingClosetTimeGenerator = new ContinuousUniformGenerator(250 * 60, 560 * 60, mySimulation.SeedGenerator.Next());
			
			_foldingDeskTimeGenerator = new ContinuousUniformGenerator(30 * 60, 60 * 60, mySimulation.SeedGenerator.Next());
			_foldingChairTimeGenerator = new ContinuousUniformGenerator(14 * 60, 24 * 60, mySimulation.SeedGenerator.Next());
			_foldingClosetTimeGenerator = new ContinuousUniformGenerator(35 * 60, 75 * 60, mySimulation.SeedGenerator.Next());
			
			_assemblyOfFittingsOnClosetTimeGenerator = new ContinuousUniformGenerator(15 * 60, 25 * 60, mySimulation.SeedGenerator.Next());
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="OperationAgent", id="59", type="Start"
		public void ProcessStart(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			myMessage.Code = Mc.OperationStepFinished;
			var furniture = myMessage.Furniture;
			
			// Podla aktualneho stavu nabytku sa vykona dany vyrobny krok
			if (furniture.CurrentOperationStep == FurnitureOperationStep.NotStarted)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.MaterialPreparationInWarehouse;
				var preparationDuration = _materialPreparationTimeGenerator.Next();
				
				myMessage.Warehouse.WarehouseSections[furniture.CurrentWorker.Id - 1].AnimatePreparationStep(preparationDuration);
				Hold(preparationDuration, myMessage);
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.MaterialPrepared)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Cutting;
				
				var operationDuration = furniture.Type switch
				{
					FurnitureType.Desk => _cuttingDeskTimeGenerator.Next(),
					FurnitureType.Chair => _cuttingChairTimeGenerator.Next(),
					FurnitureType.Closet => _cuttingClosetTimeGenerator.Next(),
					_ => throw new Exception()
				};

				furniture.CurrentAssemblyLine.AnimateOperationStep(operationDuration);
				Hold(operationDuration, myMessage);
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Cut)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Staining;
				
				var operationDuration = furniture.Type switch
				{
					FurnitureType.Desk => _stainingDeskTimeGenerator.Next(),
					FurnitureType.Chair => _stainingChairTimeGenerator.Next(),
					FurnitureType.Closet => _stainingClosetTimeGenerator.Next(),
					_ => throw new Exception()
				};
				
				furniture.CurrentAssemblyLine.AnimateOperationStep(operationDuration);
				Hold(operationDuration, myMessage);
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Stained && furniture.NeedsToBeVarnished)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Varnishing;
				
				var operationDuration = furniture.Type switch
				{
					FurnitureType.Desk => _varnishingDeskTimeGenerator.Next(),
					FurnitureType.Chair => _varnishingChairTimeGenerator.Next(),
					FurnitureType.Closet => _varnishingClosetTimeGenerator.Next(),
					_ => throw new Exception()
				};
				
				furniture.CurrentAssemblyLine.AnimateOperationStep(operationDuration);
				Hold(operationDuration, myMessage);
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Stained || furniture.CurrentOperationStep == FurnitureOperationStep.Varnished)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Folding;
				
				var operationDuration = furniture.Type switch
				{
					FurnitureType.Desk => _foldingDeskTimeGenerator.Next(),
					FurnitureType.Chair => _foldingChairTimeGenerator.Next(),
					FurnitureType.Closet => _foldingClosetTimeGenerator.Next(),
					_ => throw new Exception()
				};
				
				furniture.CurrentAssemblyLine.AnimateOperationStep(operationDuration);
				Hold(operationDuration, myMessage);
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Folded)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.AssemblingFittings;
				
				var operationDuration = furniture.Type switch
				{
					FurnitureType.Closet => _assemblyOfFittingsOnClosetTimeGenerator.Next(),
					_ => throw new Exception($"Only closet can be assembled - {furniture.Type}")
				};
				
				furniture.CurrentAssemblyLine.AnimateOperationStep(operationDuration);
				Hold(operationDuration, myMessage);
			}
			else
			{
				throw new Exception($"Unknown operation step {furniture.CurrentOperationStep}");
			}
		}

		public void ProcessOperationStepFinished(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			
			// Ak je dokončené skladanie a nie je to skriňa, nábytok je hotový
			if (myMessage.Furniture.Type != FurnitureType.Closet && myMessage.Furniture.CurrentOperationStep == FurnitureOperationStep.Folding)
			{
				myMessage.Furniture.CurrentOperationStep = FurnitureOperationStep.Completed;
			}
			else
			{
				// Aktualizacia aktualneho stavu nabytku
				myMessage.Furniture.CurrentOperationStep++;
			}
			
			// Poznačíme čas dokončenia výrobného kroku
			myMessage.Furniture.OperationStepEndTime = MySim.CurrentTime;

			if (myMessage.Furniture.CurrentOperationStep == FurnitureOperationStep.MaterialPrepared)
			{
				myMessage.Warehouse.WarehouseSections[myMessage.Furniture.CurrentWorker.Id - 1].HideOperationStepProgressBar();
			}
			else
			{
				myMessage.Furniture.CurrentAssemblyLine?.HideOperationStepProgressBar();
			}
			
			message.Addressee = MyAgent;
			AssistantFinished(message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.OperationStepFinished:
					ProcessOperationStepFinished(message);
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
		public new OperationAgent MyAgent
		{
			get
			{
				return (OperationAgent)base.MyAgent;
			}
		}
	}
}