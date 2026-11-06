using ServiceHub.Api.Domain.Entities;
using ServiceHub.Api.Domain.Enums;
using ServiceHub.Api.DTOs;
using ServiceHub.Api.Exceptions;
using ServiceHub.Api.Repositories;

namespace ServiceHub.Api.Services;

public interface IChamadoService
{
    Task<List<ChamadoDto>> ListarAsync(ChamadoFiltro filtro);
    Task<ChamadoDto> ObterPorIdAsync(int id);
    Task<ChamadoDto> CriarAsync(CriarChamadoRequest request, int usuarioId);
    Task<ChamadoDto> AtualizarAsync(int id, AtualizarChamadoRequest request, int usuarioId);
    Task<ChamadoDto> AlterarStatusAsync(int id, AlterarStatusChamadoRequest request, int usuarioId);
    Task<List<AuditoriaDto>> ListarHistoricoAsync(int id);
}

public class ChamadoService : IChamadoService
{
    private const string Entidade = "Chamado";

    private readonly IChamadoRepository _chamados;
    private readonly IClienteRepository _clientes;
    private readonly IProjetoRepository _projetos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IAuditoriaRepository _auditoria;
    private readonly ILogger<ChamadoService> _logger;

    public ChamadoService(
        IChamadoRepository chamados,
        IClienteRepository clientes,
        IProjetoRepository projetos,
        IUsuarioRepository usuarios,
        IAuditoriaRepository auditoria,
        ILogger<ChamadoService> logger)
    {
        _chamados = chamados;
        _clientes = clientes;
        _projetos = projetos;
        _usuarios = usuarios;
        _auditoria = auditoria;
        _logger = logger;
    }

    public Task<List<ChamadoDto>> ListarAsync(ChamadoFiltro filtro) => _chamados.ListarAsync(filtro);

    public async Task<ChamadoDto> ObterPorIdAsync(int id) =>
        await _chamados.ObterDtoPorIdAsync(id)
        ?? throw new RecursoNaoEncontradoException($"Chamado {id} não encontrado.");

    public async Task<ChamadoDto> CriarAsync(CriarChamadoRequest request, int usuarioId)
    {
        _ = await _clientes.ObterPorIdAsync(request.ClienteId)
            ?? throw new RegraNegocioException("Cliente informado não existe.");

        await ValidarProjetoAsync(request.ProjetoId, request.ClienteId);
        await ValidarResponsavelAsync(request.ResponsavelId);

        var chamado = new Chamado
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            ClienteId = request.ClienteId,
            ProjetoId = request.ProjetoId,
            ResponsavelId = request.ResponsavelId,
            Prioridade = request.Prioridade!.Value,   // "!" = "confie em mim, não é nulo" (o [Required] já garantiu)
            Status = StatusChamado.Aberto,
            CriadoEm = DateTime.UtcNow
        };

        await _chamados.AdicionarAsync(chamado);
        await _auditoria.RegistrarAsync(Entidade, chamado.Id, AcoesAuditoria.Criacao,
            $"Chamado aberto com prioridade {chamado.Prioridade}.", usuarioId);

        _logger.LogInformation("Chamado {ChamadoId} criado (prioridade {Prioridade}) pelo usuário {UsuarioId}",
            chamado.Id, chamado.Prioridade, usuarioId);

