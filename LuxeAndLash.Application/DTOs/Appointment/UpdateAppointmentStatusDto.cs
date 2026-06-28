namespace LuxeAndLash.Application.DTOs.Appointment;

public class UpdateAppointmentStatusDto
{
    public string Status  { get; set; } = string.Empty;
    public string? Reason { get; set; }
}