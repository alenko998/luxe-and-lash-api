namespace LuxeAndLash.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string firstName, string userId, string token);
    Task SendAppointmentReceivedAsync(string email, string firstName, string serviceName, DateTime dateTime);
    Task SendAppointmentConfirmedAsync(string email, string firstName, string serviceName, DateTime dateTime);
    Task SendAppointmentRejectedAsync(string email, string firstName, string serviceName, string reason);
    Task SendAppointmentCancelledByAdminAsync(string email, string firstName, string serviceName, string reason);
    Task SendAppointmentCancelledByUserAsync(string adminEmail, string clientName, string serviceName, DateTime dateTime);
    Task SendPasswordResetEmailAsync(string email, string firstName, string userId, string token);
    Task SendReminderEmailAsync(string email, string firstName, string serviceName, DateTime dateTime);
}