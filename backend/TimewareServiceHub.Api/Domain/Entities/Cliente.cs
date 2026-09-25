namespace TimewareServiceHub.Api.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    // CNPJ/CPF. Tem UNIQUE no banco.
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // "string?" = pode ser nulo. Com <Nullable>enable</Nullable> o compilador avisa
    // se você usar um valor possivelmente nulo sem checar (o Java não tem isso nativamente).
    public string? Telefone { get; set; }

    // Varejo, Saúde, Finanças...
    public string Setor { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    // Navegação 1:N (como @OneToMany(mappedBy = "cliente") no JPA)
    public ICollection<Projeto> Projetos { get; set; } = new List<Projeto>();
}
