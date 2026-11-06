using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Api.DTOs;
using ServiceHub.Api.Services;

namespace ServiceHub.Api.Controllers;

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
