using TimewareServiceHub.Api.Domain.Enums;

namespace TimewareServiceHub.Api.Domain.Entities;

public class Projeto
{
    public int Id { get; set; }

    // Chave estrangeira + propriedade de navegação (como @ManyToOne + @JoinColumn no JPA).
    // No EF Core é comum ter os DOIS: o Id (ClienteId) e o objeto (Cliente).
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public StatusProjeto Status { get; set; } = StatusProjeto.Planejado;

    public int ResponsavelId { get; set; }
    public Usuario? Responsavel { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
