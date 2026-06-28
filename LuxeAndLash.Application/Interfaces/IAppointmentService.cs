using LuxeAndLash.Application.Common;
using LuxeAndLash.Application.DTOs.Appointment;

namespace LuxeAndLash.Application.Interfaces;

public interface IAppointmentService
{
    Task<Result<AppointmentResponseDto>> CreateAsync(string userId, CreateAppointmentDto dto);
    Task<Result<List<AppointmentResponseDto>>> GetByUserAsync(string userId);
    Task<Result<List<AppointmentResponseDto>>> GetAllAsync();
    Task<Result<AppointmentResponseDto>> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<Result> CancelByUserAsync(int id, string userId);
}