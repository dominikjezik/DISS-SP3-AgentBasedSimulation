using DiscreteSimulation.Core.Utilities;
using DiscreteSimulation.FurnitureManufacturer.Entities;
using OSPABA;
using Simulation;

namespace Agents.AssemblyLinesAgent
{
	//meta! id="11"
	public class AssemblyLinesAgent : OSPABA.Agent
	{
		public AssemblyLine[] AssemblyLines { get; private set; }

		public PriorityQueue<AssemblyLine, int> AvailableAssemblyLines = new();
		
		public EntitiesQueue<MyMessage> RequestsQueue;
		
		public Statistics RequestsQueueWaitingTime { get; private set; } = new();
		
		public AssemblyLinesAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
			RequestsQueue = new EntitiesQueue<MyMessage>(MySim);
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			var mySimulation = (MySimulation)MySim;
			
			RequestsQueue.Clear();
			ResetAssemblyLines(mySimulation.CountOfAssemblyLines);
			RequestsQueueWaitingTime.Clear();
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new AssemblyLinesManager(SimId.AssemblyLinesManager, MySim, this);
			AddOwnMessage(Mc.RequestAssemblyLine);
			AddOwnMessage(Mc.ReleaseAssemblyLine);
		}
		//meta! tag="end"
		
		private void ResetAssemblyLines(int count)
		{
			AssemblyLines = new AssemblyLine[count];
			AvailableAssemblyLines.Clear();
			
			for (int i = 0; i < count; i++)
			{
				var assemblyLine = new AssemblyLine(MySim)
				{
					Id = i + 1
				};
				
				AssemblyLines[i] = assemblyLine;
				AvailableAssemblyLines.Enqueue(assemblyLine, assemblyLine.Id);
			}
		}
	}
}