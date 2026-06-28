using LuxeAndLash.Application.Common;
using LuxeAndLash.Application.DTOs.Auth;
using LuxeAndLash.Application.Interfaces;
using LuxeAndLash.Domain.Entities;
using LuxeAndLash.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LuxeAndLash.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthService(UserManager<User> userManager, IEmailService emailService, IConfiguration config)
    {
        _userManager  = userManager;
        _emailService = emailService;
        _config       = config;
    }

    public async Task<Result<string>> RegisterAsync(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return Result<string>.Failure("Passwords do not match.");

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return Result<string>.Failure("Email is already in use.");

        var user = new User
        {
            FirstName  = dto.FirstName,
            LastName   = dto.LastName,
            Email      = dto.Email,
            UserName   = dto.Email,
            Role       = UserRole.Client,
            IsVerified = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        await _userManager.AddToRoleAsync(user, "Client");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendVerificationEmailAsync(user.Email!, user.FirstName, user.Id, token);

        return Result<string>.Success("Registration successful. Please check your email to verify your account.");
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        if (!user.IsVerified)
            return Result<AuthResponseDto>.Failure("Please verify your email before logging in.");

        var token = GenerateJwtToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token     = token,
            Email     = user.Email!,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Role      = user.Role.ToString()
        });
    }

    public async Task<Result> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure("User not found.");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return Result.Failure("Invalid or expired verification link.");

        user.IsVerified = true;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Success(); // ne otkrivamo da li email postoji

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FirstName, user.Id, token);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return Result.Failure(result.Errors.First().Description);

        return Result.Success();
    }

    private string GenerateJwtToken(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:ExpiryDays"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer:   _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims:   claims,
            expires:  expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}