using System.Data;
using Microsoft.Data.SqlClient;
using ServiceHub.Api.Data;
using ServiceHub.Api.DTOs;

namespace ServiceHub.Api.Repositories;

// ADO.NET "puro" = equivalente ao JDBC puro do Java.
//   SqlConnection   ~ java.sql.Connection
//   SqlCommand      ~ PreparedStatement
//   SqlParameter    ~ ps.setString(1, ...)
//   SqlDataReader   ~ ResultSet
// É a camada mais baixa: EF Core e Dapper usam ADO.NET por baixo.
public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuditoriaRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task RegistrarAsync(string entidade, int entidadeId, string acao, string? detalhes, int? usuarioId)
    {
        const string sql = """
            INSERT INTO dbo.Auditoria (Entidade, EntidadeId, Acao, Detalhes, UsuarioId)
            VALUES (@Entidade, @EntidadeId, @Acao, @Detalhes, @UsuarioId);
            """;

        await using var conn = _connectionFactory.Criar();
        await conn.OpenAsync();                                   // Dapper abre sozinho; no ADO.NET é manual

        await using var cmd = new SqlCommand(sql, conn);
        // Parâmetros tipados: evitam SQL Injection e ajudam o SQL Server a reaproveitar o plano de execução.
        cmd.Parameters.Add("@Entidade", SqlDbType.NVarChar, 50).Value = entidade;
        cmd.Parameters.Add("@EntidadeId", SqlDbType.Int).Value = entidadeId;
        cmd.Parameters.Add("@Acao", SqlDbType.NVarChar, 30).Value = acao;
        // null do C# NÃO é NULL do banco no ADO.NET: é preciso usar DBNull.Value.
        cmd.Parameters.Add("@Detalhes", SqlDbType.NVarChar, -1).Value = (object?)detalhes ?? DBNull.Value;
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = (object?)usuarioId ?? DBNull.Value;

        await cmd.ExecuteNonQueryAsync();                         // INSERT/UPDATE/DELETE => ExecuteNonQuery
    }

    public async Task<List<AuditoriaDto>> ListarAsync(string entidade, int entidadeId)
    {
        const string sql = """
            SELECT a.Id, a.Entidade, a.EntidadeId, a.Acao, a.Detalhes, u.Nome AS UsuarioNome, a.DataHora
            FROM dbo.Auditoria a
            LEFT JOIN dbo.Usuarios u ON u.Id = a.UsuarioId
            WHERE a.Entidade = @Entidade AND a.EntidadeId = @EntidadeId
            ORDER BY a.DataHora DESC, a.Id DESC;
            """;

        await using var conn = _connectionFactory.Criar();
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Entidade", SqlDbType.NVarChar, 50).Value = entidade;
        cmd.Parameters.Add("@EntidadeId", SqlDbType.Int).Value = entidadeId;

        var lista = new List<AuditoriaDto>();

        // SELECT => ExecuteReader. Lemos linha a linha e montamos o objeto NA MÃO (é isso que o Dapper automatiza).
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new AuditoriaDto(
                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                Entidade: reader.GetString(reader.GetOrdinal("Entidade")),
                EntidadeId: reader.GetInt32(reader.GetOrdinal("EntidadeId")),
                Acao: reader.GetString(reader.GetOrdinal("Acao")),
                Detalhes: reader.IsDBNull(reader.GetOrdinal("Detalhes")) ? null : reader.GetString(reader.GetOrdinal("Detalhes")),
                UsuarioNome: reader.IsDBNull(reader.GetOrdinal("UsuarioNome")) ? null : reader.GetString(reader.GetOrdinal("UsuarioNome")),
                DataHora: reader.GetDateTime(reader.GetOrdinal("DataHora"))));
        }

        return lista;
    }
}
