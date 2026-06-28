using LuxeAndLash.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace LuxeAndLash.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromName;

    public EmailService(IConfiguration config)
    {
        _config   = config;
        _host     = config["EmailSettings:Host"]!;
        _port     = int.Parse(config["EmailSettings:Port"]!);
        _username = config["EmailSettings:Username"]!;
        _password = config["EmailSettings:Password"]!;
        _fromName = config["EmailSettings:FromName"]!;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _username));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_username, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendVerificationEmailAsync(string email, string firstName, string userId, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"http://localhost:5000/api/auth/verify-email?userId={userId}&token={encodedToken}";
        var html = $@"
            <h2>Welcome to Luxe & Lash, {firstName}!</h2>
            <p>Please verify your email address by clicking the button below:</p>
            <a href='{link}' style='background:#B8826A;color:white;padding:12px 24px;text-decoration:none;border-radius:2px;display:inline-block;'>
                Verify Email
            </a>
            <p>This link expires in 24 hours.</p>";
        await SendAsync(email, firstName, "Verify your email — Luxe & Lash", html);
    }

    public async Task SendAppointmentReceivedAsync(string email, string firstName, string serviceName, DateTime dateTime)
    {
        var html = $@"
            <h2>Hi {firstName}, we received your request!</h2>
            <p><strong>Service:</strong> {serviceName}</p>
            <p><strong>Requested time:</strong> {dateTime:dddd, MMMM d yyyy 'at' h:mm tt}</p>
            <p>We'll confirm your appointment shortly.</p>";
        await SendAsync(email, firstName, "Appointment request received — Luxe & Lash", html);
    }

    public async Task SendAppointmentConfirmedAsync(string email, string firstName, string serviceName, DateTime dateTime)
    {
        var html = $@"
            <h2>Your appointment is confirmed! 💅</h2>
            <p><strong>Service:</strong> {serviceName}</p>
            <p><strong>Date & Time:</strong> {dateTime:dddd, MMMM d yyyy 'at' h:mm tt}</p>
            <p>We look forward to seeing you!</p>";
        await SendAsync(email, firstName, "Appointment confirmed — Luxe & Lash", html);
    }

    public async Task SendAppointmentRejectedAsync(string email, string firstName, string serviceName, string reason)
    {
        var html = $@"
            <h2>Hi {firstName}, about your appointment request</h2>
            <p>Unfortunately we're unable to confirm your <strong>{serviceName}</strong> appointment.</p>
            <p><strong>Reason:</strong> {reason}</p>
            <p>Please book a new time that works for you.</p>";
        await SendAsync(email, firstName, "Appointment update — Luxe & Lash", html);
    }

    public async Task SendAppointmentCancelledByAdminAsync(string email, string firstName, string serviceName, string reason)
    {
        var html = $@"
            <h2>Hi {firstName}, your appointment has been cancelled</h2>
            <p><strong>Service:</strong> {serviceName}</p>
            <p><strong>Reason:</strong> {reason}</p>
            <p>We're sorry for the inconvenience. Please feel free to book again.</p>";
        await SendAsync(email, firstName, "Appointment cancelled — Luxe & Lash", html);
    }

    public async Task SendAppointmentCancelledByUserAsync(string adminEmail, string clientName, string serviceName, DateTime dateTime)
    {
        var html = $@"
            <h2>Appointment cancelled by client</h2>
            <p><strong>Client:</strong> {clientName}</p>
            <p><strong>Service:</strong> {serviceName}</p>
            <p><strong>Was scheduled for:</strong> {dateTime:dddd, MMMM d yyyy 'at' h:mm tt}</p>";
        await SendAsync(adminEmail, "Admin", "Client cancelled appointment — Luxe & Lash", html);
    }

    public async Task SendPasswordResetEmailAsync(string email, string firstName, string userId, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"http://localhost:3000/reset-password?userId={userId}&token={encodedToken}";
        var html = $@"
            <h2>Hi {firstName}, reset your password</h2>
            <p>Click the button below to reset your password:</p>
            <a href='{link}' style='background:#B8826A;color:white;padding:12px 24px;text-decoration:none;border-radius:2px;display:inline-block;'>
                Reset Password
            </a>
            <p>This link expires in 1 hour. If you didn't request this, ignore this email.</p>";
        await SendAsync(email, firstName, "Reset your password — Luxe & Lash", html);
    }

    public async Task SendReminderEmailAsync(string email, string firstName, string serviceName, DateTime dateTime)
    {
        var html = $@"
            <h2>Reminder: your appointment is tomorrow! 💅</h2>
            <p><strong>Service:</strong> {serviceName}</p>
            <p><strong>Date & Time:</strong> {dateTime:dddd, MMMM d yyyy 'at' h:mm tt}</p>
            <p>See you soon!</p>";
        await SendAsync(email, firstName, "Appointment reminder — Luxe & Lash", html);
    }
}