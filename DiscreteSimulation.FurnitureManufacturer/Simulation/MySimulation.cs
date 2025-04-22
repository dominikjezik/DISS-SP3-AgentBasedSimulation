using Agents.WorkersGroupBAgent;
using Agents.TransferAgent;
using Agents.WorkersAgent;
using Agents.WorkersGroupAAgent;
using Agents.OperationAgent;
using Agents.AssemblyLinesAgent;
using Agents.EnvironmentAgent;
using Agents.ManufacturerAgent;
using Agents.ModelAgent;
using Agents.WorkersGroupCAgent;
using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.Core.Utilities;

namespace Simulation
{
	public class MySimulation : OSPABA.Simulation
	{
		public SeedGenerator SeedGenerator { get; private set; }
		
		#region Parameters
		
		public int CountOfWorkersGroupA { get; set; }
    
		public int CountOfWorkersGroupB { get; set; }
    
		public int CountOfWorkersGroupC { get; set; }
		
		public int CountOfAssemblyLines { get; set; }
    
		public bool EnableWorkerLocationPreference { get; set; } = true;
		
		#endregion
		
		#region SimulationStatistics
		
		public Statistics AverageProcessingOrderTime { get; private set; } = new();
		
		public Statistics AverageProcessingFurnitureTime { get; private set; } = new();
		
		public Statistics AveragePendingOrdersCount { get; private set; } = new();
    
		public Statistics AveragePendingItemsForLineCount { get; private set; } = new();
    
		public Statistics AveragePendingItemsForWorkerACount { get; private set; } = new();
    
		public Statistics AveragePendingItemsForWorkerCCount { get; private set; } = new();
		
		public Statistics AveragePendingItemsForWorkerBCount { get; private set; } = new();
		
		public Statistics AveragePendingItemsForWorkerMixCount { get; private set; } = new();
    
		public Statistics AveragePendingOrdersWaitingTime { get; private set; } = new();
		
		public Statistics AveragePendingItemsForLineWaitingTime { get; private set; } = new();
		
		public Statistics AveragePendingItemsForWorkerAWaitingTime { get; private set; } = new();
		
		public Statistics AveragePendingItemsForWorkerBWaitingTime { get; private set; } = new();
		
		public Statistics AveragePendingItemsForWorkerCWaitingTime { get; private set; } = new();
		
		public Statistics AveragePendingItemsForWorkerMixWaitingTime { get; private set; } = new();
		
		public Statistics AverageAssemblyLinesUtilization { get; private set; } = new();
		
		public Statistics AverageWorkersGroupAUtilization { get; private set; } = new();
    
		public Statistics AverageWorkersGroupBUtilization { get; private set; } = new();
    
		public Statistics AverageWorkersGroupCUtilization { get; private set; } = new();
    
		public Statistics[] AverageAllAssemblyLinesUtilization { get; private set; }
		
		public Statistics[] AverageAllWorkersUtilization { get; private set; }

		#endregion
		
		public MySimulation()
		{
			SeedGenerator = new SeedGenerator();
			Init();
		}

		override public void PrepareSimulation()
		{
			base.PrepareSimulation();
			ClearStatistics();
		}

		private void ClearStatistics()
		{
			AverageProcessingOrderTime.Clear();
			AverageProcessingFurnitureTime.Clear();
			
			AveragePendingOrdersCount.Clear();
			AveragePendingItemsForLineCount.Clear();
			AveragePendingItemsForWorkerACount.Clear();
			AveragePendingItemsForWorkerBCount.Clear();
			AveragePendingItemsForWorkerCCount.Clear();
			AveragePendingItemsForWorkerMixCount.Clear();
			
			AveragePendingOrdersWaitingTime.Clear();
			AveragePendingItemsForLineWaitingTime.Clear();
			AveragePendingItemsForWorkerAWaitingTime.Clear();
			AveragePendingItemsForWorkerBWaitingTime.Clear();
			AveragePendingItemsForWorkerCWaitingTime.Clear();
			AveragePendingItemsForWorkerMixWaitingTime.Clear();

			AverageAssemblyLinesUtilization.Clear();
			
			AverageWorkersGroupAUtilization.Clear();
			AverageWorkersGroupBUtilization.Clear();
			AverageWorkersGroupCUtilization.Clear();
			
			AverageAllAssemblyLinesUtilization = new Statistics[CountOfAssemblyLines];
			
			for (var i = 0; i < AverageAllAssemblyLinesUtilization.Length; i++)
			{
				AverageAllAssemblyLinesUtilization[i] = new Statistics();
			}
			
			AverageAllWorkersUtilization = new Statistics[CountOfWorkersGroupA + CountOfWorkersGroupB + CountOfWorkersGroupC];
			
			for (var i = 0; i < AverageAllWorkersUtilization.Length; i++)
			{
				AverageAllWorkersUtilization[i] = new Statistics();
			}
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Reset entities, queues, local statistics, etc...
		}