        return await ObterPorIdAsync(chamado.Id);
    }

    // Definir prioridade, atribuir responsável, trocar projeto, editar texto.
    public async Task<ChamadoDto> AtualizarAsync(int id, AtualizarChamadoRequest request, int usuarioId)
    {
        var chamado = await _chamados.ObterEntidadeAsync(id)
            ?? throw new RecursoNaoEncontradoException($"Chamado {id} não encontrado.");

        if (chamado.Status == StatusChamado.Cancelado)
            throw new RegraNegocioException("Chamados cancelados não podem ser alterados.");

        await ValidarProjetoAsync(request.ProjetoId, chamado.ClienteId);
        await ValidarResponsavelAsync(request.ResponsavelId);

        // Monta um resumo do que mudou para a auditoria.
        var mudancas = new List<string>();
        var novaPrioridade = request.Prioridade!.Value;
        if (chamado.Prioridade != novaPrioridade)
            mudancas.Add($"Prioridade: {chamado.Prioridade} -> {novaPrioridade}");
        if (chamado.ResponsavelId != request.ResponsavelId)
            mudancas.Add($"Responsável: {chamado.ResponsavelId?.ToString() ?? "nenhum"} -> {request.ResponsavelId?.ToString() ?? "nenhum"}");
        if (chamado.ProjetoId != request.ProjetoId)
            mudancas.Add("Projeto alterado");
        if (chamado.Titulo != request.Titulo.Trim() || chamado.Descricao != request.Descricao.Trim())
            mudancas.Add("Título/descrição alterados");

        chamado.Titulo = request.Titulo.Trim();
        chamado.Descricao = request.Descricao.Trim();
        chamado.ProjetoId = request.ProjetoId;
        chamado.ResponsavelId = request.ResponsavelId;
        chamado.Prioridade = novaPrioridade;
        chamado.AtualizadoEm = DateTime.UtcNow;

        await _chamados.SalvarAlteracoesAsync();

        if (mudancas.Count > 0)
            await _auditoria.RegistrarAsync(Entidade, id, AcoesAuditoria.Atualizacao, string.Join("; ", mudancas), usuarioId);

        return await ObterPorIdAsync(id);
    }

    // Regras de status:
    //  - chamado Cancelado não muda mais;
    //  - não pode "mudar" para o mesmo status;
    //  - ao Concluir preenche ConcluidoEm; ao reabrir, limpa.
    public async Task<ChamadoDto> AlterarStatusAsync(int id, AlterarStatusChamadoRequest request, int usuarioId)
    {
        var chamado = await _chamados.ObterEntidadeAsync(id)
            ?? throw new RecursoNaoEncontradoException($"Chamado {id} não encontrado.");

        var novoStatus = request.Status!.Value;

        if (chamado.Status == StatusChamado.Cancelado)
            throw new RegraNegocioException("Chamados cancelados não podem mudar de status.");

        if (chamado.Status == novoStatus)
            throw new RegraNegocioException($"O chamado já está com status {novoStatus}.");

        var statusAnterior = chamado.Status;
        chamado.Status = novoStatus;
        chamado.AtualizadoEm = DateTime.UtcNow;
        chamado.ConcluidoEm = novoStatus == StatusChamado.Concluido ? DateTime.UtcNow : null;

        await _chamados.SalvarAlteracoesAsync();

        var detalhes = $"Status: {statusAnterior} -> {novoStatus}";
        if (!string.IsNullOrWhiteSpace(request.Comentario))
            detalhes += $". Comentário: {request.Comentario.Trim()}";

        await _auditoria.RegistrarAsync(Entidade, id, AcoesAuditoria.AlteracaoStatus, detalhes, usuarioId);

        _logger.LogInformation("Chamado {ChamadoId}: {StatusAnterior} -> {StatusNovo} (usuário {UsuarioId})",
            id, statusAnterior, novoStatus, usuarioId);

        return await ObterPorIdAsync(id);
    }

    public async Task<List<AuditoriaDto>> ListarHistoricoAsync(int id)
    {
        _ = await ObterPorIdAsync(id); // garante 404 se o chamado não existir
        return await _auditoria.ListarAsync(Entidade, id);
    }

    private async Task ValidarProjetoAsync(int? projetoId, int clienteId)
    {
        if (projetoId is null) return;

        var projeto = await _projetos.ObterPorIdAsync(projetoId.Value)
            ?? throw new RegraNegocioException("Projeto informado não existe.");

        if (projeto.ClienteId != clienteId)
            throw new RegraNegocioException("O projeto informado não pertence ao cliente do chamado.");
    }

    private async Task ValidarResponsavelAsync(int? responsavelId)
    {
        if (responsavelId is null) return;

        var usuario = await _usuarios.ObterPorIdAsync(responsavelId.Value);
        if (usuario is null || !usuario.Ativo)
            throw new RegraNegocioException("Responsável informado não existe ou está inativo.");
    }
}
