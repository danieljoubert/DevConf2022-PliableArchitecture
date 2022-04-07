using System.Xml;
using System.Xml.Serialization;

namespace Demo
{
	[XmlRoot("param")]
	public class WorkerThreadParameter
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("dependencyName")]
		public string DependencyName { get; set; }

		[XmlAttribute("dependencyType")]
		public string DependencyType { get; set; }

		[XmlAttribute("value")]
		public string Value { get; set; }

		[XmlAttribute("typeConverter")]
		public string TypeConverter { get; set; }
	}
}

