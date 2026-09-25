using TimewareServiceHub.Api.DTOs;

namespace TimewareServiceHub.Api.Repositories;

public interface IAuditoriaRepository
{
    Task RegistrarAsync(string entidade, int entidadeId, string acao, string? detalhes, int? usuarioId);
    Task<List<AuditoriaDto>> ListarAsync(string entidade, int entidadeId);
}
