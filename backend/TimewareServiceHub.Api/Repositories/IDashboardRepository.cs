using TimewareServiceHub.Api.DTOs;

namespace TimewareServiceHub.Api.Repositories;

public interface IDashboardRepository
{
    Task<DashboardDto> ObterAsync();
}
