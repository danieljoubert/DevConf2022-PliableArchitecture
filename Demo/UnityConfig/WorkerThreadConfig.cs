using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Demo
{
	[XmlRoot("workerThread")]
	public class WorkerThreadConfig
	{
		[XmlAttribute("type")]
		public string Type { get; set; }

		[XmlAttribute("numberOfInstances")]
		public int NumberOfInstances { get; set; }

		[XmlAttribute("dependencyName")]
		public string DependencyName { get; set; }

		[XmlAttribute("method")]
		public string MethodName { get; set; }

		[XmlAttribute("wait")]
		public string Wait { get; set; }

		[XmlElement("param")]
		public List<WorkerThreadParameter> Parameters { get; set; }

		[XmlElement("noThreads")]
		public int NoThreads { get; set; }
	}
}

