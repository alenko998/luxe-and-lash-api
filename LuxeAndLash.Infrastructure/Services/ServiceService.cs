using LuxeAndLash.Application.Common;
using LuxeAndLash.Application.DTOs.Service;
using LuxeAndLash.Application.Interfaces;
using LuxeAndLash.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LuxeAndLash.Infrastructure.Services;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _context;

    public ServiceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ServiceResponseDto>>> GetAllActiveAsync()
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Id)
            .ToListAsync();

        return Result<List<ServiceResponseDto>>.Success(
            services.Select(s => new ServiceResponseDto
            {
                Id              = s.Id,
                Name            = s.Name,
                Description     = s.Description,
                Price           = s.Price,
                DurationMinutes = s.DurationMinutes,
                IsActive        = s.IsActive
            }).ToList());
    }

    public async Task<Result<ServiceResponseDto>> GetByIdAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return Result<ServiceResponseDto>.Failure("Service not found.");

        return Result<ServiceResponseDto>.Success(new ServiceResponseDto
        {
            Id              = service.Id,
            Name            = service.Name,
            Description     = service.Description,
            Price           = service.Price,
            DurationMinutes = service.DurationMinutes,
            IsActive        = service.IsActive
        });
    }
}