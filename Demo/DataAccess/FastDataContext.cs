using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Demo
{
  public sealed class FastDataContext : IDataContext
  {
    public FastDataContext(string connectionStringName)
    {
      _connectionFactory = new ConnectionFactory(connectionStringName);
      _dataReader = new FastDataReader(_connectionFactory);
      _dataUpdater = new FastDataUpdater(_connectionFactory);
      CommandTimeout = 60;
    }

    private readonly IConnectionFactory _connectionFactory;
    private readonly FastDataReader _dataReader;
    private readonly FastDataUpdater _dataUpdater;

    public int CommandTimeout
    {
      get { return _connectionFactory.CommandTimeout; }
      set { _connectionFactory.CommandTimeout = value; }
    }

    public int ExecuteCommand(string command, params object[] parameters)
    {
      using (IDbConnection con = _connectionFactory.CreateConnection())
      using (IDbCommand cmd = con.CreateCommand())
      {
        con.Open();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = string.Format(CultureInfo.InvariantCulture, command, parameters);
        return cmd.ExecuteNonQuery();
      }
    }

    public IEnumerable<TResult> ExecuteQuery<TResult>(string query, params object[] parameters)
    {
      string sqlQuery = string.Format(CultureInfo.InvariantCulture, query, parameters);
      return _dataReader.ReadAll(sqlQuery, typeof(TResult)).Cast<TResult>().ToArray();
    }

    public void ExecuteReader(DataTable dataTable, string query, params object[] parameters)
    {
      using (IDbConnection con = _connectionFactory.CreateConnection())
      using (IDbCommand cmd = con.CreateCommand())
      {
        con.Open();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = string.Format(CultureInfo.InvariantCulture, query, parameters);
        using (IDataReader dr = cmd.ExecuteReader())
          dataTable.Load(dr);
      }
    }

    public T ExecuteScalar<T>(string command, params object[] parameters)
    {
      using (IDbConnection con = _connectionFactory.CreateConnection())
      using (IDbCommand cmd = con.CreateCommand())
      {
        con.Open();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = string.Format(CultureInfo.InvariantCulture, command, parameters);
        object scalar = cmd.ExecuteScalar();
        return FastDataReaderHelper.ConvertValue<T>(scalar);
      }
    }

    public IDataStorable Update(IDataStorable value)
    {
      if (value.IsNew)
        _dataUpdater.Insert(value);
      else
        _dataUpdater.Update(value);
      return value;
    }

    public ICollection<object> RetrieveQuery(string query, Type type)
    {
      return _dataReader.ReadAll(query, type);
    }

    public void Delete(object itemToDelete)
    {
      Guard.ArgumentNotNull(itemToDelete, "itemToDelete");
      _dataUpdater.Delete(itemToDelete);
    }
  }
}
