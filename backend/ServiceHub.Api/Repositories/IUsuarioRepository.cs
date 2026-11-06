using ServiceHub.Api.Domain.Entities;

namespace ServiceHub.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<List<Usuario>> ListarAtivosAsync();
}
