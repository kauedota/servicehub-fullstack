using ServiceHub.Api.DTOs;

namespace ServiceHub.Api.Repositories;

public interface IDashboardRepository
{
    Task<DashboardDto> ObterAsync();
}
