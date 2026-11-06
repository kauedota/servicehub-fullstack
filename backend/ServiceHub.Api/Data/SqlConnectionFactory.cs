using Microsoft.Data.SqlClient;

namespace ServiceHub.Api.Data;

// Fábrica de conexões usada por Dapper e ADO.NET.
// Equivalente a injetar um DataSource no Spring e chamar dataSource.getConnection().
// Criar SqlConnection é barato: o driver mantém um POOL de conexões por trás (como o HikariCP).
public interface ISqlConnectionFactory
{
    SqlConnection Criar();
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection Criar() => new SqlConnection(_connectionString);
}
