using LuxeAndLash.Application.DTOs.Auth;
using LuxeAndLash.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LuxeAndLash.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        return Ok(new { message = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _authService.VerifyEmailAsync(userId, token);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        return Ok(new { message = "Email verified successfully. You can now log in." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(new { message = "If this email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto.UserId, dto.Token, dto.NewPassword);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        return Ok(new { message = "Password reset successfully." });
    }
}