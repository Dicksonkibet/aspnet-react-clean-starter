using System.Text;
using System.Text.Json.Serialization;
using CleanStart.API.Auth;
using CleanStart.API.Middleware;
using CleanStart.Application;
using CleanStart.Application.Common.Interfaces;
using CleanStart.Infrastructure;
using CleanStart.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// APPLICATION / INFRASTRUCTURE
// ============================================================
// Each layer registers itself in one line — Program.cs never needs to know the
// internal wiring details of Application or Infrastructure.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<JwtTokenGenerator>();

// ============================================================
// CONTROLLERS
// ============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// CORS
// ============================================================
// Locked to an explicit allow-list in every environment except Development.
// Configure via "Cors:AllowedOrigins" in appsettings/env vars
// (e.g. Cors__AllowedOrigins__0=https://yourapp.com). Development stays wide open so
// local Vite dev servers on arbitrary ports don't need to be enumerated one by one.
const string CorsPolicy = "Default";
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            return;
        }

        policy.WithOrigins(configuredOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================================
// AUTHENTICATION
// ============================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    var jwtSection = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ============================================================
// SWAGGER
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ============================================================
// MIDDLEWARE
// ============================================================
app.UseGlobalExceptionHandling();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// ============================================================
// HEALTH CHECK
// ============================================================
// A simple, cheap endpoint most hosts (Render, Railway, Fly.io) can point their
// health check at. Verifies real DB connectivity, not just "the process is running".
app.MapGet("/healthz", async (AppDbContext db, CancellationToken ct) =>
{
    var checkedAtUtc = DateTime.UtcNow;
    try
    {
        var canConnect = await db.Database.CanConnectAsync(ct);
        return canConnect
            ? Results.Ok(new { status = "healthy", checkedAtUtc })
            : Results.Json(new { status = "unhealthy", reason = "database unreachable", checkedAtUtc },
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "unhealthy", errorType = ex.GetType().Name, detail = ex.Message, checkedAtUtc },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ============================================================
// DATABASE MIGRATIONS
// ============================================================
// Applies pending migrations on startup. Fine for early-stage projects; once you have
// a real release process, prefer running `dotnet ef database update` as a separate
// deploy step instead of doing it inside app startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