		override public void ReplicationFinished()
		{
			// Collect local statistics into global, update UI, etc...
			
			// Ak bolo vykonávanie replikácie prerušené zastavením simulácie pouzívateľom
			// výsledky tejto replikácie sa nezapočítavajú do štatistík
			if (!IsRunning())
			{
				return;
			}
			
			AverageProcessingOrderTime.AddValue(EnvironmentAgent.ProcessingOrderTime.Mean);
			AverageProcessingFurnitureTime.AddValue(ManufacturerAgent.ProcessingFurnitureTime.Mean);
			
			AveragePendingOrdersCount.AddValue(ManufacturerAgent.PendingOrders.AverageQueueLength);
			AveragePendingItemsForLineCount.AddValue(AssemblyLinesAgent.RequestsQueue.AverageQueueLength);
			AveragePendingItemsForWorkerACount.AddValue(WorkersGroupAAgent.WorkersRequestsQueue.AverageQueueLength);
			AveragePendingItemsForWorkerBCount.AddValue(WorkersGroupBAgent.WorkersRequestsQueue.AverageQueueLength);
			AveragePendingItemsForWorkerCCount.AddValue(WorkersGroupCAgent.WorkersRequestsQueue.AverageQueueLength);
			AveragePendingItemsForWorkerMixCount.AddValue(WorkersAgent.MixedWorkersRequestsQueue.AverageQueueLength);
			
			AveragePendingOrdersWaitingTime.AddValue(ManufacturerAgent.PendingOrdersWaitingTime.Mean);
			AveragePendingItemsForLineWaitingTime.AddValue(AssemblyLinesAgent.RequestsQueueWaitingTime.Mean);
			AveragePendingItemsForWorkerAWaitingTime.AddValue(WorkersAgent.WorkersARequestsQueueWaitingTime.Mean);
			AveragePendingItemsForWorkerBWaitingTime.AddValue(WorkersAgent.WorkersBRequestsQueueWaitingTime.Mean);
			AveragePendingItemsForWorkerCWaitingTime.AddValue(WorkersAgent.WorkersCRequestsQueueWaitingTime.Mean);
			AveragePendingItemsForWorkerMixWaitingTime.AddValue(WorkersAgent.WorkersMixRequestsQueueWaitingTime.Mean);
			
			for (var i = 0; i < AssemblyLinesAgent.AssemblyLines.Length; i++)
			{
				AssemblyLinesAgent.AssemblyLines[i].RefreshStatistics();
				AverageAllAssemblyLinesUtilization[i].AddValue(AssemblyLinesAgent.AssemblyLines[i].Utilization);
			}
			
			AverageAssemblyLinesUtilization.AddValue(AssemblyLinesAgent.AssemblyLines.Average(assemblyLine => assemblyLine.Utilization));
			
	        for (var i = 0; i < WorkersGroupAAgent.Workers.Length; i++)
	        {
		        WorkersGroupAAgent.Workers[i].RefreshStatistics();
		        AverageAllWorkersUtilization[i].AddValue(WorkersGroupAAgent.Workers[i].Utilization);
	        }
	        
	        for (var i = 0; i < WorkersGroupBAgent.Workers.Length; i++)
	        {
		        WorkersGroupBAgent.Workers[i].RefreshStatistics();
	            AverageAllWorkersUtilization[WorkersGroupAAgent.Workers.Length + i].AddValue(WorkersGroupBAgent.Workers[i].Utilization);
	        }
	        
	        for (var i = 0; i < WorkersGroupCAgent.Workers.Length; i++)
	        {
		        WorkersGroupCAgent.Workers[i].RefreshStatistics();
	            AverageAllWorkersUtilization[WorkersGroupAAgent.Workers.Length + WorkersGroupBAgent.Workers.Length + i].AddValue(WorkersGroupCAgent.Workers[i].Utilization);
	        }
	        
	        AverageWorkersGroupAUtilization.AddValue(WorkersGroupAAgent.Workers.Average(worker => worker.Utilization));
	        AverageWorkersGroupBUtilization.AddValue(WorkersGroupBAgent.Workers.Average(worker => worker.Utilization));
	        AverageWorkersGroupCUtilization.AddValue(WorkersGroupCAgent.Workers.Average(worker => worker.Utilization));
	        
	        base.ReplicationFinished();
		}

		override public void SimulationFinished()
		{
			// Display simulation results
			base.SimulationFinished();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			ModelAgent = new ModelAgent(SimId.ModelAgent, this, null);
			EnvironmentAgent = new EnvironmentAgent(SimId.EnvironmentAgent, this, ModelAgent);
			ManufacturerAgent = new ManufacturerAgent(SimId.ManufacturerAgent, this, ModelAgent);
			AssemblyLinesAgent = new AssemblyLinesAgent(SimId.AssemblyLinesAgent, this, ManufacturerAgent);
			TransferAgent = new TransferAgent(SimId.TransferAgent, this, ManufacturerAgent);
			WorkersAgent = new WorkersAgent(SimId.WorkersAgent, this, ManufacturerAgent);
			OperationAgent = new OperationAgent(SimId.OperationAgent, this, ManufacturerAgent);
			WorkersGroupAAgent = new WorkersGroupAAgent(SimId.WorkersGroupAAgent, this, WorkersAgent);
			WorkersGroupBAgent = new WorkersGroupBAgent(SimId.WorkersGroupBAgent, this, WorkersAgent);
			WorkersGroupCAgent = new WorkersGroupCAgent(SimId.WorkersGroupCAgent, this, WorkersAgent);
		}
		public ModelAgent ModelAgent
		{ get; set; }
		public EnvironmentAgent EnvironmentAgent
		{ get; set; }
		public ManufacturerAgent ManufacturerAgent
		{ get; set; }
		public AssemblyLinesAgent AssemblyLinesAgent
		{ get; set; }
		public TransferAgent TransferAgent
		{ get; set; }
		public WorkersAgent WorkersAgent
		{ get; set; }
		public OperationAgent OperationAgent
		{ get; set; }
		public WorkersGroupAAgent WorkersGroupAAgent
		{ get; set; }
		public WorkersGroupBAgent WorkersGroupBAgent
		{ get; set; }
		public WorkersGroupCAgent WorkersGroupCAgent
		{ get; set; }
		//meta! tag="end"
	}
}