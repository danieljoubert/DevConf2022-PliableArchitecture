namespace Demo
{
	public class DataFilter : ITask<Location, Location>
	{
		public DataFilter(IDataContext dataContext)
		{
			Guard.ArgumentNotNull(dataContext, nameof(dataContext));
			_dataContext = dataContext;
		}

		private readonly IDataContext _dataContext;

		public Location Execute(Location value)
		{
			if (value == null)
				return null;
			var devices = _dataContext.RetrieveAll<Device>(d => d.Imei == value.Imei);
			if (devices.Count == 0)
				return null;
			return value;
		}
	}
}
