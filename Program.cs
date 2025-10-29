using MAP2ADAMOINT.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register ONLY the mapper service - no databases needed
builder.Services.AddScoped<DataMapperService>();

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("  MAP2ADAMOINT Middleware API");
Console.WriteLine("  Data transformation between ADAMO and MAP Tool");
Console.WriteLine("  No database connections - pure transformation layer");
Console.WriteLine("═══════════════════════════════════════════════════════");

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

