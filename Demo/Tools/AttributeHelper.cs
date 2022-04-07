using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public static class AttributeHelper
	{
		public static T GetFirstAttribute<T>(this MemberInfo info)
			where T : Attribute
		{
			Guard.ArgumentNotNull(info, "info");
			object[] attributes = info.GetCustomAttributes(typeof(T), true);
			return (T)attributes.FirstOrDefault();
		}

		public static bool HasAttribute<T>(this MemberInfo info)
			where T : Attribute
		{
			return GetFirstAttribute<T>(info) != null;
		}
	}
}
