namespace TimewareServiceHub.Api.Domain.Enums;

// Enums em C# são parecidos com os do Java, mas mais simples:
// por baixo são inteiros (0, 1, 2...) e não podem ter métodos/campos como no Java.
// No banco gravamos como TEXTO (ex.: 'Aberto'), configurado no AppDbContext com HasConversion<string>().

public enum PerfilUsuario
{
    Admin,
    Analista
}

public enum StatusProjeto
{
    Planejado,
    EmAndamento,
    Pausado,
    Concluido
}

public enum StatusChamado
{
    Aberto,
    EmAndamento,
    Concluido,
    Cancelado
}

public enum Prioridade
{
    Baixa,
    Media,
    Alta,
    Critica
}
