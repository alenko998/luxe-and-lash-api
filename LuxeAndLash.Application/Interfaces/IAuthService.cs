using LuxeAndLash.Application.Common;
using LuxeAndLash.Application.DTOs.Auth;

namespace LuxeAndLash.Application.Interfaces;

public interface IAuthService
{
    Task<Result<string>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Result> VerifyEmailAsync(string userId, string token);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(string userId, string token, string newPassword);
}