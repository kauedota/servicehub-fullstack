using ServiceHub.Api.Domain.Enums;

namespace ServiceHub.Api.Domain.Entities;

public class Chamado
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    // Opcional: um chamado pode não estar ligado a um projeto (int? = Integer nullable no Java)
    public int? ProjetoId { get; set; }
    public Projeto? Projeto { get; set; }

    public int? ResponsavelId { get; set; }
    public Usuario? Responsavel { get; set; }

    public Prioridade Prioridade { get; set; } = Prioridade.Media;
    public StatusChamado Status { get; set; } = StatusChamado.Aberto;

    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public DateTime? ConcluidoEm { get; set; }
}
