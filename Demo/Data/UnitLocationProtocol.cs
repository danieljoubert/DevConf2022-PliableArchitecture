using System;

namespace Demo
{
	public class UnitLocationProtocol
	{
		public string DeviceType { get; set; }
		public string UniqueIdentifier { get; set; }
		public DateTime TimeStamp { get; set; }
		public double Lon { get; set; }
		public double Lat { get; set; }
		public int speed { get; set; }
	}
}
