using System.Threading.RateLimiting;
using Hirenix.Application;
using Hirenix.Infrastructure;
using Hirenix.Infrastructure.Data;
using Hirenix.Infrastructure.Data.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Add Layers ──────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Controllers ─────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ─── CORS (for mobile dev) ───────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─── Rate Limiting ───────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login: 5 requests / 1 phút / mỗi IP
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // OTP: 3 requests / 5 phút / mỗi IP
    options.AddPolicy("OtpPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

// ─── Database Seeding ────────────────────────────────────────────────
if (args.Contains("--seed"))
{
    Console.WriteLine("\n🌱 Seed mode detected. Running database seeder...\n");
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HirenixDbContext>();
    
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    
    var seeder = new DatabaseSeeder(context);
    await seeder.SeedAllAsync();
    
    Console.WriteLine("\n✅ Seeding completed. Exiting application.\n");
    return; // Exit after seeding
}

// ─── Middleware Pipeline ─────────────────────────────────────────────
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
