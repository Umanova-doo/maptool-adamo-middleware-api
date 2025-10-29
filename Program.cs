using MAP2ADAMOINT.Data;
using MAP2ADAMOINT.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Feature flags from configuration
var enableWrites = builder.Configuration.GetValue<bool>("DatabaseFeatures:EnableDatabaseWrites") 
    || Environment.GetEnvironmentVariable("ENABLE_DATABASE_WRITES") == "true";
var enableMigration = builder.Configuration.GetValue<bool>("DatabaseFeatures:EnableMigration")
    || Environment.GetEnvironmentVariable("ENABLE_MIGRATION") == "true";

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("  MAP2ADAMOINT Middleware API");
Console.WriteLine("  Data transformation between ADAMO and MAP Tool");
Console.WriteLine("═══════════════════════════════════════════════════════");

// Configure database contexts - reads from appsettings.json ConnectionStrings section
var mapToolConnStr = builder.Configuration.GetConnectionString("MapToolDb");
var adamoConnStr = builder.Configuration.GetConnectionString("AdamoDb");

// Configure PostgreSQL for MAP Tool
if (!string.IsNullOrEmpty(mapToolConnStr) && mapToolConnStr != "CONFIGURE_ME")
{
    try
    {
        builder.Services.AddDbContext<MapToolContext>(options =>
            options.UseNpgsql(mapToolConnStr));
        Console.WriteLine($"  ✓ PostgreSQL configured: {ExtractHost(mapToolConnStr)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ PostgreSQL configuration error: {ex.Message}");
    }
}
else
{
    Console.WriteLine("  ⚠ PostgreSQL not configured (set in appsettings.json)");
}

// Configure Oracle for ADAMO
if (!string.IsNullOrEmpty(adamoConnStr) && adamoConnStr != "CONFIGURE_ME")
{
    try
    {
        builder.Services.AddDbContext<AdamoContext>(options =>
            options.UseOracle(adamoConnStr));
        Console.WriteLine($"  ✓ Oracle configured: {ExtractDataSource(adamoConnStr)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Oracle configuration error: {ex.Message}");
    }
}
else
{
    Console.WriteLine("  ⚠ Oracle not configured (set in appsettings.json)");
}

// Helper to extract host from connection string for display
static string ExtractHost(string connStr)
{
    var hostPart = connStr.Split(';').FirstOrDefault(p => p.StartsWith("Host=", StringComparison.OrdinalIgnoreCase));
    return hostPart?.Split('=')[1] ?? "configured";
}

static string ExtractDataSource(string connStr)
{
    var dsPart = connStr.Split(';').FirstOrDefault(p => p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase));
    return dsPart?.Split('=')[1] ?? "configured";
}

Console.WriteLine($"  Database Writes: {(enableWrites ? "ENABLED ✓" : "DISABLED (transformation only)")}");
Console.WriteLine($"  Migration: {(enableMigration ? "ENABLED ✓" : "DISABLED")}");
Console.WriteLine("═══════════════════════════════════════════════════════");

// Register application services
builder.Services.AddScoped<DataMapperService>();
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<MigrationService>();

// Store feature flags in DI for controllers
builder.Services.AddSingleton(new FeatureFlags
{
    EnableDatabaseWrites = enableWrites,
    EnableMigration = enableMigration
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "MAP2ADAMOINT API", 
        Version = "v1",
        Description = "Integration API between MAP Tool (PostgreSQL) and ADAMO (Oracle)"
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("MAP2ADAMOINT API starting on port 8085");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();

