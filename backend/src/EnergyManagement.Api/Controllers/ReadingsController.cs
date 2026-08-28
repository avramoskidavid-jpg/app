using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyManagement.Api.Controllers;

[ApiController]
[Route("api/readings")]
[Authorize]
public class ReadingsController : ControllerBase
{
    private readonly IReadingService _readingService;

    public ReadingsController(IReadingService readingService)
    {
        _readingService = readingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReadingDto>>> GetByMeter([FromQuery] int meterId)
    {
        return Ok(await _readingService.GetByMeterAsync(meterId));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ReadingDto>> Create(ReadingCreateDto dto)
    {
        var created = await _readingService.CreateAsync(dto);
        return created is null ? BadRequest(new { message = "Meter not found." }) : Ok(created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ReadingDto>> Update(int id, ReadingUpdateDto dto)
    {
        var updated = await _readingService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _readingService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
