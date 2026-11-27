using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApiTemplateWinAuth.Extensions
{
    public static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonSerialization(this IServiceCollection services)
        {
            static void Configure(JsonSerializerOptions o)
            {
                o.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                // o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // optional
            }

            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => Configure(o.JsonSerializerOptions)); // controllers
            services.ConfigureHttpJsonOptions(o => Configure(o.SerializerOptions));                           // minimal APIs

            return services;
        }

        public static IServiceCollection AddCompression(this IServiceCollection services)
        {
            services.AddResponseCompression(o =>
            {
                o.EnableForHttps = true;
                o.Providers.Add<BrotliCompressionProvider>(); // first = preferred when client accepts both
                o.Providers.Add<GzipCompressionProvider>();   // fallback
            });

            services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
            services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

            return services;
        }
    }
}
