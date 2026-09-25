using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimewareServiceHub.Api.DTOs;
using TimewareServiceHub.Api.Extensions;
using TimewareServiceHub.Api.Services;

namespace TimewareServiceHub.Api.Controllers;

[ApiController]
[Route("api/projetos")]
[Authorize]
public class ProjetosController : ControllerBase
{
    private readonly IProjetoService _projetoService;

    public ProjetosController(IProjetoService projetoService)
    {
        _projetoService = projetoService;
    }

    /// GET /api/projetos?clienteId=1
    [HttpGet]
    public async Task<ActionResult<List<ProjetoDto>>> Listar([FromQuery] int? clienteId) =>
        Ok(await _projetoService.ListarAsync(clienteId));

    /// GET /api/projetos/3
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjetoDto>> ObterPorId(int id) =>
        Ok(await _projetoService.ObterPorIdAsync(id));

    /// POST /api/projetos
    [HttpPost]
    public async Task<ActionResult<ProjetoDto>> Criar([FromBody] ProjetoRequest request)
    {
        var projeto = await _projetoService.CriarAsync(request, User.ObterUsuarioId());
        return CreatedAtAction(nameof(ObterPorId), new { id = projeto.Id }, projeto);
    }

    /// PATCH /api/projetos/3/status  { "status": "EmAndamento" }
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ProjetoDto>> AlterarStatus(int id, [FromBody] AlterarStatusProjetoRequest request) =>
        Ok(await _projetoService.AlterarStatusAsync(id, request.Status!.Value, User.ObterUsuarioId()));
}
