namespace TimewareServiceHub.Api.Exceptions;

// Exceções de domínio. O ExceptionHandlingMiddleware converte cada uma em um status HTTP.
// Em C# TODAS as exceções são "unchecked": não existe "throws" na assinatura do método,
// diferente das checked exceptions do Java.

/// <summary>Vira HTTP 404.</summary>
public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string mensagem) : base(mensagem) { }
}

/// <summary>Regra de negócio violada. Vira HTTP 400.</summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}

/// <summary>Login inválido. Vira HTTP 401.</summary>
public class CredenciaisInvalidasException : Exception
{
    public CredenciaisInvalidasException() : base("E-mail ou senha inválidos.") { }
}
