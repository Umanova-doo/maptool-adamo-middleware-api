using MAP2ADAMOINT.Data;
using MAP2ADAMOINT.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure PostgreSQL DbContext for MAP Tool
builder.Services.AddDbContext<MapToolContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MapToolDb")
        ?? Environment.GetEnvironmentVariable("MAPTOOL_CONNECTION_STRING")
        ?? "Host=postgres;Port=5432;Database=MAP23;Username=postgres;Password=postgresUser234";
    
    options.UseNpgsql(connectionString);
});

// Configure Oracle DbContext for ADAMO
builder.Services.AddDbContext<AdamoContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AdamoDb")
        ?? Environment.GetEnvironmentVariable("ADAMO_CONNECTION_STRING")
        ?? "User Id=system;Password=oracle;Data Source=oracle:1521/XE";
    
    options.UseOracle(connectionString);
});

// Register application services
builder.Services.AddScoped<DataMapperService>();
builder.Services.AddScoped<SyncService>();

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

