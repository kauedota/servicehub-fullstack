using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Api.DTOs;
using ServiceHub.Api.Extensions;
using ServiceHub.Api.Services;

namespace ServiceHub.Api.Controllers;

// CONTROLLER = camada HTTP. Recebe a requisição, chama o service e escolhe o status code.
//
// Spring Boot                          ASP.NET Core
// @RestController                  =>  [ApiController] + herdar de ControllerBase
// @RequestMapping("/api/clientes") =>  [Route("api/clientes")]
// @GetMapping("/{id}")             =>  [HttpGet("{id:int}")]
// @PathVariable / @RequestParam    =>  parâmetro do método / [FromQuery]
// @RequestBody @Valid              =>  [FromBody] (a validação é automática com [ApiController])
// ResponseEntity<T>                =>  ActionResult<T>  (Ok(), CreatedAtAction(), NoContent(), NotFound())
// @PreAuthorize("isAuthenticated")  =>  [Authorize]
[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// GET /api/clientes?busca=varejo
    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> Listar([FromQuery] string? busca)
    {
        var clientes = await _clienteService.ListarAsync(busca);
        return Ok(clientes);                                        // 200
    }

    /// GET /api/clientes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> ObterPorId(int id)
    {
        // Se não existir, o service lança RecursoNaoEncontradoException e o middleware devolve 404.
        return Ok(await _clienteService.ObterPorIdAsync(id));
    }

    /// POST /api/clientes
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] ClienteRequest request)
    {
        // User = usuário autenticado (vindo do JWT). Como o Authentication/Principal do Spring Security.
        var cliente = await _clienteService.CriarAsync(request, User.ObterUsuarioId());

        // 201 Created + header Location: /api/clientes/{id}
        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
    }

    /// PUT /api/clientes/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Atualizar(int id, [FromBody] ClienteRequest request)
    {
        return Ok(await _clienteService.AtualizarAsync(id, request, User.ObterUsuarioId()));
    }

    /// DELETE /api/clientes/5   (somente Admin)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]                                    // como @PreAuthorize("hasRole('ADMIN')")
    public async Task<IActionResult> Excluir(int id)
    {
        await _clienteService.ExcluirAsync(id, User.ObterUsuarioId());
        return NoContent();                                         // 204
    }
}
