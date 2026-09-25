using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimewareServiceHub.Api.DTOs;
using TimewareServiceHub.Api.Extensions;
using TimewareServiceHub.Api.Services;

namespace TimewareServiceHub.Api.Controllers;

[ApiController]
[Route("api/chamados")]
[Authorize]
public class ChamadosController : ControllerBase
{
    private readonly IChamadoService _chamadoService;

    public ChamadosController(IChamadoService chamadoService)
    {
        _chamadoService = chamadoService;
    }

    /// GET /api/chamados?status=Aberto&prioridade=Alta&clienteId=1&responsavelId=2&busca=erro
    /// [FromQuery] num objeto: o ASP.NET preenche cada propriedade a partir da query string.
    [HttpGet]
    public async Task<ActionResult<List<ChamadoDto>>> Listar([FromQuery] ChamadoFiltro filtro) =>
        Ok(await _chamadoService.ListarAsync(filtro));

    /// GET /api/chamados/10
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ChamadoDto>> ObterPorId(int id) =>
        Ok(await _chamadoService.ObterPorIdAsync(id));

    /// POST /api/chamados
    [HttpPost]
    public async Task<ActionResult<ChamadoDto>> Criar([FromBody] CriarChamadoRequest request)
    {
        var chamado = await _chamadoService.CriarAsync(request, User.ObterUsuarioId());
        return CreatedAtAction(nameof(ObterPorId), new { id = chamado.Id }, chamado);
    }

    /// PUT /api/chamados/10  (prioridade, responsável, projeto, título, descrição)
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ChamadoDto>> Atualizar(int id, [FromBody] AtualizarChamadoRequest request) =>
        Ok(await _chamadoService.AtualizarAsync(id, request, User.ObterUsuarioId()));

    /// PATCH /api/chamados/10/status  { "status": "Concluido", "comentario": "Resolvido" }
    /// PATCH = alteração parcial (só o status). PUT = substitui o recurso.
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ChamadoDto>> AlterarStatus(int id, [FromBody] AlterarStatusChamadoRequest request) =>
        Ok(await _chamadoService.AlterarStatusAsync(id, request, User.ObterUsuarioId()));

    /// GET /api/chamados/10/historico  (auditoria)
    [HttpGet("{id:int}/historico")]
    public async Task<ActionResult<List<AuditoriaDto>>> Historico(int id) =>
        Ok(await _chamadoService.ListarHistoricoAsync(id));
}
