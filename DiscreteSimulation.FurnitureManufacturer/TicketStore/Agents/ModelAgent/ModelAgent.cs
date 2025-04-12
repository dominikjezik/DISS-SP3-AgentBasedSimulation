using OSPABA;
using Simulation;

namespace Agents.ModelAgent
{
	//meta! id="1"
	public class ModelAgent : OSPABA.Agent
	{
		public ModelAgent(int id, OSPABA.Simulation mySim, Agent parent) :
			base(id, mySim, parent)
		{
			Init();
		}

		override public void PrepareReplication()
		{
			base.PrepareReplication();
			// Setup component for the next replication
			
			// Vytvorenie prvotnej spravy inicializacie
			var message = new MyMessage(MySim)
			{
				Addressee = this,
				Code = Mc.Initialize
			};
			MyManager.Notice(message);
		}

		//meta! userInfo="Generated code: do not modify", tag="begin"
		private void Init()
		{
			new ModelManager(SimId.ModelManager, MySim, this);
			AddOwnMessage(Mc.CustomerArrival);
			AddOwnMessage(Mc.ServiceCustomer);
		}
		//meta! tag="end"
	}
}