using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Api.DTOs;
using ServiceHub.Api.Services;

namespace ServiceHub.Api.Controllers;

// Usado pelo frontend para preencher os selects de "Responsável".
[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> Listar() =>
        Ok(await _usuarioService.ListarAsync());
}
