namespace ServiceHub.Api.DTOs;

// Preenchido "na mão" com ADO.NET (SqlDataReader) no AuditoriaRepository.
public record AuditoriaDto(
    long Id,
    string Entidade,
    int EntidadeId,
    string Acao,
    string? Detalhes,
    string? UsuarioNome,
    DateTime DataHora);

// "static class" com constantes: evita strings mágicas espalhadas pelo código.
public static class AcoesAuditoria
{
    public const string Criacao = "Criacao";
    public const string Atualizacao = "Atualizacao";
    public const string AlteracaoStatus = "AlteracaoStatus";
    public const string Exclusao = "Exclusao";
}
