using ServiceHub.Api.Domain.Entities;
using ServiceHub.Api.DTOs;
using ServiceHub.Api.Exceptions;
using ServiceHub.Api.Repositories;

namespace ServiceHub.Api.Services;

public interface IClienteService
{
    Task<List<ClienteDto>> ListarAsync(string? busca);
    Task<ClienteDto> ObterPorIdAsync(int id);
    Task<ClienteDto> CriarAsync(ClienteRequest request, int usuarioId);
    Task<ClienteDto> AtualizarAsync(int id, ClienteRequest request, int usuarioId);
    Task ExcluirAsync(int id, int usuarioId);
}

// SERVICE = regras de negócio (como uma classe @Service no Spring).
// Não conhece HTTP (nada de status code aqui) e não conhece SQL (isso é do repositório).
public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clientes;
    private readonly IAuditoriaRepository _auditoria;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(IClienteRepository clientes, IAuditoriaRepository auditoria, ILogger<ClienteService> logger)
    {
        _clientes = clientes;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<List<ClienteDto>> ListarAsync(string? busca)
    {
        var clientes = await _clientes.ListarAsync(busca);
        return clientes.Select(ParaDto).ToList();
    }

    public async Task<ClienteDto> ObterPorIdAsync(int id)
    {
        // "??" + throw: se for null, lança. Parecido com Optional.orElseThrow() do Java.
        var cliente = await _clientes.ObterPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException($"Cliente {id} não encontrado.");
        return ParaDto(cliente);
    }

    public async Task<ClienteDto> CriarAsync(ClienteRequest request, int usuarioId)
    {
        var documento = request.Documento.Trim();

        if (await _clientes.DocumentoExisteAsync(documento))
            throw new RegraNegocioException($"Já existe um cliente com o documento {documento}.");

        // "Object initializer": cria e preenche propriedades num só bloco (substitui o Builder do Lombok).
        var cliente = new Cliente
        {
            Nome = request.Nome.Trim(),
            Documento = documento,
            Email = request.Email.Trim().ToLowerInvariant(),
            Telefone = request.Telefone?.Trim(),
            Setor = request.Setor.Trim(),
            CriadoEm = DateTime.UtcNow
        };

        await _clientes.AdicionarAsync(cliente);
        await _auditoria.RegistrarAsync("Cliente", cliente.Id, AcoesAuditoria.Criacao,
            $"Cliente '{cliente.Nome}' criado.", usuarioId);

        _logger.LogInformation("Cliente {ClienteId} criado pelo usuário {UsuarioId}", cliente.Id, usuarioId);
        return ParaDto(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(int id, ClienteRequest request, int usuarioId)
    {
        var cliente = await _clientes.ObterPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException($"Cliente {id} não encontrado.");

        var documento = request.Documento.Trim();
        if (await _clientes.DocumentoExisteAsync(documento, ignorarId: id))
            throw new RegraNegocioException($"Já existe outro cliente com o documento {documento}.");

        // Só alteramos as propriedades. O EF Core detecta a mudança (tracking) e gera o UPDATE.
        cliente.Nome = request.Nome.Trim();
        cliente.Documento = documento;
        cliente.Email = request.Email.Trim().ToLowerInvariant();
        cliente.Telefone = request.Telefone?.Trim();
        cliente.Setor = request.Setor.Trim();
        cliente.AtualizadoEm = DateTime.UtcNow;

        await _clientes.AtualizarAsync(cliente);
        await _auditoria.RegistrarAsync("Cliente", cliente.Id, AcoesAuditoria.Atualizacao,
            "Dados cadastrais atualizados.", usuarioId);

        return ParaDto(cliente);
    }

    public async Task ExcluirAsync(int id, int usuarioId)
    {
        var cliente = await _clientes.ObterPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException($"Cliente {id} não encontrado.");

        // Regra de negócio explícita, com mensagem amigável, em vez de deixar estourar erro de FK do banco.
        if (await _clientes.PossuiVinculosAsync(id))
            throw new RegraNegocioException("Não é possível excluir: o cliente possui projetos ou chamados.");

        await _clientes.RemoverAsync(cliente);
        await _auditoria.RegistrarAsync("Cliente", id, AcoesAuditoria.Exclusao,
            $"Cliente '{cliente.Nome}' excluído.", usuarioId);

        _logger.LogInformation("Cliente {ClienteId} excluído pelo usuário {UsuarioId}", id, usuarioId);
    }

    // Mapeamento manual Entidade -> DTO (em projetos maiores: AutoMapper ou Mapster, parecidos com o MapStruct).
    private static ClienteDto ParaDto(Cliente c) =>
        new(c.Id, c.Nome, c.Documento, c.Email, c.Telefone, c.Setor, c.CriadoEm, c.AtualizadoEm);
}
