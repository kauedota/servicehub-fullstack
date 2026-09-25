using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimewareServiceHub.Api.DTOs;
using TimewareServiceHub.Api.Services;

namespace TimewareServiceHub.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// GET /api/dashboard
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Obter() =>
        Ok(await _dashboardService.ObterAsync());
}
