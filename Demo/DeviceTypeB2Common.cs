namespace Demo
{
	internal class DeviceTypeB2Common : IConvert2Common
	{
		public Location Convert(UnitLocationProtocol value)
		{
			return new Location
			{
				Imei = value.UniqueIdentifier,
				DateTime = value.TimeStamp,
				Latitude = value.Lat,
				Longitude = value.Lon,
				Speed = value.speed
			};
		}
	}
}
