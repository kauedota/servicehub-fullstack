using Dapper;
using Microsoft.EntityFrameworkCore;
using TimewareServiceHub.Api.Data;
using TimewareServiceHub.Api.Domain.Entities;
using TimewareServiceHub.Api.DTOs;

namespace TimewareServiceHub.Api.Repositories;

public class ChamadoRepository : IChamadoRepository
{
    private readonly AppDbContext _db;
    private readonly ISqlConnectionFactory _connectionFactory;

    public ChamadoRepository(AppDbContext db, ISqlConnectionFactory connectionFactory)
    {
        _db = db;
        _connectionFactory = connectionFactory;
    }

    // SELECT base com INNER JOIN (todo chamado TEM cliente) e LEFT JOIN (projeto/responsável são opcionais).
    // Os aliases (AS ClienteNome) precisam bater com as propriedades do ChamadoDto.
    private const string SelectBase = """
        SELECT  ch.Id, ch.Titulo, ch.Descricao, ch.Prioridade, ch.Status,
                ch.ClienteId,     cl.Nome AS ClienteNome,
                ch.ProjetoId,     pr.Nome AS ProjetoNome,
                ch.ResponsavelId, us.Nome AS ResponsavelNome,
                ch.CriadoEm, ch.AtualizadoEm, ch.ConcluidoEm
        FROM dbo.Chamados ch
        INNER JOIN dbo.Clientes cl ON cl.Id = ch.ClienteId
        LEFT  JOIN dbo.Projetos pr ON pr.Id = ch.ProjetoId
        LEFT  JOIN dbo.Usuarios us ON us.Id = ch.ResponsavelId
        """;

    // ---------------- LEITURA COM DAPPER ----------------
    public async Task<List<ChamadoDto>> ListarAsync(ChamadoFiltro filtro)
    {
        // Filtro opcional: "(@Status IS NULL OR ch.Status = @Status)" => se o parâmetro vier nulo, ignora o filtro.
        var sql = SelectBase + """

            WHERE (@Status IS NULL OR ch.Status = @Status)
              AND (@Prioridade IS NULL OR ch.Prioridade = @Prioridade)
              AND (@ClienteId IS NULL OR ch.ClienteId = @ClienteId)
              AND (@ResponsavelId IS NULL OR ch.ResponsavelId = @ResponsavelId)
              AND (@Busca IS NULL OR ch.Titulo LIKE '%' + @Busca + '%')
            ORDER BY
                CASE ch.Prioridade WHEN 'Critica' THEN 1 WHEN 'Alta' THEN 2 WHEN 'Media' THEN 3 ELSE 4 END,
                ch.CriadoEm DESC;
            """;

        // Parâmetros via objeto anônimo. O Dapper cria @Status, @Prioridade... => protege contra SQL Injection.
        // ATENÇÃO: enum vai como NÚMERO por padrão no Dapper; como no banco é texto, convertemos com ToString().
        var parametros = new
        {
            Status = filtro.Status?.ToString(),
            Prioridade = filtro.Prioridade?.ToString(),
            filtro.ClienteId,
            filtro.ResponsavelId,
            Busca = string.IsNullOrWhiteSpace(filtro.Busca) ? null : filtro.Busca.Trim()
        };

        // "await using" = try-with-resources do Java: fecha a conexão (devolve ao pool) no final.
        await using var conn = _connectionFactory.Criar();
        var resultado = await conn.QueryAsync<ChamadoDto>(sql, parametros);
        return resultado.ToList();
    }

    public async Task<ChamadoDto?> ObterDtoPorIdAsync(int id)
    {
        var sql = SelectBase + "\nWHERE ch.Id = @Id;";
        await using var conn = _connectionFactory.Criar();
        return await conn.QuerySingleOrDefaultAsync<ChamadoDto>(sql, new { Id = id });
    }

    // ---------------- ESCRITA COM EF CORE ----------------
    public Task<Chamado?> ObterEntidadeAsync(int id) =>
        _db.Chamados.FirstOrDefaultAsync(c => c.Id == id);

    public async Task AdicionarAsync(Chamado chamado)
    {
        _db.Chamados.Add(chamado);
        await _db.SaveChangesAsync();
    }

    public Task SalvarAlteracoesAsync() => _db.SaveChangesAsync();
}
