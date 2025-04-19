using OSPABA;
using Simulation;
namespace Agents.AssemblyLinesAgent
{
	//meta! id="11"
	public class AssemblyLinesManager : OSPABA.Manager
	{
		public AssemblyLinesManager(int id, OSPABA.Simulation mySim, Agent myAgent) :
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

		//meta! sender="ManufacturerAgent", id="63", type="Request"
		public void ProcessRequestAssemblyLine(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			myMessage.Furniture.StartedWaitingTime = MySim.CurrentTime;
			
			if (MyAgent.AvailableAssemblyLines.Count > 0)
			{
				var assemblyLine = MyAgent.AvailableAssemblyLines.Dequeue();
				myMessage.AssemblyLine = assemblyLine;
				
				MyAgent.RequestsQueueWaitingTime.AddValue(0);
				
				Response(myMessage);
			}
			else
			{
				MyAgent.RequestsQueue.Enqueue(myMessage);
			}
		}

		//meta! sender="ManufacturerAgent", id="64", type="Notice"
		public void ProcessReleaseAssemblyLine(MessageForm message)
		{
			var myMessage = (MyMessage)message;
			var assemblyLine = myMessage.AssemblyLine;
			
			if (MyAgent.RequestsQueue.Count > 0)
			{
				myMessage = MyAgent.RequestsQueue.Dequeue();
				myMessage.AssemblyLine = assemblyLine;
				
				MyAgent.RequestsQueueWaitingTime.AddValue(MySim.CurrentTime - myMessage.Furniture.StartedWaitingTime);
				
				Response(myMessage);
			}
			else
			{
				MyAgent.AvailableAssemblyLines.Enqueue(assemblyLine, assemblyLine.Id);
			}
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
			case Mc.ReleaseAssemblyLine:
				ProcessReleaseAssemblyLine(message);
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
		public new AssemblyLinesAgent MyAgent
		{
			get
			{
				return (AssemblyLinesAgent)base.MyAgent;
			}
		}
	}
}