using TimewareServiceHub.Api.DTOs;
using TimewareServiceHub.Api.Exceptions;
using TimewareServiceHub.Api.Repositories;

namespace TimewareServiceHub.Api.Services;

public interface IUsuarioService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<List<UsuarioDto>> ListarAsync();
}

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(IUsuarioRepository usuarios, ITokenService tokenService, ILogger<UsuarioService> logger)
    {
        _usuarios = usuarios;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObterPorEmailAsync(email);

        // Mesma mensagem para "usuário não existe" e "senha errada": não revelamos quais e-mails existem.
        if (usuario is null || !usuario.Ativo || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            // Log estruturado: {Email} vira um CAMPO pesquisável na ferramenta de logs, não só texto.
            // Nunca logar a senha.
            _logger.LogWarning("Tentativa de login inválida para {Email}", email);
            throw new CredenciaisInvalidasException();
        }

        var (token, expiraEm) = _tokenService.GerarToken(usuario);   // "desconstrução" da tupla
        _logger.LogInformation("Usuário {UsuarioId} autenticado", usuario.Id);

        return new LoginResponse(token, expiraEm,
            new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil));
    }

    public async Task<List<UsuarioDto>> ListarAsync()
    {
        var usuarios = await _usuarios.ListarAtivosAsync();
        // Select = map() do Stream do Java. ToList() = collect(Collectors.toList()).
        return usuarios.Select(u => new UsuarioDto(u.Id, u.Nome, u.Email, u.Perfil)).ToList();
    }
}
