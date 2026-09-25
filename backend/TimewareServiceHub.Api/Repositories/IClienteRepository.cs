using TimewareServiceHub.Api.Domain.Entities;

namespace TimewareServiceHub.Api.Repositories;

// Interface do repositório. Em C#, por convenção, interfaces começam com "I".
// Diferente do Spring Data JPA, o EF Core NÃO gera a implementação a partir da interface:
// nós escrevemos a classe ClienteRepository (ou usamos o DbContext direto).
public interface IClienteRepository
{
    // Task<T> = CompletableFuture<T> do Java. Todo acesso a banco é assíncrono (async/await).
    Task<List<Cliente>> ListarAsync(string? busca);
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<bool> DocumentoExisteAsync(string documento, int? ignorarId = null);
    Task<bool> PossuiVinculosAsync(int id);
    Task AdicionarAsync(Cliente cliente);
    Task AtualizarAsync(Cliente cliente);
    Task RemoverAsync(Cliente cliente);
}
