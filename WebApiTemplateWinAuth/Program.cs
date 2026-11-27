using Scalar.AspNetCore;
using WebApiTemplateWinAuth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Windows auth doesn't work over HTTP/2 -> Kestrel serves HTTP/1.1 (ignored under IIS)
builder.WebHost.UseHttp1ForWindowsAuth();

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

// Windows Authentication; every endpoint requires a signed-in user unless marked anonymous
builder.Services.AddWindowsAuth();

// Add Sql settings, connection factory and repositories.
builder.Services.AddSqlData();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    app.MapScalarApiReference().AllowAnonymous();

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

// CORS must run before authentication, so preflight requests and 401 responses get CORS headers
app.UseFrontendCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
