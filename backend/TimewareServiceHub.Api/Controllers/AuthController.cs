using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimewareServiceHub.Api.DTOs;
using TimewareServiceHub.Api.Services;

namespace TimewareServiceHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// POST /api/auth/login  { "email": "...", "senha": "..." }  =>  200 { token, expiraEm, usuario }
    [HttpPost("login")]
    [AllowAnonymous]   // rota pública (como .permitAll() no SecurityFilterChain)
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var resposta = await _usuarioService.LoginAsync(request);
        return Ok(resposta);
    }
}
