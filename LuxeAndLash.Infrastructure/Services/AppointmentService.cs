using LuxeAndLash.Application.Common;
using LuxeAndLash.Application.DTOs.Appointment;
using LuxeAndLash.Application.Interfaces;
using LuxeAndLash.Domain.Entities;
using LuxeAndLash.Domain.Enums;
using LuxeAndLash.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LuxeAndLash.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AppointmentService(AppDbContext context, IEmailService emailService)
    {
        _context      = context;
        _emailService = emailService;
    }

    public async Task<Result<AppointmentResponseDto>> CreateAsync(string userId, CreateAppointmentDto dto)
    {
        var service = await _context.Services.FindAsync(dto.ServiceId);
        if (service == null)
            return Result<AppointmentResponseDto>.Failure("Service not found.");

        var conflict = await _context.Appointments
            .AnyAsync(a => a.DateTime == dto.DateTime &&
                           a.Status != AppointmentStatus.Rejected &&
                           a.Status != AppointmentStatus.Cancelled);
        if (conflict)
            return Result<AppointmentResponseDto>.Failure("This time slot is already taken.");

        var appointment = new Appointment
        {
            UserId    = userId,
            ServiceId = dto.ServiceId,
            DateTime  = dto.DateTime,
            Notes     = dto.Notes,
            Status    = AppointmentStatus.Pending
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        await _emailService.SendAppointmentReceivedAsync(
            user!.Email!, user.FirstName, service.Name, dto.DateTime);

        return Result<AppointmentResponseDto>.Success(MapToDto(appointment, service, user));
    }

    public async Task<Result<List<AppointmentResponseDto>>> GetByUserAsync(string userId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateTime)
            .ToListAsync();

        return Result<List<AppointmentResponseDto>>.Success(
            appointments.Select(a => MapToDto(a, a.Service, a.User)).ToList());
    }

    public async Task<Result<List<AppointmentResponseDto>>> GetAllAsync()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.User)
            .OrderByDescending(a => a.DateTime)
            .ToListAsync();

        return Result<List<AppointmentResponseDto>>.Success(
            appointments.Select(a => MapToDto(a, a.Service, a.User)).ToList());
    }

    public async Task<Result<AppointmentResponseDto>> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return Result<AppointmentResponseDto>.Failure("Appointment not found.");

        if (!Enum.TryParse<AppointmentStatus>(dto.Status, true, out var newStatus))
            return Result<AppointmentResponseDto>.Failure("Invalid status.");

        appointment.Status      = newStatus;
        appointment.AdminReason = dto.Reason;
        await _context.SaveChangesAsync();

        var email     = appointment.User.Email!;
        var firstName = appointment.User.FirstName;
        var service   = appointment.Service.Name;
        var dateTime  = appointment.DateTime;

        switch (newStatus)
        {
            case AppointmentStatus.Confirmed:
                await _emailService.SendAppointmentConfirmedAsync(email, firstName, service, dateTime);
                break;
            case AppointmentStatus.Rejected:
                await _emailService.SendAppointmentRejectedAsync(email, firstName, service, dto.Reason ?? "");
                break;
            case AppointmentStatus.Cancelled:
                await _emailService.SendAppointmentCancelledByAdminAsync(email, firstName, service, dto.Reason ?? "");
                break;
        }

        return Result<AppointmentResponseDto>.Success(
            MapToDto(appointment, appointment.Service, appointment.User));
    }

    public async Task<Result> CancelByUserAsync(int id, string userId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (appointment == null)
            return Result.Failure("Appointment not found.");

        if (appointment.Status == AppointmentStatus.Cancelled ||
            appointment.Status == AppointmentStatus.Rejected)
            return Result.Failure("Appointment cannot be cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        var adminEmail  = "admin@gmail.com";
        var clientName  = $"{appointment.User.FirstName} {appointment.User.LastName}";
        var serviceName = appointment.Service.Name;

        await _emailService.SendAppointmentCancelledByUserAsync(
            adminEmail, clientName, serviceName, appointment.DateTime);

        return Result.Success();
    }

    private static AppointmentResponseDto MapToDto(Appointment a, Domain.Entities.Service s, User u) => new()
    {
        Id           = a.Id,
        ServiceName  = s.Name,
        Price        = s.Price,
        DateTime     = a.DateTime,
        Status       = a.Status.ToString(),
        Notes        = a.Notes,
        AdminReason  = a.AdminReason,
        ClientName   = $"{u.FirstName} {u.LastName}",
        ClientEmail  = u.Email!,
        CreatedAt    = a.CreatedAt
    };
}