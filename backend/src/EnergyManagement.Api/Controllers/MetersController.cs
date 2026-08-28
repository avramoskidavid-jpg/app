using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyManagement.Api.Controllers;

[ApiController]
[Route("api/meters")]
[Authorize]
public class MetersController : ControllerBase
{
    private readonly IMeterService _meterService;

    public MetersController(IMeterService meterService)
    {
        _meterService = meterService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MeterDto>>> GetByBuilding([FromQuery] int buildingId)
    {
        return Ok(await _meterService.GetByBuildingAsync(buildingId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MeterDto>> GetById(int id)
    {
        var meter = await _meterService.GetByIdAsync(id);
        return meter is null ? NotFound() : Ok(meter);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<MeterDto>> Create(MeterCreateDto dto)
    {
        var created = await _meterService.CreateAsync(dto);
        return created is null ? BadRequest(new { message = "Building not found." }) : CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<MeterDto>> Update(int id, MeterUpdateDto dto)
    {
        var updated = await _meterService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _meterService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
