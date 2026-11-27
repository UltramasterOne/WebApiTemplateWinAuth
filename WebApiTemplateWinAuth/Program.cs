using Scalar.AspNetCore;
using WebApiTemplateWinAuth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure json serialization
builder.Services.AddJsonSerialization();

// Configure json compression
builder.Services.AddCompression();

// Add Sql settings.
builder.Services.AddSqlData();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        foreach (var url in app.Urls)
        {
            Console.WriteLine($"Scalar UI available at: {url}/scalar");
        }
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseResponseCompression();

app.MapControllers();

app.Run();
