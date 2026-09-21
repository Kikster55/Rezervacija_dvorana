using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Api.Dtos;
using ResourceBooking.Api.Entities;
using ResourceBooking.Api.Extensions;
using ResourceBooking.Api.Services;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/resources")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly ResourceService _resourceService;

    public ResourcesController(ResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    /// <summary>Popis resursa. Obični korisnici vide samo aktivne, admin i neaktivne.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] ResourceType? type)
    {
        return Ok(await _resourceService.GetAllAsync(search, type, includeInactive: User.IsAdmin()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResourceDto>> GetById(int id)
    {
        return Ok(await _resourceService.GetByIdAsync(id));
    }

    /// <summary>Termini resursa za odabrani datum. Bez datuma se uzima današnji dan.</summary>
    [HttpGet("{id:int}/availability")]
    public async Task<ActionResult<ResourceAvailabilityDto>> GetAvailability(
        int id,
        [FromQuery] DateOnly? date)
    {
        var day = date ?? DateOnly.FromDateTime(DateTime.Now);
        return Ok(await _resourceService.GetAvailabilityAsync(id, day));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ResourceDto>> Create(ResourceUpsertRequest request)
    {
        var created = await _resourceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ResourceDto>> Update(int id, ResourceUpsertRequest request)
    {
        return Ok(await _resourceService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult> Delete(int id)
    {
        var message = await _resourceService.DeleteAsync(id);
        return Ok(new { message });
    }
}
