using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceHub.Api.Configuration;
using ServiceHub.Api.Domain.Entities;

namespace ServiceHub.Api.Services;

public interface ITokenService
{
    // Retorno com "tupla nomeada": devolve dois valores sem criar uma classe. Não existe no Java.
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}

// Gera o JWT. Estrutura do token: HEADER.PAYLOAD.ASSINATURA (Base64Url).
// O payload traz as "claims" (dados do usuário). A assinatura (HMAC-SHA256 com a chave secreta)
// garante que ninguém alterou o token. ATENÇÃO: o payload NÃO é criptografado, só assinado.
public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    // IOptions<JwtOptions> = seção "Jwt" do appsettings.json injetada já convertida para objeto.
    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_options.ExpiracaoMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),   // "sub": id do usuário
            new("name", usuario.Nome),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new("role", usuario.Perfil.ToString()),                   // usado por [Authorize(Roles = "Admin")]
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // id único do token
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
