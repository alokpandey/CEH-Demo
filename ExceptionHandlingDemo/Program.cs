/*
 * File: Program.cs
 * Project: Exception Handling Demo
 * Created: May 2024
 * Author: Alok Pandey
 *
 * Description:
 * Main entry point for the ASP.NET Core application.
 * Configures services, middleware, and request pipeline.
 * Registers exception handling middleware and related services.
 */

using ExceptionHandlingDemo.Middleware;
using ExceptionHandlingDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Register services
builder.Services.AddSingleton<RetryPolicyService>();
builder.Services.AddSingleton<MetricsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Add our custom exception handling middleware
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
