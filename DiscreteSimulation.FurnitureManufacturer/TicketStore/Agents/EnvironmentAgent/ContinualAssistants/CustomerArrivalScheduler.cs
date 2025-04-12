using OSPABA;
using Simulation;
using Agents.EnvironmentAgent;
using DiscreteSimulation.Core.Generators;

namespace Agents.EnvironmentAgent.ContinualAssistants
{
	//meta! id="8"
	public class CustomerArrivalScheduler : OSPABA.Scheduler
	{
		private ExponentialDistributionGenerator _customerArrivalGenerator;
		
		public CustomerArrivalScheduler(int id, OSPABA.Simulation mySim, CommonAgent myAgent) :
			base(id, mySim, myAgent)
		{
			var mySimulation = (MySimulation)mySim;
			_customerArrivalGenerator = new(1.0/15, mySimulation.SeedGenerator.Next());
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
		}

		//meta! sender="EnvironmentAgent", id="9", type="Start"
		public void ProcessStart(MessageForm message)
		{
			// Ked manazer spusti tento scheduler, tak ho nechame bezat po dobu exp rozdelenia
			// odosielame spravu samemu sebe po uplynuti casu
			message.Code = Mc.CustomerArrival;
			Hold(_customerArrivalGenerator.Next(), message);
		}

		//meta! userInfo="Process messages defined in code", id="0"
		public void ProcessDefault(MessageForm message)
		{
			switch (message.Code)
			{
				case Mc.CustomerArrival:
					// Nastal prichod noveho zakaznika, preto to oznamime agentovi
					message.Addressee = MyAgent;
					Notice(message);
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
	}
}