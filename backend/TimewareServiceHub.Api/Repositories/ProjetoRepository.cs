using Microsoft.EntityFrameworkCore;
using TimewareServiceHub.Api.Data;
using TimewareServiceHub.Api.Domain.Entities;

namespace TimewareServiceHub.Api.Repositories;

// EF Core com relacionamentos.
public class ProjetoRepository : IProjetoRepository
{
    private readonly AppDbContext _db;

    public ProjetoRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Projeto>> ListarAsync(int? clienteId)
    {
        // Include = carrega o relacionamento no mesmo SELECT (JOIN).
        // Equivalente ao "JOIN FETCH" do JPQL. Evita o problema N+1.
        var query = _db.Projetos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Responsavel)
            .AsQueryable();

        if (clienteId is not null)
            query = query.Where(p => p.ClienteId == clienteId);

        return await query.OrderBy(p => p.Nome).ToListAsync();
    }

    public Task<Projeto?> ObterPorIdAsync(int id) =>
        _db.Projetos.FirstOrDefaultAsync(p => p.Id == id);

    public Task<Projeto?> ObterComDetalhesAsync(int id) =>
        _db.Projetos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Responsavel)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task AdicionarAsync(Projeto projeto)
    {
        _db.Projetos.Add(projeto);
        await _db.SaveChangesAsync();
    }

    public Task SalvarAlteracoesAsync() => _db.SaveChangesAsync();
}
