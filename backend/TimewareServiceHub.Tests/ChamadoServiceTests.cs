using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TimewareServiceHub.Api.Domain.Entities;
using TimewareServiceHub.Api.Domain.Enums;
using TimewareServiceHub.Api.DTOs;
using TimewareServiceHub.Api.Exceptions;
using TimewareServiceHub.Api.Repositories;
using TimewareServiceHub.Api.Services;
using Xunit;

namespace TimewareServiceHub.Tests;

// Testes UNITÁRIOS do ChamadoService: os repositórios são "mockados", ou seja, nenhum banco é usado.
//
// JUnit 5 + Mockito                 xUnit + Moq
// @Test                         =>  [Fact]
// @ParameterizedTest            =>  [Theory] + [InlineData]
// @BeforeEach                   =>  construtor da classe de teste (xUnit cria uma instância por teste)
// assertThrows(X.class, ...)    =>  await Assert.ThrowsAsync<X>(...)
// when(mock.m()).thenReturn(x)  =>  mock.Setup(m => m.M()).ReturnsAsync(x)
// verify(mock).m()              =>  mock.Verify(m => m.M(), Times.Once)
public class ChamadoServiceTests
{
    private readonly Mock<IChamadoRepository> _chamados = new();
    private readonly Mock<IClienteRepository> _clientes = new();
    private readonly Mock<IProjetoRepository> _projetos = new();
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IAuditoriaRepository> _auditoria = new();
    private readonly ChamadoService _service;

    public ChamadoServiceTests()
    {
        _service = new ChamadoService(
            _chamados.Object, _clientes.Object, _projetos.Object, _usuarios.Object, _auditoria.Object,
            NullLogger<ChamadoService>.Instance);

        // Depois de salvar, o service relê o chamado como DTO. Devolvemos um DTO qualquer.
        _chamados.Setup(r => r.ObterDtoPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new ChamadoDto { Id = id });
    }

    [Fact]
    public async Task AlterarStatus_ParaConcluido_DevePreencherConcluidoEm_E_RegistrarAuditoria()
    {
        // Arrange (Given)
        var chamado = new Chamado { Id = 1, Status = StatusChamado.EmAndamento };
        _chamados.Setup(r => r.ObterEntidadeAsync(1)).ReturnsAsync(chamado);

        // Act (When)
        await _service.AlterarStatusAsync(1, new AlterarStatusChamadoRequest { Status = StatusChamado.Concluido }, usuarioId: 99);

        // Assert (Then)
        Assert.Equal(StatusChamado.Concluido, chamado.Status);
        Assert.NotNull(chamado.ConcluidoEm);
        _chamados.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
        _auditoria.Verify(a => a.RegistrarAsync("Chamado", 1, AcoesAuditoria.AlteracaoStatus,
            It.Is<string>(d => d.Contains("EmAndamento -> Concluido")), 99), Times.Once);
    }

    [Fact]
    public async Task AlterarStatus_ChamadoCancelado_DeveLancarRegraNegocio()
    {
        _chamados.Setup(r => r.ObterEntidadeAsync(1))
            .ReturnsAsync(new Chamado { Id = 1, Status = StatusChamado.Cancelado });

        await Assert.ThrowsAsync<RegraNegocioException>(() =>
            _service.AlterarStatusAsync(1, new AlterarStatusChamadoRequest { Status = StatusChamado.Aberto }, 1));

        _chamados.Verify(r => r.SalvarAlteracoesAsync(), Times.Never);
    }

    [Fact]
    public async Task AlterarStatus_ChamadoInexistente_DeveLancarNaoEncontrado()
    {
        _chamados.Setup(r => r.ObterEntidadeAsync(It.IsAny<int>())).ReturnsAsync((Chamado?)null);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            _service.AlterarStatusAsync(123, new AlterarStatusChamadoRequest { Status = StatusChamado.Concluido }, 1));
    }

    [Theory]
    [InlineData(StatusChamado.Concluido, StatusChamado.Aberto)]
    [InlineData(StatusChamado.Aberto, StatusChamado.EmAndamento)]
    public async Task AlterarStatus_SaindoDeConcluidoOuNaoConcluindo_DeveLimparConcluidoEm(StatusChamado atual, StatusChamado novo)
    {
        var chamado = new Chamado { Id = 1, Status = atual, ConcluidoEm = DateTime.UtcNow };
        _chamados.Setup(r => r.ObterEntidadeAsync(1)).ReturnsAsync(chamado);

        await _service.AlterarStatusAsync(1, new AlterarStatusChamadoRequest { Status = novo }, 1);

        Assert.Null(chamado.ConcluidoEm);
    }

    [Fact]
    public async Task Criar_ComProjetoDeOutroCliente_DeveLancarRegraNegocio()
    {
        _clientes.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(new Cliente { Id = 1 });
        _projetos.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(new Projeto { Id = 10, ClienteId = 2 });

        var request = new CriarChamadoRequest
        {
            Titulo = "Teste",
            Descricao = "Descrição",
            ClienteId = 1,
            ProjetoId = 10,
            Prioridade = Prioridade.Alta
        };

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(() => _service.CriarAsync(request, 1));
        Assert.Contains("não pertence", erro.Message);
        _chamados.Verify(r => r.AdicionarAsync(It.IsAny<Chamado>()), Times.Never);
    }
}
