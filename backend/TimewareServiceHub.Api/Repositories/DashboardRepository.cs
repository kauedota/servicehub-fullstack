using System.Data;
using Dapper;
using TimewareServiceHub.Api.Data;
using TimewareServiceHub.Api.DTOs;

namespace TimewareServiceHub.Api.Repositories;

// DAPPER chamando uma STORED PROCEDURE que devolve DOIS result sets.
// Relatórios/agregações (COUNT, GROUP BY) são o caso clássico para Dapper ou SQL puro:
// o SQL é mais claro que LINQ e não há entidades para rastrear.
public class DashboardRepository : IDashboardRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DashboardRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DashboardDto> ObterAsync()
    {
        await using var conn = _connectionFactory.Criar();

        // QueryMultiple lê vários SELECTs de uma vez só (uma ida ao banco).
        using var multi = await conn.QueryMultipleAsync(
            "dbo.sp_ObterDashboard",
            commandType: CommandType.StoredProcedure);

        var dashboard = await multi.ReadSingleAsync<DashboardDto>();               // 1º SELECT: totais
        dashboard.ChamadosPorPrioridade = (await multi.ReadAsync<ChamadosPorPrioridadeDto>()).ToList(); // 2º SELECT

        return dashboard;
    }
}
