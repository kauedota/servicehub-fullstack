using ServiceHub.Api.Domain.Enums;

namespace ServiceHub.Api.Domain.Entities;

// Entidade = classe que representa uma tabela (como @Entity no JPA).
// Diferença: no EF Core NÃO precisamos de anotações. O mapeamento fica no AppDbContext (Fluent API).
// "{ get; set; }" = propriedade automática. Substitui o par getter/setter do Java (ou o @Data do Lombok).
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}
