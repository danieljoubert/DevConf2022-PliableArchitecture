using System.Data.Linq.Mapping;

namespace Demo
{
	[Table]
	public class Device : DataStorable<int>
	{
		[Column(IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.OnInsert, UpdateCheck = UpdateCheck.Never)]
		public override int Id { get; set; }

		[Column(UpdateCheck = UpdateCheck.Never)]
		public string Imei { get; set; }

		[Column(UpdateCheck = UpdateCheck.Never)]
		public bool IsActive { get; set; }
	}
}
