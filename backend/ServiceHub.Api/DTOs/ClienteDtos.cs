using System.ComponentModel.DataAnnotations;

namespace ServiceHub.Api.DTOs;

// Resposta (saída). "Positional record": o construtor e as propriedades são gerados a partir dos parâmetros.
public record ClienteDto(
    int Id,
    string Nome,
    string Documento,
    string Email,
    string? Telefone,
    string Setor,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

// Requisição (entrada) para criar e atualizar. "init" = só pode ser atribuído na criação do objeto.
public record ClienteRequest
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(150, MinimumLength = 2)]
    public string Nome { get; init; } = string.Empty;

    [Required(ErrorMessage = "Documento (CNPJ/CPF) é obrigatório.")]
    [StringLength(18, MinimumLength = 11)]
    public string Documento { get; init; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    public string Email { get; init; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; init; }

    [Required(ErrorMessage = "Setor é obrigatório.")]
    [StringLength(50)]
    public string Setor { get; init; } = string.Empty;
}
