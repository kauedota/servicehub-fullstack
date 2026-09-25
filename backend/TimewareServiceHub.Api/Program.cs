using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TimewareServiceHub.Api.Configuration;
using TimewareServiceHub.Api.Data;
using TimewareServiceHub.Api.Middleware;
using TimewareServiceHub.Api.Repositories;
using TimewareServiceHub.Api.Services;

// =====================================================================================
// Program.cs = ponto de entrada. Faz o papel de:
//   - classe com @SpringBootApplication + main()
//   - classes @Configuration com @Bean (registro de dependências)
//   - SecurityFilterChain (autenticação/autorização)
// Diferença importante: no Spring a descoberta de beans é AUTOMÁTICA (component scan).
// No ASP.NET Core o registro é EXPLÍCITO: você escreve builder.Services.AddScoped<Interface, Classe>().
// =====================================================================================

var builder = WebApplication.CreateBuilder(args);

// ---------- 1) Controllers + JSON (enums como texto: "Aberto" em vez de 0; datas sempre em UTC) ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    });

// ---------- 2) Banco de dados ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

// EF Core: DbContext registrado como Scoped (uma instância por requisição)
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
// Dapper / ADO.NET: fábrica de conexões (Singleton: uma instância para a aplicação toda)
builder.Services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddScoped<DatabaseInitializer>();

// ---------- 3) Injeção de Dependência (equivalente a @Repository / @Service + @Autowired) ----------
// Tempo de vida:
//   AddSingleton  -> uma instância para sempre            (≈ @Scope("singleton"), padrão do Spring)
//   AddScoped     -> uma instância por requisição HTTP    (≈ @RequestScope)
//   AddTransient  -> nova instância a cada injeção        (≈ @Scope("prototype"))
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProjetoRepository, ProjetoRepository>();
builder.Services.AddScoped<IChamadoRepository, ChamadoRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProjetoService, ProjetoService>();
builder.Services.AddScoped<IChamadoService, ChamadoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ---------- 4) Autenticação JWT ----------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Seção Jwt não configurada.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        // Mantém os nomes curtos das claims ("sub", "role") em vez de convertê-los para URIs longas.
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

// ---------- 5) CORS (libera o frontend React em outra porta) ----------
var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
    p.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("X-Correlation-Id")));

// ---------- 6) Swagger com botão "Authorize" para colar o JWT ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Timeware Service Hub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole aqui apenas o token retornado por POST /api/auth/login"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---------- 7) Health check (equivalente ao /actuator/health) ----------
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("sqlserver");

// =====================================================================================
// PIPELINE HTTP: a ORDEM IMPORTA. Cada "Use..." é um middleware por onde a requisição passa.
// =====================================================================================
var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();      // 1º: mede o tempo total e loga o status final
app.UseMiddleware<ExceptionHandlingMiddleware>();   // 2º: captura exceções de tudo que vem depois

// Em produção normalmente o Swagger fica desligado ou protegido. Aqui fica ligado para a demonstração.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");
app.UseAuthentication();   // lê o header Authorization e valida o JWT -> preenche HttpContext.User
app.UseAuthorization();    // aplica [Authorize] / [Authorize(Roles = "Admin")]

app.MapControllers();
app.MapHealthChecks("/health");

// Cria/atualiza o banco ao subir (facilita a execução local e a demonstração; desligar em produção).
if (app.Configuration.GetValue<bool>("Database:InicializarAoSubir"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InicializarAsync();
}

app.Run();

// Permite testes de integração com WebApplicationFactory<Program>.
public partial class Program { }
