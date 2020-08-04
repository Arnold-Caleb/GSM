using System;
using GsmComm.GsmCommunication;

namespace GSMLib
{
	
	public class CommSetting
	{
		public static int Comm_Port=GsmCommMain.DefaultPortNumber;
		public static Int64 Comm_BaudRate=9600;
		public static Int64 Comm_TimeOut=GsmCommMain.DefaultTimeout;
		public static GsmCommMain comm;

		public CommSetting()
		{
			// ...
			// ...
			// ...
		}		
	}
}
