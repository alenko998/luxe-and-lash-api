using LuxeAndLash.Domain.Enums;

namespace LuxeAndLash.Domain.Entities;

public class Appointment
{
    public int Id              { get; set; }
    public string UserId       { get; set; } = string.Empty;
    public int ServiceId       { get; set; }
    public DateTime DateTime   { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes       { get; set; }
    public string? AdminReason { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    public User User           { get; set; } = null!;
    public Service Service     { get; set; } = null!;
}