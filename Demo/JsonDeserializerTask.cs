using System.Web.Script.Serialization;

namespace Demo
{
	public class JsonDeserializerTask<T> : ITask<string, T>
	{
		private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

		public T Execute(string value)
		{
			return _serializer.Deserialize<T>(value);
		}
	}
}
