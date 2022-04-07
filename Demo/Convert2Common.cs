using System.Collections.Generic;

namespace Demo
{
	internal class Convert2Common : ITask<UnitLocationProtocol, Location>
	{
		public Convert2Common(IDictionary<string, IConvert2Common> converters)
		{
			_converters = converters;
		}

		private readonly IDictionary<string, IConvert2Common> _converters;

		public Location Execute(UnitLocationProtocol value)
		{
			if(_converters.TryGetValue(value.DeviceType, out IConvert2Common converter))
				return converter.Convert(value);
			return null;
		}
	}
}
