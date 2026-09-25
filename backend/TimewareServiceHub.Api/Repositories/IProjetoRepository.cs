using TimewareServiceHub.Api.Domain.Entities;

namespace TimewareServiceHub.Api.Repositories;

public interface IProjetoRepository
{
    Task<List<Projeto>> ListarAsync(int? clienteId);
    Task<Projeto?> ObterPorIdAsync(int id);
    Task<Projeto?> ObterComDetalhesAsync(int id);
    Task AdicionarAsync(Projeto projeto);
    Task SalvarAlteracoesAsync();
}
