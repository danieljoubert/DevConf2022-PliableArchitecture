using System;
using System.Data.Linq.Mapping;
using System.Reflection;

namespace Demo
{
  public static class FastDataUpdaterHelper
  {
    public static MethodInfo GetPropertyValueMethod(PropertyInfo info)
    {
      ColumnAttribute column = info.GetFirstAttribute<ColumnAttribute>();
      return GetMethodInfo<int>(ReadPropertyValue, info.PropertyType);
    }

    private static MethodInfo GetMethodInfo<T>(Func<T, object> func, Type destType)
    {
      return func.GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(destType);
    }

    private static object ReadPropertyValue<T>(T value)
    {
      return value;
    }
  }
}