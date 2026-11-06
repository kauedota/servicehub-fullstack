using Microsoft.EntityFrameworkCore;
using ServiceHub.Api.Data;
using ServiceHub.Api.Domain.Entities;

namespace ServiceHub.Api.Repositories;

// Repositório com EF CORE: não escrevemos SQL, escrevemos LINQ e o EF traduz para T-SQL.
public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _db;

    // Injeção de dependência pelo construtor (como @RequiredArgsConstructor / construtor com @Autowired).
    public ClienteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Cliente>> ListarAsync(string? busca)
    {
        // AsNoTracking: só leitura, o EF não guarda as entidades para detectar mudanças => mais rápido.
        var query = _db.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            // Contains vira: WHERE Nome LIKE '%termo%'
            query = query.Where(c => c.Nome.Contains(termo)
                                  || c.Documento.Contains(termo)
                                  || c.Email.Contains(termo));
        }

        // Nada foi ao banco até aqui. O SQL só é executado no ToListAsync() (execução adiada).
        return await query.OrderBy(c => c.Nome).ToListAsync();
    }

    // COM tracking (padrão): a entidade volta "vigiada". Se o service alterar uma propriedade,
    // o SaveChanges gera o UPDATE só das colunas alteradas. Igual ao estado "managed" do JPA.
    public Task<Cliente?> ObterPorIdAsync(int id) =>
        _db.Clientes.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> DocumentoExisteAsync(string documento, int? ignorarId = null) =>
        _db.Clientes.AnyAsync(c => c.Documento == documento && (ignorarId == null || c.Id != ignorarId));

    public async Task<bool> PossuiVinculosAsync(int id) =>
        await _db.Projetos.AnyAsync(p => p.ClienteId == id)
        || await _db.Chamados.AnyAsync(c => c.ClienteId == id);

    public async Task AdicionarAsync(Cliente cliente)
    {
        _db.Clientes.Add(cliente);      // marca como "Added"
        await _db.SaveChangesAsync();   // INSERT ... ; o Id gerado (IDENTITY) volta preenchido em cliente.Id
    }

    public async Task AtualizarAsync(Cliente cliente)
    {
        // A entidade já está rastreada (veio do ObterPorIdAsync), então basta salvar.
        await _db.SaveChangesAsync();   // UPDATE Clientes SET ... WHERE Id = @id
    }

    public async Task RemoverAsync(Cliente cliente)
    {
        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();   // DELETE FROM Clientes WHERE Id = @id
    }
}
