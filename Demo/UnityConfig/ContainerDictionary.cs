using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Unity;
using Unity.Injection;

namespace Demo.UnityConfig
{
	public class ContainerDictionaryExtension : SectionExtension
	{
		public override void AddExtensions(SectionExtensionContext context)
		{
			context.AddElement<ContainerDictionaryElement>("containerDictionary");
		}
	}

	public class ContainerDictionaryElement : ValueElement
	{
		public override ParameterValue GetInjectionParameterValue(IUnityContainer container, Type parameterType)
		{
			Type valueType = parameterType.GenericTypeArguments[1];
			Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
			IDictionary dictionary = (IDictionary)dictionaryType.Assembly.CreateInstance(dictionaryType.FullName);
			foreach (var reg in container.Registrations.Where(r => r.RegisteredType == valueType))
			{
				string name = reg.Name ?? string.Empty;
				name = ConvertKeyToUppercase ? name.ToUpperInvariant() : name;
				dictionary.Add(name, container.Resolve(reg.RegisteredType, reg.Name));
			}
			return new InjectionParameter(parameterType, dictionary);
		}

		private const string ConvertKeyToUppercasePropertyName = "convertKeyToUpperCase";
		[ConfigurationProperty(ConvertKeyToUppercasePropertyName, IsRequired = false)]
		public bool ConvertKeyToUppercase
		{
			get { return (bool)base[ConvertKeyToUppercasePropertyName]; }
			set { base[ConvertKeyToUppercasePropertyName] = value; }
		}

	}
}
