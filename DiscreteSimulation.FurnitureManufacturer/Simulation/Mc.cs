using OSPABA;
namespace Simulation
{
	public class Mc : OSPABA.IdList
	{
		//meta! userInfo="Generated code: do not modify", tag="begin"
		public const int ReleaseAssemblyLine = 1025;
		public const int WorkerIsAvailable = 1026;
		public const int ProcessOrder = 1001;
		public const int RequestWorkerIfAvailable = 1028;
		public const int OrderArrived = 1002;
		public const int OrderFinished = 1003;
		public const int Initialize = 1004;
		public const int RequestWorker = 1006;
		public const int TransferWorker = 1007;
		public const int ExecuteOperationStep = 1010;
		public const int ReleaseWorker = 1012;
		public const int RequestAssemblyLine = 1024;
		//meta! tag="end"

		// 1..1000 range reserved for user
		public const int WorkerTransferFinished = 1;
		public const int OperationStepFinished = 2;
	}
}