using Scalar.AspNetCore;
using WebApiTemplateWinAuth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Unhandled exceptions and error status codes are returned as ProblemDetails (RFC 9457)
builder.Services.AddProblemDetails();

// Configure json serialization
builder.Services.AddJsonSerialization();

// Configure response compression (Brotli / Gzip)
builder.Services.AddCompression();

// Add Cors
builder.Services.AddFrontendCors(builder.Configuration);

// Add Sql settings, connection factory, repositories and health checks.
builder.Services.AddSqlData();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        foreach (var url in app.Urls)
        {
            app.Logger.LogInformation("Scalar UI available at: {Url}/scalar", url);
        }
    });
}

app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseFrontendCors();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
