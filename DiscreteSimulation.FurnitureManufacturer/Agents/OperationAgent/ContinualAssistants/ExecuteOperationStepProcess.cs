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
					new EmpiricalProbabilityTableItem(25 * 70, 150 * 60, 0.6),
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
				Hold(_materialPreparationTimeGenerator.Next(), myMessage);
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.MaterialPrepared)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Cutting;
				switch (furniture.Type)
				{
					case FurnitureType.Desk:
						Hold(_cuttingDeskTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Chair:
						Hold(_cuttingChairTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Closet:
						Hold(_cuttingClosetTimeGenerator.Next(), myMessage);
						break;
					default:
						throw new Exception();
				}
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Cut)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Staining;
				switch (furniture.Type)
				{
					case FurnitureType.Desk:
						Hold(_stainingDeskTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Chair:
						Hold(_stainingChairTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Closet:
						Hold(_stainingClosetTimeGenerator.Next(), myMessage);
						break;
					default:
						throw new Exception();
				}
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Stained)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Varnishing;
				switch (furniture.Type)
				{
					case FurnitureType.Desk:
						Hold(_varnishingDeskTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Chair:
						Hold(_varnishingChairTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Closet:
						Hold(_varnishingClosetTimeGenerator.Next(), myMessage);
						break;
					default:
						throw new Exception();
				}
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Varnished)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.Folding;
				switch (furniture.Type)
				{
					case FurnitureType.Desk:
						Hold(_foldingDeskTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Chair:
						Hold(_foldingChairTimeGenerator.Next(), myMessage);
						break;
					case FurnitureType.Closet:
						Hold(_foldingClosetTimeGenerator.Next(), myMessage);
						break;
					default:
						throw new Exception();
				}
			}
			else if (furniture.CurrentOperationStep == FurnitureOperationStep.Folded)
			{
				furniture.CurrentOperationStep = FurnitureOperationStep.AssemblingFittings;
				
				if (furniture.Type == FurnitureType.Closet)
				{
					Hold(_assemblyOfFittingsOnClosetTimeGenerator.Next(), myMessage);
				}
				else
				{
					throw new Exception($"Only closet can be assembled - {furniture.Type}");
				}
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