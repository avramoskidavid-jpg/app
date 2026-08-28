using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyManagement.Api.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly IEnergyAnalyticsService _analytics;

    public AlertsController(IAlertService alertService, IEnergyAnalyticsService analytics)
    {
        _alertService = alertService;
        _analytics = analytics;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> GetAll(
        [FromQuery] int? buildingId,
        [FromQuery] AlertSeverity? severity,
        [FromQuery] AlertStatus? status)
    {
        return Ok(await _alertService.GetAllAsync(buildingId, severity, status));
    }

    [HttpGet("count")]
    public async Task<ActionResult<object>> GetOpenCount()
    {
        return Ok(new { count = await _alertService.GetOpenCountAsync() });
    }

    [HttpPut("{id:int}/resolve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AlertDto>> Resolve(int id)
    {
        var resolved = await _alertService.ResolveAsync(id);
        return resolved is null ? NotFound() : Ok(resolved);
    }

    /// Manually triggers an anomaly scan across all buildings, creating alerts as needed.
    /// Useful for demoing the alert pipeline without waiting for a new reading to be entered.
    [HttpPost("scan")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<object>> Scan()
    {
        var created = await _analytics.ScanAllBuildingsForAnomaliesAsync();
        return Ok(new { alertsCreated = created });
    }
}
