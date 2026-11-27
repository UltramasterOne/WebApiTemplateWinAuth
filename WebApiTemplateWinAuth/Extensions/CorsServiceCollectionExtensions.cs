using WebApiTemplateWinAuth.Options;

namespace WebApiTemplateWinAuth.Extensions
{
    public static class CorsServiceCollectionExtensions
    {
        public const string FrontendPolicy = "Frontend";

        public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new();

            services.AddCors(o => o.AddPolicy(FrontendPolicy, p => p
                .WithOrigins(settings.Origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));   // required for Windows auth from another origin (fetch credentials: 'include')

            return services;
        }

        public static IApplicationBuilder UseFrontendCors(this IApplicationBuilder app) =>
            app.UseCors(FrontendPolicy);
    }
}
