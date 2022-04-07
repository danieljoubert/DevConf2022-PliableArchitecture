using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using AddParameterAction = System.Action<System.Data.IDbCommand, object>;
using ReadResultAction = System.Action<System.Data.IDataReader, object>;

namespace Demo
{
  public class FastDataUpdater
  {
    public FastDataUpdater(IConnectionFactory connectionFactory)
    {
      _connectionFactory = connectionFactory;
    }

    private static readonly ConcurrentDictionary<Type, string> UpdateQueryCache = new ConcurrentDictionary<Type, string>();
    private static readonly ConcurrentDictionary<Type, string> InsertQueryCache = new ConcurrentDictionary<Type, string>();
    private static readonly ConcurrentDictionary<Type, AddParameterAction> AddParamtersCache =
      new ConcurrentDictionary<Type, AddParameterAction>();
    private static readonly ConcurrentDictionary<Type, ReadResultAction> ReadParamtersCache =
      new ConcurrentDictionary<Type, ReadResultAction>();

    private readonly IConnectionFactory _connectionFactory;

    public int Update(object value)
    {
      return Execute(value, type => UpdateQueryCache.GetOrAdd(type, GetUpdateSql), true);
    }

    public int Insert(object value)
    {
      return Execute(value, type => InsertQueryCache.GetOrAdd(type, GetInsertSql), true);
    }

    public int Delete(object value)
    {
      return Execute(value, type => string.Format(CultureInfo.InvariantCulture, "delete from {0} where Id = @Id", type.Name), false);
    }

    public int Execute(object value, Func<Type, string> getQuery, bool readParameters)
    {
      Guard.ArgumentNotNull(value, "value");
      var type = value.GetType();
      using (IDbConnection con = _connectionFactory.CreateConnection())
      using (IDbCommand cmd = con.CreateCommand())
      {
        con.Open();
        cmd.CommandTimeout = _connectionFactory.CommandTimeout;
        cmd.CommandText = getQuery(type);
        AddParameters(cmd, type, value);
        using (IDataReader dr = cmd.ExecuteReader())
        {
          if (readParameters && dr.Read())
            ReadResult(dr, type, value);
          return dr.RecordsAffected;
        }
      }
    }

    public static string GetUpdateSql(Type type)
    {
      string tableName = GetTableName(type);
      var props = from p in type.GetProperties()
                  let columnAttribute = p.GetFirstAttribute<ColumnAttribute>()
                  where columnAttribute != null && !columnAttribute.IsPrimaryKey && !columnAttribute.IsDbGenerated
                  select string.Format(CultureInfo.InvariantCulture, "[{0}] = @{0}", p.Name);
      return string.Format(CultureInfo.InvariantCulture, "update {0} set {1} where Id = @Id", tableName, string.Join(",", props));
    }

    public static string GetInsertSql(Type type)
    {
      string tableName = GetTableName(type);
      var paramProps = (from p in type.GetProperties()
                        let columnAttribute = p.GetFirstAttribute<ColumnAttribute>()
                        where columnAttribute != null && !columnAttribute.IsPrimaryKey && !columnAttribute.IsDbGenerated
                        select p.Name).ToArray();
      var allProps = (from p in type.GetProperties()
                      where ShouldReadColumn(p.GetFirstAttribute<ColumnAttribute>())
                      select p.Name).ToArray();
      return string.Format(CultureInfo.InvariantCulture, "insert into {0} ({1}) output {2} values ({3})",
        tableName, string.Join(",", paramProps.Select(n => "[" + n + "]")), string.Join(",", allProps.Select(n => "inserted.[" + n + "]")),
        string.Join(",", paramProps.Select(n => "@" + n)));
    }

    private static string GetTableName(Type type)
    {
      string tableName = type.Name;
      TableAttribute table = type.GetFirstAttribute<TableAttribute>();
      if (!string.IsNullOrEmpty(table?.Name))
        tableName = table.Name;
      return tableName;
    }

    public static void AddParameters(IDbCommand cmd, Type type, object value)
    {
      var action = AddParamtersCache.GetOrAdd(type, BuildAddParameterAction);
      action(cmd, value);
    }

    private static AddParameterAction BuildAddParameterAction(Type type)
    {
      DynamicMethod dm = new DynamicMethod("CreateParameters", typeof(void),
        new[] { typeof(IDbCommand), typeof(object) }, true)
      { InitLocals = true };
      ILGenerator il = dm.GetILGenerator();
      AddParameters(type, il);
      il.Emit(OpCodes.Ret);
      return (AddParameterAction)dm.CreateDelegate(typeof(AddParameterAction));
    }

    public static void AddParameters(Type type, ILGenerator il)
    {
      var properties = (from p in type.GetProperties()
                        let columnAttribute = p.GetFirstAttribute<ColumnAttribute>()
                        where columnAttribute != null && (columnAttribute.IsPrimaryKey || !columnAttribute.IsDbGenerated)
                        select p).ToArray();
      foreach (var property in properties)
        AddParameter(type, il, property);
    }

    private static void AddParameter(Type type, ILGenerator il, PropertyInfo property)
    {
      Action<IDbCommand, string, object> addParameter = AddParameter;
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldstr, property.Name);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Isinst, type);
      il.Emit(OpCodes.Callvirt, property.GetGetMethod());
      MethodInfo readPropertyMethod = FastDataUpdaterHelper.GetPropertyValueMethod(property);
      il.Emit(OpCodes.Call, readPropertyMethod);
      il.Emit(OpCodes.Call, addParameter.Method);
    }

    private static void AddParameter(IDbCommand command, string propertyName, object propertyValue)
    {
      var parameter = command.CreateParameter();
      parameter.ParameterName = propertyName;
      parameter.Value = propertyValue ?? DBNull.Value;
      command.Parameters.Add(parameter);
    }

    private void ReadResult(IDataReader reader, Type type, object value)
    {
      var action = ReadParamtersCache.GetOrAdd(type, BuildReadResultAction);
      action(reader, value);
    }

    private ReadResultAction BuildReadResultAction(Type type)
    {
      DynamicMethod dm = new DynamicMethod("ReadValues", typeof(void), new[] { typeof(IDataReader), typeof(object) },
        true)
      { InitLocals = true };
      ILGenerator il = dm.GetILGenerator();
      il.DeclareLocal(type);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Isinst, type);
      il.Emit(OpCodes.Stloc_0);

      foreach (var property in type.GetProperties().Where(p => ShouldReadColumn(p.GetFirstAttribute<ColumnAttribute>())))
        ReadField(il, property);

      il.Emit(OpCodes.Ret);
      return (ReadResultAction)dm.CreateDelegate(typeof(ReadResultAction));
    }

    private static bool ShouldReadColumn(ColumnAttribute column)
    {
      return column != null && (!column.IsDbGenerated || column.IsPrimaryKey);
    }

    private static void ReadField(ILGenerator il, PropertyInfo property)
    {
      MethodInfo method = FastDataReaderHelper.GetReadfieldMethod(property);
      il.Emit(OpCodes.Ldloc_0); //obj
      il.Emit(OpCodes.Ldarg_0); //reader
      il.Emit(OpCodes.Ldstr, property.Name); //property name
      il.Emit(OpCodes.Call, method); //ReadField<long>(cmd, "column")
      il.Emit(OpCodes.Callvirt, property.GetSetMethod()); //obj.Property = ReadParameter
    }

  }
}
