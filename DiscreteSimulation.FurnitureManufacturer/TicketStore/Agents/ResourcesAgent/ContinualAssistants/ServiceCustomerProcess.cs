using Agents.ResourcesAgent;
using DiscreteSimulation.Core.Generators;
using OSPABA;
using Simulation;
namespace Agents.ResourcesAgent.ContinualAssistants
{
	//meta! id="13"
	public class ServiceCustomerProcess : OSPABA.Process
	{
		private ExponentialDistributionGenerator _customerServiceGenerator;
		
		public ServiceCustomerProcess(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var mySimulation = (MySimulation)mySim;
			_customerServiceGenerator = new(1.0/13, mySimulation.SeedGenerator.Next());
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="ResourcesAgent", id="14", type="Start"
		public void ProcessStart(MessageForm message)
		{
			// Zacina sa obsluha zákazníka, nachystame ukoncenie obsluhy
			message.Code = Mc.CustomerServiceEnded;
			Hold(_customerServiceGenerator.Next(), message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.CustomerServiceEnded:
					message.Addressee = MyAgent;
					AssistantFinished(message);
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
		public new ResourcesAgent MyAgent
		{
			get
			{
				return (ResourcesAgent)base.MyAgent;
			}
		}
	}
}