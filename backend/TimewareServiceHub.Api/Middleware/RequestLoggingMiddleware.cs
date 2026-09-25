using System.Diagnostics;

namespace TimewareServiceHub.Api.Middleware;

// Observabilidade básica: loga TODA requisição com método, rota, status e tempo de resposta,
// e propaga um Correlation ID (header X-Correlation-Id) para rastrear a mesma requisição
// entre frontend, API e outros serviços.
public class RequestLoggingMiddleware
{
    private const string HeaderCorrelationId = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderCorrelationId].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");
        context.Response.Headers[HeaderCorrelationId] = correlationId;

        // BeginScope: todo log feito DURANTE esta requisição (inclusive nos services) leva o CorrelationId junto.
        // Parecido com o MDC do SLF4J/Logback.
        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            var cronometro = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                cronometro.Stop();
                _logger.LogInformation("HTTP {Metodo} {Caminho} respondeu {StatusCode} em {DuracaoMs} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    cronometro.ElapsedMilliseconds);
            }
        }
    }
}
