namespace TimewareServiceHub.Api.Configuration;

// "Options pattern": uma classe fortemente tipada ligada a uma seção do appsettings.json.
// Equivalente ao @ConfigurationProperties(prefix = "jwt") do Spring Boot.
// Registrado no Program.cs com: builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"))
public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public int ExpiracaoMinutos { get; set; } = 120;
}
