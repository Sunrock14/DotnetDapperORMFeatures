using System.Data;
using Microsoft.Data.SqlClient;

namespace DapperSamples.Data;

public class SqlConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
} 