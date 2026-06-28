using LuxeAndLash.Application.Common;
using LuxeAndLash.Application.DTOs.Service;

namespace LuxeAndLash.Application.Interfaces;

public interface IServiceService
{
    Task<Result<List<ServiceResponseDto>>> GetAllActiveAsync();
    Task<Result<ServiceResponseDto>> GetByIdAsync(int id);
}