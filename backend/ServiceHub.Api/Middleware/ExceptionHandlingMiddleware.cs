using Microsoft.AspNetCore.Mvc;
using ServiceHub.Api.Exceptions;

namespace ServiceHub.Api.Middleware;

// Tratamento GLOBAL de exceções. Equivalente ao @RestControllerAdvice + @ExceptionHandler do Spring.
//
// Middleware = um "elo" do pipeline HTTP. Cada requisição passa por todos os middlewares em ordem
// (parecido com os Filters do Servlet / a SecurityFilterChain). Aqui envolvemos o resto do pipeline
// num try/catch: qualquer exceção lançada em controller/service/repository cai aqui.
//
// Resposta no padrão "Problem Details" (RFC 7807), o mesmo usado pelo Spring 6 (ProblemDetail).
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);   // chama o próximo middleware (e, no fim, o controller)
        }
        catch (Exception ex)
        {
            await TratarAsync(context, ex);
        }
    }

    private async Task TratarAsync(HttpContext context, Exception ex)
    {
        // "switch expression" com pattern matching por tipo (Java 21 tem algo parecido).
        var (status, titulo) = ex switch
        {
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            RegraNegocioException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            CredenciaisInvalidasException => (StatusCodes.Status401Unauthorized, "Não autorizado"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        if (status == StatusCodes.Status500InternalServerError)
            // Erro inesperado: loga com stack trace completo (nível Error).
            _logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
        else
            // Erro esperado (de negócio): Warning, sem poluir com stack trace.
            _logger.LogWarning("{Titulo}: {Mensagem}", titulo, ex.Message);

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            // Em erro 500 NÃO devolvemos ex.Message: pode vazar detalhes internos (SQL, caminhos...).
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro inesperado. Informe o traceId ao suporte."
                : ex.Message,
            Instance = context.Request.Path
        };
        problema.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problema, options: null, contentType: "application/problem+json");
    }
}
