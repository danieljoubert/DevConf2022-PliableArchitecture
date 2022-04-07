using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace Demo
{
  public static class FastDataReaderHelper
  {
    public static MethodInfo GetReadfieldMethod(PropertyInfo info)
    {
      ColumnAttribute column = info.GetFirstAttribute<ColumnAttribute>();

      Type type = info.PropertyType;
      bool isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
      if (isNullable)
      {
        var nonNullableType = type.GetGenericArguments()[0];
        return GetMethodInfo(ReadNullableField<long>, nonNullableType);
      }
      if (info.PropertyType.IsValueType || info.PropertyType == typeof(string) || info.PropertyType == typeof(byte[]))
        return GetMethodInfo(ReadField<int?>, info.PropertyType);
      return null;
    }

    private static MethodInfo GetMethodInfo<T>(Func<IDataReader, string, T> func)
    {
      return func.GetMethodInfo();
    }

    private static MethodInfo GetMethodInfo<T>(Func<IDataReader, string, T> func, Type destType)
    {
      return func.GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(destType);
    }

    private static T ReadField<T>(IDataReader reader, string propertyName)
    {
      try
      {
        return ConvertValue<T>(reader[propertyName]);
      }
      catch (Exception ex)
      {
        throw new DataAccessException(ex, "Error reading field: [{0}]", propertyName);
      }
    }

    public static T ConvertValue<T>(object value)
    {
      if (value == DBNull.Value)
        return default(T);
      if (value is T || typeof(T).IsEnum)
        return (T)value;
      return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }

    private static T? ReadNullableField<T>(IDataReader reader, string propertyName)
      where T : struct
    {
      try
      {
        object value = reader[propertyName];
        if (value == DBNull.Value)
          return null;
        return (T)value;
      }
      catch (Exception ex)
      {
        throw new DataAccessException(ex, "Error reading field: [{0}]", propertyName);
      }
    }
  }
}