using TimewareServiceHub.Api.Domain.Entities;
using TimewareServiceHub.Api.DTOs;

namespace TimewareServiceHub.Api.Repositories;

// Este repositório mistura duas tecnologias, de propósito:
//  - LEITURA (listagem com JOINs e filtros)  => Dapper  (SQL escrito à mão, rápido, controle total)
//  - ESCRITA (criar/alterar)                 => EF Core (change tracking, menos código)
// Esse padrão é às vezes chamado de "CQRS leve": caminhos diferentes para ler e para escrever.
public interface IChamadoRepository
{
    Task<List<ChamadoDto>> ListarAsync(ChamadoFiltro filtro);
    Task<ChamadoDto?> ObterDtoPorIdAsync(int id);
    Task<Chamado?> ObterEntidadeAsync(int id);
    Task AdicionarAsync(Chamado chamado);
    Task SalvarAlteracoesAsync();
}
