using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Demo
{
	public class XmlSerializerTask<T> : ITask<T, string>
	{
		public string Execute(T value)
		{
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add("", "");
			XmlSerializer serializer = new XmlSerializer(value.GetType());
			var s = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
			using (var ms = new MemoryStream())
			using (XmlWriter stringWriter = XmlWriter.Create(ms, s))
			{
				serializer.Serialize(stringWriter, value, namespaces);
				ms.Position = 0;
				StreamReader reader = new StreamReader(ms);
				return reader.ReadToEnd();
			}
		}
	}
}
