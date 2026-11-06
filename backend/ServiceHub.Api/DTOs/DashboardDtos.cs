namespace ServiceHub.Api.DTOs;

// Preenchido pelo Dapper a partir da stored procedure dbo.sp_ObterDashboard.
public class DashboardDto
{
    public int TotalClientes { get; set; }
    public int TotalProjetos { get; set; }
    public int ChamadosAbertos { get; set; }
    public int ChamadosEmAndamento { get; set; }
    public int ChamadosConcluidos { get; set; }
    public List<ChamadosPorPrioridadeDto> ChamadosPorPrioridade { get; set; } = new();
}

public class ChamadosPorPrioridadeDto
{
    public string Prioridade { get; set; } = string.Empty;
    public int Total { get; set; }
    public int EmAberto { get; set; }
}
