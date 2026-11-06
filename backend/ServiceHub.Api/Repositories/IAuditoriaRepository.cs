using ServiceHub.Api.DTOs;

namespace ServiceHub.Api.Repositories;

public interface IAuditoriaRepository
{
    Task RegistrarAsync(string entidade, int entidadeId, string acao, string? detalhes, int? usuarioId);
    Task<List<AuditoriaDto>> ListarAsync(string entidade, int entidadeId);
}
