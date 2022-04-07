using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Demo
{
  public interface IConnectionFactory
  {
    IDbConnection CreateConnection();
    int CommandTimeout { get; set; }
  }

  public class ConnectionFactory : IConnectionFactory
  {
    public ConnectionFactory(string connectionStringName)
    {
      var settings = ConfigurationManager.ConnectionStrings[connectionStringName];
      _connectionString = settings.ConnectionString;
      _factory = DbProviderFactories.GetFactory(settings.ProviderName);
    }

    private readonly string _connectionString;
    private readonly DbProviderFactory _factory;

    public int CommandTimeout { get; set; }

    public IDbConnection CreateConnection()
    {
      IDbConnection connection = _factory.CreateConnection();
      connection.ConnectionString = _connectionString;
      return connection;
    }
  }
}