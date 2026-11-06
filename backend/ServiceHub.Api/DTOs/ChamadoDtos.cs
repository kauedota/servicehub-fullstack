using System.ComponentModel.DataAnnotations;
using ServiceHub.Api.Domain.Enums;

namespace ServiceHub.Api.DTOs;

// Este DTO é preenchido pelo DAPPER (ChamadoRepository). O Dapper casa o NOME da coluna do SELECT
// com o nome da propriedade. Por isso é uma classe com { get; set; } (mais simples para o Dapper).
// O Dapper também converte o texto 'Aberto' da coluna para o enum StatusChamado.Aberto.
public class ChamadoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public Prioridade Prioridade { get; set; }
    public StatusChamado Status { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int? ProjetoId { get; set; }
    public string? ProjetoNome { get; set; }
    public int? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public DateTime? ConcluidoEm { get; set; }
}

// Filtros da listagem, lidos da query string: GET /api/chamados?status=Aberto&prioridade=Alta
public record ChamadoFiltro
{
    public StatusChamado? Status { get; init; }
    public Prioridade? Prioridade { get; init; }
    public int? ClienteId { get; init; }
    public int? ResponsavelId { get; init; }
    public string? Busca { get; init; }
}

public record CriarChamadoRequest
{
    [Required(ErrorMessage = "Título é obrigatório.")]
    [StringLength(150, MinimumLength = 3)]
    public string Titulo { get; init; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória.")]
    [StringLength(2000)]
    public string Descricao { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Cliente é obrigatório.")]
    public int ClienteId { get; init; }

    public int? ProjetoId { get; init; }
    public int? ResponsavelId { get; init; }

    [Required(ErrorMessage = "Prioridade é obrigatória.")]
    public Prioridade? Prioridade { get; init; }
}

// Atualização: define prioridade, atribui responsável, troca projeto, edita texto.
public record AtualizarChamadoRequest
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string Titulo { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string Descricao { get; init; } = string.Empty;

    public int? ProjetoId { get; init; }
    public int? ResponsavelId { get; init; }

    [Required(ErrorMessage = "Prioridade é obrigatória.")]
    public Prioridade? Prioridade { get; init; }
}

public record AlterarStatusChamadoRequest
{
    [Required(ErrorMessage = "Status é obrigatório.")]
    public StatusChamado? Status { get; init; }

    [StringLength(500)]
    public string? Comentario { get; init; }
}
