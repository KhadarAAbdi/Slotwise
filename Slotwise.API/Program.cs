using Serilog;
using Slotwise.API.ErrorHandling;
using Slotwise.API.Logging;
using Slotwise.Application;
using Slotwise.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Replace the default logger with Serilog. Levels, overrides and the console format come from the
// "Serilog" section of appsettings, and FromLogContext lets the correlation id appear on every line.
builder.Services.AddSerilog((services, config) => 
    config.ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().WriteTo.Console());

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// The correlation id comes first so the request log line written below carries it too.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
