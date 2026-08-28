using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyManagement.Api.Controllers;

[ApiController]
[Route("api/buildings")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;
    private readonly IEnergyAnalyticsService _analytics;

    public BuildingsController(IBuildingService buildingService, IEnergyAnalyticsService analytics)
    {
        _buildingService = buildingService;
        _analytics = analytics;
    }

    [HttpGet]
    public async Task<ActionResult<List<BuildingDto>>> GetAll()
    {
        return Ok(await _buildingService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BuildingDetailsDto>> GetById(int id)
    {
        var details = await _buildingService.GetDetailsAsync(id);
        return details is null ? NotFound() : Ok(details);
    }

    [HttpGet("{id:int}/energy-target")]
    public async Task<ActionResult<EnergyTargetResult>> GetEnergyTarget(int id)
    {
        return Ok(await _analytics.CalculateEnergyTargetAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BuildingDto>> Create(BuildingCreateDto dto)
    {
        var created = await _buildingService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BuildingDto>> Update(int id, BuildingUpdateDto dto)
    {
        var updated = await _buildingService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _buildingService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
