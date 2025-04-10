using System.Data;

namespace DapperSamples.Data;

public interface IDatabaseConnectionFactory
{
    IDbConnection CreateConnection();
} 