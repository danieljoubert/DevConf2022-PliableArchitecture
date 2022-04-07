using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Practices.Unity.Configuration;
using Unity;
using System.ComponentModel;
using System.Globalization;

namespace Demo
{
	public class WorkerThreadElement : InstanceElement
  {
		private WorkerThreadConfig _config;

		public override void Deserialize(XmlReader reader)
		{
			string xml = reader.ReadOuterXml().Trim();
			XmlSerializer oSerializer = new XmlSerializer(typeof(WorkerThreadConfig));
			using (XmlReader stringReader = new XmlTextReader(xml, XmlNodeType.Document, null))
				_config = (WorkerThreadConfig)oSerializer.Deserialize(stringReader);
		}

		protected override void ConfigureContainer(IUnityContainer container)
    {
      Type type = Type.GetType(_config.Type);
      int noInstances = _config.NumberOfInstances == 0 ? 1 : _config.NumberOfInstances;
      for (int i = 0; i < noInstances; i++)
      {
        object instance = container.Resolve(type, _config.DependencyName);
				string[] parameterNames = _config.Parameters.Select(p => p.Name).ToArray();
				MethodInfo methodInfo = GetMethod(type, _config.MethodName, parameterNames);
				var parameters = GetParameterValues(methodInfo, container);
				TimeSpan sleep = TimeSpan.Zero;
				if(!string.IsNullOrWhiteSpace(_config.Wait))
					sleep = TimeSpan.ParseExact(_config.Wait, "c", CultureInfo.InvariantCulture);
				int noThreads = _config.NoThreads == 0 ? 1 : _config.NoThreads;
				WorkerThread thread = new WorkerThread(instance, methodInfo, parameters, sleep, noThreads);
				thread.Start();
        container.RegisterInstance(type, Guid.NewGuid().ToString(), thread);
      }
    }

		private static MethodInfo GetMethod(Type type, string methodName, string[] paramNames)
		{
			return type
				.GetMethods()
				.Where(m => m.Name == methodName && m.GetParameters().Length == paramNames.Length)
				.FirstOrDefault(m => m.GetParameters().All(p => paramNames.Contains(p.Name)));
		}

		private object[] GetParameterValues(MethodInfo methodInfo, IUnityContainer container)
		{
			return _config.Parameters.Select(p => GetParameterValue(methodInfo, p, container)).ToArray();
		}

		private static object GetParameterValue(MethodInfo methodInfo, WorkerThreadParameter parameter, IUnityContainer container)
		{
			ParameterInfo parameterInfo = methodInfo.GetParameters().FirstOrDefault(p => p.Name == parameter.Name);
			return GetParameterValue(parameterInfo, parameter, container);
		}

		private static object GetParameterValue(ParameterInfo info, WorkerThreadParameter parameter, IUnityContainer container)
		{
			if (!string.IsNullOrEmpty(parameter.Value))
			{
				if (string.IsNullOrEmpty(parameter.TypeConverter))
					return Convert.ChangeType(parameter.Value, info.ParameterType, CultureInfo.InvariantCulture);
				Type typeConverterType = Type.GetType(parameter.TypeConverter);
				TypeConverter typeConverter = (TypeConverter)typeConverterType.Assembly.CreateInstance(typeConverterType.AssemblyQualifiedName);
				return typeConverter.ConvertFrom(parameter.Value);
			}
			if (!string.IsNullOrEmpty(parameter.DependencyType))
			{
				Type dependencyType = Type.GetType(parameter.DependencyType);
				return container.Resolve(dependencyType, parameter.DependencyName);
			}
			return container.Resolve(info.ParameterType, parameter.DependencyName);
		}

	}
}

