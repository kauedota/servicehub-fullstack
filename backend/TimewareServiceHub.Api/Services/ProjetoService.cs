using TimewareServiceHub.Api.Domain.Entities;
using TimewareServiceHub.Api.Domain.Enums;
using TimewareServiceHub.Api.DTOs;
using TimewareServiceHub.Api.Exceptions;
using TimewareServiceHub.Api.Repositories;

namespace TimewareServiceHub.Api.Services;

public interface IProjetoService
{
    Task<List<ProjetoDto>> ListarAsync(int? clienteId);
    Task<ProjetoDto> ObterPorIdAsync(int id);
    Task<ProjetoDto> CriarAsync(ProjetoRequest request, int usuarioId);
    Task<ProjetoDto> AlterarStatusAsync(int id, StatusProjeto status, int usuarioId);
}

public class ProjetoService : IProjetoService
{
    private readonly IProjetoRepository _projetos;
    private readonly IClienteRepository _clientes;
    private readonly IUsuarioRepository _usuarios;
    private readonly IAuditoriaRepository _auditoria;

    public ProjetoService(IProjetoRepository projetos, IClienteRepository clientes,
        IUsuarioRepository usuarios, IAuditoriaRepository auditoria)
    {
        _projetos = projetos;
        _clientes = clientes;
        _usuarios = usuarios;
        _auditoria = auditoria;
    }

    public async Task<List<ProjetoDto>> ListarAsync(int? clienteId)
    {
        var projetos = await _projetos.ListarAsync(clienteId);
        return projetos.Select(ParaDto).ToList();
    }

    public async Task<ProjetoDto> ObterPorIdAsync(int id)
    {
        var projeto = await _projetos.ObterComDetalhesAsync(id)
            ?? throw new RecursoNaoEncontradoException($"Projeto {id} não encontrado.");
        return ParaDto(projeto);
    }

    public async Task<ProjetoDto> CriarAsync(ProjetoRequest request, int usuarioId)
    {
        _ = await _clientes.ObterPorIdAsync(request.ClienteId)
            ?? throw new RegraNegocioException("Cliente informado não existe.");

        var responsavel = await _usuarios.ObterPorIdAsync(request.ResponsavelId);
        if (responsavel is null || !responsavel.Ativo)
            throw new RegraNegocioException("Responsável informado não existe ou está inativo.");

        var projeto = new Projeto
        {
            ClienteId = request.ClienteId,
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim(),
            ResponsavelId = request.ResponsavelId,
            Status = StatusProjeto.Planejado,
            CriadoEm = DateTime.UtcNow
        };

        await _projetos.AdicionarAsync(projeto);
        await _auditoria.RegistrarAsync("Projeto", projeto.Id, AcoesAuditoria.Criacao,
            $"Projeto '{projeto.Nome}' criado.", usuarioId);

        return await ObterPorIdAsync(projeto.Id);
    }

    public async Task<ProjetoDto> AlterarStatusAsync(int id, StatusProjeto status, int usuarioId)
    {
        var projeto = await _projetos.ObterPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException($"Projeto {id} não encontrado.");

        if (projeto.Status == status)
            throw new RegraNegocioException($"O projeto já está com status {status}.");

        var anterior = projeto.Status;
        projeto.Status = status;
        projeto.AtualizadoEm = DateTime.UtcNow;
        await _projetos.SalvarAlteracoesAsync();

        await _auditoria.RegistrarAsync("Projeto", id, AcoesAuditoria.AlteracaoStatus,
            $"Status: {anterior} -> {status}", usuarioId);

        return await ObterPorIdAsync(id);
    }

    private static ProjetoDto ParaDto(Projeto p) => new(
        p.Id,
        p.ClienteId,
        p.Cliente?.Nome ?? string.Empty,
        p.Nome,
        p.Descricao,
        p.Status,
        p.ResponsavelId,
        p.Responsavel?.Nome ?? string.Empty,
        p.CriadoEm);
}
