using ServiceHub.Api.DTOs;
using ServiceHub.Api.Repositories;

namespace ServiceHub.Api.Services;

public interface IDashboardService
{
    Task<DashboardDto> ObterAsync();
}

// Service "fino": hoje só repassa. Mantemos a camada para o controller não depender do repositório
// e para ter onde colocar regras futuras (ex.: cache, filtro por período, permissão por perfil).
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public Task<DashboardDto> ObterAsync() => _repository.ObterAsync();
}
