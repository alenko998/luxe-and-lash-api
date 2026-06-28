using LuxeAndLash.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace LuxeAndLash.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public UserRole Role    { get; set; } = UserRole.Client;
    public bool IsVerified  { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}