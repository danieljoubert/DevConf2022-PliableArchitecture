using System.Xml;
using System.Xml.Serialization;

namespace Demo
{
	public class XmlDeserializerTask<T> : ITask<string, T>
	{
		public T Execute(string value)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(T));
			using (XmlReader stringReader = new XmlTextReader(value, XmlNodeType.Document, null))
				return (T)serializer.Deserialize(stringReader);
		}
	}
}
