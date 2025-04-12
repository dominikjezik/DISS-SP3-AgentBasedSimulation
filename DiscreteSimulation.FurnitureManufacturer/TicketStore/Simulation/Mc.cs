using OSPABA;
namespace Simulation
{
	public class Mc : OSPABA.IdList
	{
		//meta! userInfo="Generated code: do not modify", tag="begin"
		public const int Initialize = 1003;
		public const int CustomerLeave = 1004;
		public const int CustomerArrival = 1001;
		public const int ServiceCustomer = 1002;
		//meta! tag="end"

		// 1..1000 range reserved for user
		public const int CustomerServiceEnded = 1;
	}
}