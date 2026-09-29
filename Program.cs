using Pgvector.EntityFrameworkCore;
using Asp.Versioning;
using BackendEgitimiYeni.BackgroundServices;
using BackendEgitimiYeni.Configuration;
using BackendEgitimiYeni.Data;
using BackendEgitimiYeni.Middleware;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


// AI Configuration
builder.Services.Configure<AiSettings>(
    builder.Configuration.GetSection(
        AiSettings.SectionName
    )
);


if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"
            ),
            npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            }
        )
    );
}


// Application Services
builder.Services.AddScoped<
    IProductService,
    ProductService
>();

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();

builder.Services.AddScoped<
    IQueryRewritingService,
    QueryRewritingService
>();

builder.Services.AddScoped<
    IAiToolExecutor,
    AiToolExecutor
>();

builder.Services.AddScoped<
    IAiToolService,
    AiToolService
>();

builder.Services.AddScoped<
    IRagEvaluationService,
    RagEvaluationService
>();

builder.Services.AddScoped<
    IContextManagementService,
    ContextManagementService
>();

builder.Services.AddScoped<
    IRagSecurityService,
    RagSecurityService
>();

builder.Services.AddScoped<
    IPdfTextExtractorService,
    PdfTextExtractorService
>();

builder.Services.AddScoped<
    ITextChunkingService,
    TextChunkingService
>();

builder.Services.AddScoped<
    IRagService,
    RagService
>();

builder.Services.AddScoped<
    IDocumentService,
    DocumentService
>();

builder.Services.AddScoped<
    ISemanticSearchService,
    SemanticSearchService
>();


// AI Service
builder.Services.AddHttpClient<
    IAiService,
    OllamaAiService
>(
    (serviceProvider, client) =>
    {
        var settings =
            serviceProvider
                .GetRequiredService<
                    IOptions<AiSettings>
                >()
                .Value;

        client.BaseAddress =
            new Uri(settings.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(
                settings.TimeoutSeconds
            );
    }
);


builder.Services.AddHttpClient<
    IEmbeddingService,
    OllamaEmbeddingService
>(
    (serviceProvider, client) =>
    {
        var settings =
            serviceProvider
                .GetRequiredService<
                    IOptions<AiSettings>
                >()
                .Value;

        client.BaseAddress =
            new Uri(settings.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(
                settings.TimeoutSeconds
            );
    }
);


// Background Services
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<
        SystemMonitorService
    >();
}


// Memory Cache
builder.Services.AddMemoryCache();


var jwtKey =
    builder.Environment.IsEnvironment("Testing")
        ? "BackendEgitimiYeni-Integration-Test-Key-2026-123456789"
        : builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT Key bulunamadı."
            );

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "BackendEgitimiYeni";

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "BackendEgitimiYeniUsers";


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
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
                        Encoding.UTF8.GetBytes(
                            jwtKey
                        )
                    )
            };
    });


builder.Services.AddAuthorization();


// API Versioning
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(1, 0);

        options.AssumeDefaultVersionWhenUnspecified =
            false;

        options.ReportApiVersions = true;

        options.ApiVersionReader =
            new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl =
            true;
    })
    .AddOpenApi();


// CORS
var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendPolicy",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});


// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(
        "FixedPolicy",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 20;

            limiterOptions.Window =
                TimeSpan.FromSeconds(10);

            limiterOptions.QueueLimit = 0;

            limiterOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        }
    );
});


// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Backend Eğitimi API",
            Version = "v1",
            Description =
                "ASP.NET Core backend eğitim projesi."
        }
    );

    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "JWT Authorization header using the Bearer scheme."
        }
    );

    options.AddSecurityRequirement(
        document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "bearer",
                        document
                    )
                ] = []
            }
    );
});


builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();


var app = builder.Build();


if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope =
        app.Services.CreateScope();

    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
        .WithDocumentPerVersion();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Backend Eğitimi API v1"
        );
    });
}


app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers()
    .RequireRateLimiting("FixedPolicy");

app.MapHealthChecks("/health");

app.Run();


public partial class Program
{
}