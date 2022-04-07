using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.PolicyInjection.Pipeline;

namespace Demo
{
	public class TraceBehavior : IInterceptionBehavior
	{
		public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
		{
			Guard.ArgumentNotNull(input, nameof(input));
			Guard.ArgumentNotNull(getNext, nameof(getNext));

			Stopwatch sw = new Stopwatch();
			sw.Start();
			IMethodReturn result = getNext()(input, getNext);
			sw.Stop();

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(CultureInfo.InvariantCulture, "Date & Time: {0:yyyy-MM-dd HH:mm:ss}\r\n", DateTime.Now);
			sb.AppendFormat(CultureInfo.InvariantCulture, "Method: {0}\r\n\r\n", input.MethodBase.Name);
			ParameterInfo[] parameterInfos = input.MethodBase.GetParameters();
			for (int i = 0; i < input.Arguments.Count; i++)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, "Parameter: {0} ({1})\r\n", parameterInfos[i].Name, parameterInfos[i].ParameterType);
				if (input.Arguments[i] == null || input.Arguments[i] != null && !(input.Arguments[i] is Delegate))
					sb.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n\r\n", ObjectToText(input.Arguments[i]));
			}
			if (result.Outputs.Count > 0)
				sb.AppendFormat(CultureInfo.InvariantCulture, "Result: \r\n{0}\r\n\r\n", ObjectToText(result.ReturnValue));
			if (result.Exception != null)
				sb.AppendFormat(CultureInfo.InvariantCulture, "Exception: \r\n{0}\r\n\r\n", result.Exception);
			sb.AppendFormat(CultureInfo.InvariantCulture, "Duration: {0}", sw.Elapsed);
			sb = sb.Replace("{", "{{").Replace("}", "}}");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(sb.ToString());

			return result;
		}

		public IEnumerable<Type> GetRequiredInterfaces()
		{
			return new Type[0];
		}

		public bool WillExecute { get { return true; } }

		public string ObjectToText(object item)
		{
			return ObjectToText(item, string.Empty);
		}

		public string ObjectToText(object item, string indent)
		{
			if (indent.Length > 3)
				return string.Empty;
			if (item == null)
				return indent + "<null>";
			Type type = item.GetType();
			if (type.IsValueType || type == typeof(string) || type == typeof(Type) || type == typeof(object))
				return string.Format(CultureInfo.InvariantCulture, "{0}{1} ({2})", indent, item, type);
			StringBuilder msg = new StringBuilder();
			if (type.GetInterface(typeof(IEnumerable).Name) != null)
			{
				int index = 0;
				foreach (var i in (IEnumerable)item)
				{
					msg.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}[{2}]\r\n", indent, i, index++);
					msg.AppendLine(ObjectToText(i, indent + "\t"));
				}
			}
			else
			{
				foreach (PropertyInfo info in item.GetType().GetProperties(BindingFlags.Public))
				{
					object value = info.GetValue(item, null);
					if (type.IsValueType || type == typeof(string))
						msg.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: {2} ({3})\r\n", indent, info.Name, value, info.PropertyType);
					else
						msg.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: {2}\r\n", indent, info.Name, ObjectToText(value, indent + "\t"));
				}
			}
			return msg.ToString();
		}
	}
}
