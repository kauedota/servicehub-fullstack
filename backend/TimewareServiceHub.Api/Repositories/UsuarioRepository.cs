using Microsoft.EntityFrameworkCore;
using TimewareServiceHub.Api.Data;
using TimewareServiceHub.Api.Domain.Entities;

namespace TimewareServiceHub.Api.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;

    public UsuarioRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Usuario?> ObterPorEmailAsync(string email) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public Task<Usuario?> ObterPorIdAsync(int id) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public Task<List<Usuario>> ListarAtivosAsync() =>
        _db.Usuarios.AsNoTracking().Where(u => u.Ativo).OrderBy(u => u.Nome).ToListAsync();
}
