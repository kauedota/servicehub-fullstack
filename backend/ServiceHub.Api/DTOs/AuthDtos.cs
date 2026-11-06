using System.ComponentModel.DataAnnotations;
using ServiceHub.Api.Domain.Enums;

namespace ServiceHub.Api.DTOs;

// DTO = objeto que trafega na API. Nunca expomos a entidade diretamente (ex.: SenhaHash não pode vazar).
// "record" = classe imutável focada em dados, com equals/hashCode/toString prontos.
// Equivalente ao "record" do Java 16+ (ou a uma classe com @Value do Lombok).
// [Required], [EmailAddress]... = Bean Validation (@NotBlank, @Email). O [ApiController] valida sozinho
// e devolve 400 automaticamente, sem precisar de @Valid.

public record LoginRequest
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    public string Senha { get; init; } = string.Empty;
}

public record UsuarioDto(int Id, string Nome, string Email, PerfilUsuario Perfil);

public record LoginResponse(string Token, DateTime ExpiraEm, UsuarioDto Usuario);
