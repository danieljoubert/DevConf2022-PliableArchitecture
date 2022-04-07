using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace Demo
{
  public class FastDataReader
  {
    public FastDataReader(IConnectionFactory connectionFactory)
    {
      _connectionFactory = connectionFactory;
    }

    private readonly static ConcurrentDictionary<Type, object> Cache = new ConcurrentDictionary<Type, object>();
    private readonly IConnectionFactory _connectionFactory;

    public object[] ReadAll(string query, Type type)
    {
      using (IDbConnection con = _connectionFactory.CreateConnection())
      using (IDbCommand cmd = con.CreateCommand())
      {
        con.Open();
        cmd.CommandTimeout = _connectionFactory.CommandTimeout;
        cmd.CommandText = query;
        using (var dr = cmd.ExecuteReader())
          return ReadResults(dr, type);
      }
    }

    public TValue Read<TValue, TId>(TId id)
    {
      if (Equals(default(TId), id))
        return default(TValue);
      var query = string.Format(CultureInfo.InvariantCulture, "select top 1 * from {0} with(nolock) where Id = {1}", typeof(TValue).Name, id);
      return ReadAll(query, typeof(TValue)).Cast<TValue>().FirstOrDefault();
    }

    private TValue[] ReadAll<TValue, TId>(string foreignKeyColumn, TId foreignKeyId)
    {
      if (Equals(default(TId), foreignKeyId))
        return new TValue[0];
      var query = string.Format(CultureInfo.InvariantCulture, "select * from {0} with(nolock) where {1} = {2}", typeof(TValue).Name, foreignKeyColumn, foreignKeyId);
      return ReadAll(query, typeof(TValue)).Cast<TValue>().ToArray();
    }

    private object[] ReadResults(IDataReader dr, Type type)
    {
      try
      {
        List<object> list = new List<object>();
        while (dr.Read())
        {
          object value = FormatterServices.GetUninitializedObject(type);
          GetProperties(value, type, dr);
          list.Add(value);
        }
        return list.ToArray();
      }
      catch (Exception ex)
      {
        throw new DataAccessException(ex, "Error while reading columns for type '{0}'", type.Name);
      }
    }

    private void GetProperties(object value, Type type, IDataReader dr)
    {
      Action<object, IDataReader, FastDataReader> funct = (Action<object, IDataReader, FastDataReader>)Cache.GetOrAdd(type, BuildPropertyReader);
      funct(value, dr, this);
    }

    private Action<object, IDataReader, FastDataReader> BuildPropertyReader(Type type)
    {
      DynamicMethod dm = new DynamicMethod("Read", typeof(void), new[] { typeof(object), typeof(IDataReader), typeof(FastDataReader) }, true) { InitLocals = true };
      ILGenerator il = dm.GetILGenerator();
      il.DeclareLocal(type);
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Isinst, type);
      il.Emit(OpCodes.Stloc_0);

      var infos = type.GetProperties()
        .Where(p => p.HasAttribute<AssociationAttribute>() || p.HasAttribute<ColumnAttribute>())
        .ToArray();
      foreach (PropertyInfo info in infos)
      {
        MethodInfo readFieldMethod = FastDataReaderHelper.GetReadfieldMethod(info);
        if (readFieldMethod != null)
          ReadValue(il, info, readFieldMethod);
        else if (info.PropertyType.IsArray)
          ReadArray(il, info);
        else
          ReadRef(il, info);
      }

      il.Emit(OpCodes.Ret);
      return (Action<object, IDataReader, FastDataReader>)dm.CreateDelegate(typeof(Action<object, IDataReader, FastDataReader>));
    }

    private void ReadArray(ILGenerator il, PropertyInfo info)
    {
      Type type = info.PropertyType.GetElementType();
      AssociationAttribute association = info.GetCustomAttribute<AssociationAttribute>();
      Guard.Validate(association != null, "An 'Association' attribute must be present on array properties on data objects");
      string thisKeyPropertyName = association.ThisKey;
      string foreignKeyPropertyName = association.OtherKey;
      MethodInfo getThisKeyProperty = info.DeclaringType.GetProperty(thisKeyPropertyName).GetGetMethod();
      Func<string, int, int[]> del = ReadAll<int, int>;
      MethodInfo readall = del.Method.GetGenericMethodDefinition().MakeGenericMethod(type, getThisKeyProperty.ReturnType);

      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Ldstr, foreignKeyPropertyName);
      il.Emit(OpCodes.Ldarg_2);
      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Callvirt, getThisKeyProperty);
      il.Emit(OpCodes.Callvirt, readall);
      il.Emit(OpCodes.Callvirt, info.GetSetMethod());
    }

    private void ReadRef(ILGenerator il, PropertyInfo info)
    {
      Func<int, int> readFunc = Read<int, int>;
      AssociationAttribute association = info.GetCustomAttribute<AssociationAttribute>();
      Guard.Validate(association != null, "An 'Association' attribute must be present on reference properties on data objects");
      string foreignKeyPropertyName = association.ThisKey;
      PropertyInfo foreignKeyPropertyInfo = info.DeclaringType.GetProperty(foreignKeyPropertyName);
      MethodInfo read = readFunc.Method.GetGenericMethodDefinition().MakeGenericMethod(new[] { info.PropertyType, foreignKeyPropertyInfo.PropertyType });

      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Ldarg_2);
      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Callvirt, foreignKeyPropertyInfo.GetGetMethod());
      il.Emit(OpCodes.Callvirt, read);
      il.Emit(OpCodes.Callvirt, info.GetSetMethod());
    }

    private static void ReadValue(ILGenerator il, PropertyInfo info, MethodInfo readFieldMethod)
    {
      il.Emit(OpCodes.Ldloc_0);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Ldstr, info.Name);
      il.Emit(OpCodes.Call, readFieldMethod);
      il.Emit(OpCodes.Callvirt, info.GetSetMethod());
    }

  }
}
