namespace LuxeAndLash.Application.DTOs.Appointment;

public class CreateAppointmentDto
{
    public int ServiceId  { get; set; }
    public DateTime DateTime { get; set; }
    public string? Notes  { get; set; }
}