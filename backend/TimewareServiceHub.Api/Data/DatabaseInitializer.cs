using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TimewareServiceHub.Api.Domain.Entities;
using TimewareServiceHub.Api.Domain.Enums;

namespace TimewareServiceHub.Api.Data;

// Prepara o banco quando a API sobe:
//   1. Espera o SQL Server ficar disponível (útil no Docker)
//   2. Cria o database se não existir                (ADO.NET)
//   3. Executa database/01-ddl.sql (tabelas, índices, procedure) (ADO.NET)
//   4. Cria os usuários de demonstração com senha em BCrypt       (EF Core)
//   5. Executa database/02-seed.sql se não houver clientes        (ADO.NET)
//
// Em um projeto real, os passos 2 e 3 normalmente seriam feitos com EF Core Migrations
// (equivalente ao Flyway/Liquibase) ou por um pipeline que roda os scripts SQL.
public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, AppDbContext db, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _db = db;
        _logger = logger;
    }

    public async Task InicializarAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        var builder = new SqlConnectionStringBuilder(connectionString);
        var nomeBanco = builder.InitialCatalog;

        // Conexão com o banco "master" para poder criar o nosso database
        builder.InitialCatalog = "master";
        var masterConnectionString = builder.ConnectionString;

        await AguardarConexaoAsync(masterConnectionString, "SQL Server");
        await CriarBancoSeNaoExisteAsync(masterConnectionString, nomeBanco);

        // Após um restart, o SQL Server aceita conexões antes de terminar de "recuperar" cada banco
        // (erro 4060: Cannot open database). Por isso esperamos também o banco da aplicação ficar online.
        await AguardarConexaoAsync(connectionString, $"banco {nomeBanco}");

        await ExecutarScriptAsync(connectionString, "01-ddl.sql");
        await CriarUsuariosDemoAsync();

        if (await TabelaVaziaAsync(connectionString, "Clientes"))
            await ExecutarScriptAsync(connectionString, "02-seed.sql");

        _logger.LogInformation("Banco {Banco} pronto.", nomeBanco);
    }

    private async Task AguardarConexaoAsync(string connectionString, string alvo)
    {
        for (var tentativa = 1; tentativa <= 30; tentativa++)
        {
            try
            {
                await using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                return;
            }
            catch (SqlException ex) when (tentativa < 30)
            {
                _logger.LogWarning("{Alvo} indisponível (tentativa {Tentativa}/30, erro {Numero}). Aguardando...",
                    alvo, tentativa, ex.Number);
                // Conexões que falharam ficam "envenenadas" no pool; limpar força uma conexão nova na próxima tentativa.
                SqlConnection.ClearAllPools();
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    // ADO.NET puro: SqlConnection + SqlCommand + parâmetro.
    private static async Task CriarBancoSeNaoExisteAsync(string masterConnectionString, string nomeBanco)
    {
        const string sql = """
            IF DB_ID(@Nome) IS NULL
            BEGIN
                DECLARE @comando NVARCHAR(300) = N'CREATE DATABASE ' + QUOTENAME(@Nome);
                EXEC sp_executesql @comando;
            END
            """;

        await using var conn = new SqlConnection(masterConnectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nome", nomeBanco);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task ExecutarScriptAsync(string connectionString, string arquivo)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Database", arquivo);
        var script = await File.ReadAllTextAsync(caminho);

        // "GO" não é T-SQL: é um separador de lotes do SSMS/sqlcmd. Por isso dividimos o script aqui.
        var lotes = Regex.Split(script, @"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(l => !string.IsNullOrWhiteSpace(l));

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        foreach (var lote in lotes)
        {
            await using var cmd = new SqlCommand(lote, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        _logger.LogInformation("Script {Arquivo} executado.", arquivo);
    }

    private static async Task<bool> TabelaVaziaAsync(string connectionString, string tabela)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        // Nome de tabela não pode ser parâmetro; aqui ele é uma constante do código, nunca entrada do usuário.
        await using var cmd = new SqlCommand($"SELECT COUNT(1) FROM dbo.{tabela}", conn);
        var total = (int)(await cmd.ExecuteScalarAsync())!;
        return total == 0;
    }

    // EF Core: AnyAsync + AddRange + SaveChangesAsync.
    private async Task CriarUsuariosDemoAsync()
    {
        if (await _db.Usuarios.AnyAsync())
            return;

        _db.Usuarios.AddRange(
            NovoUsuario("Administrador", "admin@servicehub.local", "Admin@123", PerfilUsuario.Admin),
            NovoUsuario("Ana Souza", "ana@servicehub.local", "Analista@123", PerfilUsuario.Analista),
            NovoUsuario("Bruno Lima", "bruno@servicehub.local", "Analista@123", PerfilUsuario.Analista));

        await _db.SaveChangesAsync();
        _logger.LogInformation("Usuários de demonstração criados.");
    }

    private static Usuario NovoUsuario(string nome, string email, string senha, PerfilUsuario perfil) => new()
    {
        Nome = nome,
        Email = email,
        SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
        Perfil = perfil,
        Ativo = true,
        CriadoEm = DateTime.UtcNow
    };
}
