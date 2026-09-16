using BackendEgitimiYeni.Data;
using BackendEgitimiYeni.Middleware;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Normal uygulamada PostgreSQL kullan.
// Integration test sırasında PostgreSQL yerine
// test tarafındaki InMemory database kullanılacak.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")
        ));
}

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Testing ortamında gerçek secret kullanmıyoruz.
var jwtKey = builder.Environment.IsEnvironment("Testing")
    ? "BackendEgitimiYeni-Integration-Test-Key-2026-123456789"
    : builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT Key bulunamadı.");

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "BackendEgitimiYeni";

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "BackendEgitimiYeniUsers";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    )
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}