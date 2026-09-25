using Microsoft.EntityFrameworkCore;
using TimewareServiceHub.Api.Domain.Entities;

namespace TimewareServiceHub.Api.Data;

// DbContext = a "sessão" com o banco. Equivalente ao EntityManager do JPA.
// Ele rastreia as entidades carregadas (change tracking) e gera INSERT/UPDATE/DELETE no SaveChanges().
// Registrado como SCOPED no Program.cs: uma instância por requisição HTTP.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet<T> = uma tabela consultável com LINQ. Parecido com um JpaRepository "embutido".
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<Chamado> Chamados => Set<Chamado>();

    // Fluent API: mapeamento sem anotações nas entidades.
    // Equivalente a @Table, @Column(length=...), @ManyToOne, @JoinColumn etc.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.SenhaHash).HasMaxLength(200).IsRequired();
            // Grava o enum como texto ('Admin') em vez de número (0). Como @Enumerated(EnumType.STRING).
            e.Property(x => x.Perfil).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Documento).HasMaxLength(18).IsRequired();
            e.HasIndex(x => x.Documento).IsUnique();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(20);
            e.Property(x => x.Setor).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Projeto>(e =>
        {
            e.ToTable("Projetos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(1000);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            // Restrict = não apaga em cascata (o banco bloqueia excluir cliente com projetos).
            e.HasOne(x => x.Cliente).WithMany(c => c.Projetos)
                .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Responsavel).WithMany()
                .HasForeignKey(x => x.ResponsavelId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Chamado>(e =>
        {
            e.ToTable("Chamados");
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Prioridade).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(x => x.Cliente).WithMany()
                .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Projeto).WithMany()
                .HasForeignKey(x => x.ProjetoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Responsavel).WithMany()
                .HasForeignKey(x => x.ResponsavelId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
