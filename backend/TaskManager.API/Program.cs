using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskManager.API;
using TaskManager.API.Middleware;
using TaskManager.API.Services;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// CORS - Allow frontend from environment or default to localhost for development
var frontendUrl = builder.Configuration["FrontendUrl"] 
               ?? Environment.GetEnvironmentVariable("FrontendUrl")
               ?? "http://localhost:4200";

var allowedOrigins = frontendUrl.Split(';', StringSplitOptions.RemoveEmptyEntries)
    .Select(url => url.Trim())
    .Where(url => !string.IsNullOrEmpty(url))
    .ToList();

// Always allow localhost for development
if (!allowedOrigins.Contains("http://localhost:4200"))
{
    allowedOrigins.Add("http://localhost:4200");
}

// Log allowed origins for debugging
Log.Information("CORS allowed origins: {Origins}", string.Join(", ", allowedOrigins));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// DbContext - Handle Railway's PostgreSQL URL format
var connectionString = configuration.GetConnectionString("DefaultConnection");

// If connection string is empty, try to build from individual Railway PostgreSQL variables
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("${"))
{
    // Check for DATABASE_URL first (Railway provides this when services are linked)
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                    ?? Environment.GetEnvironmentVariable("POSTGRES_URL");
    
    // If still empty or contains template variables, build from individual PG* variables (Railway provides these)
    if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("${"))
    {
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
        
        if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase) && 
            !string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
        {
            connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword}";
            Log.Information("Built connection string from PG* environment variables.");
        }
    }
}

// Convert PostgreSQL URL format (postgresql://user:pass@host:port/db) to Npgsql format
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var uri = new Uri(connectionString);
        var host = uri.Host;
        var dbPort = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = uri.UserInfo.Split(':')[0];
        var password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : "";
        
        // Build Npgsql connection string
        connectionString = $"Host={host};Port={dbPort};Database={database};Username={username};Password={Uri.UnescapeDataString(password)}";
        
        // Update configuration
        configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        Log.Information("Converted PostgreSQL URL format to Npgsql format.");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to parse PostgreSQL URL format, using as-is: {ConnectionString}", connectionString);
    }
}

// Validate connection string before using it
if (string.IsNullOrWhiteSpace(connectionString))
{
    Log.Error("Connection string is empty or null. Available env vars - PGHOST: {PGHOST}, PGDATABASE: {PGDATABASE}, PGUSER: {PGUSER}, DATABASE_URL: {DATABASE_URL}",
        Environment.GetEnvironmentVariable("PGHOST"),
        Environment.GetEnvironmentVariable("PGDATABASE"),
        Environment.GetEnvironmentVariable("PGUSER"),
        Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "SET" : "NOT SET");
    throw new InvalidOperationException("Database connection string is not configured. Please link PostgreSQL service or set ConnectionStrings__DefaultConnection environment variable.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Auth configuration
builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
builder.Services.Configure<AuthSettings>(configuration.GetSection("Auth"));

// Authentication
var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization
builder.Services.AddAuthorization();

// DI registrations
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpContextAccessor();

// Configure port: use PORT env (e.g. Railway), or 5001 in Development, else 8080
var port = Environment.GetEnvironmentVariable("PORT")
    ?? (builder.Environment.IsDevelopment() ? "5001" : "8080");
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Apply schema and seed data with retry logic (non-blocking)
// IMPORTANT: only run for local/dev environments to avoid impacting production.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    _ = Task.Run(async () =>
    {
        await Task.Delay(TimeSpan.FromSeconds(2)); // Give app time to start

        // Prefer the same connection string the app is already using (docker-compose sets ConnectionStrings__DefaultConnection).
        var dbConnectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("POSTGRES_URL");

        if (string.IsNullOrEmpty(dbConnectionString))
        {
            var pgHost = Environment.GetEnvironmentVariable("PGHOST");
            var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
            var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
            var pgUser = Environment.GetEnvironmentVariable("PGUSER");
            var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

            if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase) &&
                !string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
            {
                dbConnectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword}";
            }
        }

        // Convert PostgreSQL URL format if needed
        if (!string.IsNullOrEmpty(dbConnectionString) &&
            dbConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(dbConnectionString);
                var host = uri.Host;
                var dbPort = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.AbsolutePath.TrimStart('/');
                var username = uri.UserInfo.Split(':')[0];
                var password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : "";
                dbConnectionString = $"Host={host};Port={dbPort};Database={database};Username={username};Password={Uri.UnescapeDataString(password)}";
            }
            catch
            {
                // best-effort conversion; use as-is if parsing fails
            }
        }

        if (string.IsNullOrEmpty(dbConnectionString))
        {
            Log.Warning("Database connection string not available for local init/seed.");
            return;
        }

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Create new DbContext with correct connection string
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(dbConnectionString);
        await using var dbContext = new ApplicationDbContext(optionsBuilder.Options);

        // Retry logic for database connection
        const int maxRetries = 10;
        var delay = TimeSpan.FromSeconds(3);
        var retryCount = 0;
        var success = false;

        while (retryCount < maxRetries && !success)
        {
            try
            {
                // Use migrations so tables like `users` are created consistently.
                await dbContext.Database.MigrateAsync();
                await SeedData.EnsureSeedDataAsync(dbContext, configuration);
                success = true;
                logger.LogInformation("Database migrated and seeded successfully.");
            }
            catch (Exception ex) when (retryCount < maxRetries - 1)
            {
                retryCount++;
                logger.LogWarning(
                    "Database init attempt {RetryCount}/{MaxRetries} failed. Retrying in {Delay}s... Error: {Error}",
                    retryCount, maxRetries, delay.TotalSeconds, ex.Message);
                await Task.Delay(delay);
            }
        }

        if (!success)
        {
            logger.LogError(
                "Failed to migrate/seed database after {MaxRetries} attempts. Application will continue but DB operations may fail.",
                maxRetries);
        }
    });
}

// Middleware pipeline - CORS must be early in pipeline
app.UseCors("AllowFrontend");

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// In Development we typically run HTTP-only (no local dev certificate),
// so only enforce HTTPS redirection outside Development.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker" || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

public partial class Program { }

