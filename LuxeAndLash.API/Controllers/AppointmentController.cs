using LuxeAndLash.Application.DTOs.Appointment;
using LuxeAndLash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LuxeAndLash.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _appointmentService.CreateAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMy()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _appointmentService.GetByUserAsync(userId);
        return Ok(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _appointmentService.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        var result = await _appointmentService.UpdateStatusAsync(id, dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _appointmentService.CancelByUserAsync(id, userId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(new { message = "Appointment cancelled successfully." });
    }
}