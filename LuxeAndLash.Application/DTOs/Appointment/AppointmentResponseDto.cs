namespace LuxeAndLash.Application.DTOs.Appointment;

public class AppointmentResponseDto
{
    public int Id             { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price      { get; set; }
    public DateTime DateTime  { get; set; }
    public string Status      { get; set; } = string.Empty;
    public string? Notes      { get; set; }
    public string? AdminReason { get; set; }
    public string ClientName  { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}