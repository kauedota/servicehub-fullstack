using System.ComponentModel.DataAnnotations;
using TimewareServiceHub.Api.Domain.Enums;

namespace TimewareServiceHub.Api.DTOs;

public record ProjetoDto(
    int Id,
    int ClienteId,
    string ClienteNome,
    string Nome,
    string? Descricao,
    StatusProjeto Status,
    int ResponsavelId,
    string ResponsavelNome,
    DateTime CriadoEm);

public record ProjetoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Cliente é obrigatório.")]
    public int ClienteId { get; init; }

    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(150, MinimumLength = 2)]
    public string Nome { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Descricao { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Responsável é obrigatório.")]
    public int ResponsavelId { get; init; }
}

public record AlterarStatusProjetoRequest
{
    // Nullable + [Required] garante que o campo foi enviado (senão o enum viria com o valor padrão 0).
    [Required(ErrorMessage = "Status é obrigatório.")]
    public StatusProjeto? Status { get; init; }
}
