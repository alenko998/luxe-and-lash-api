using LuxeAndLash.Application.Interfaces;
using LuxeAndLash.Domain.Entities;
using LuxeAndLash.Domain.Enums;
using LuxeAndLash.Infrastructure.Data;
using LuxeAndLash.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──
builder.Services.AddControllers();

// ── Database ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT ──
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
    };
});

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Services ──
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceService, ServiceService>();

var app = builder.Build();

// ── Seed ──
using (var scope = app.Services.CreateScope())
{
    var services    = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var config      = services.GetRequiredService<IConfiguration>();

    foreach (var role in new[] { "Admin", "Client" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = config["AdminSettings:Email"]!;
    var adminUser  = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var admin = new User
        {
            FirstName      = config["AdminSettings:FirstName"]!,
            LastName       = config["AdminSettings:LastName"]!,
            Email          = adminEmail,
            UserName       = adminEmail,
            Role           = UserRole.Admin,
            IsVerified     = true,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, config["AdminSettings:Password"]!);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();