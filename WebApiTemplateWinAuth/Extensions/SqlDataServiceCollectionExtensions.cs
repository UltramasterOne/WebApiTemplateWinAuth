using WebApiTemplateWinAuth.Data;
using WebApiTemplateWinAuth.Options;
using WebApiTemplateWinAuth.Repositories.App;

namespace WebApiTemplateWinAuth.Extensions
{
    public static class SqlDataServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlData(this IServiceCollection services)
        {
            services.AddOptions<DatabaseOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    // options.ErpConnectionString = configuration.GetConnectionString("Erp") ?? string.Empty;
                    options.AppConnectionString = configuration.GetConnectionString("App") ?? string.Empty;
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Stateless -> singleton. Each call creates a new pooled connection.
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

            // Repositories
            services.AddScoped<ItemRepository>();

            return services;
        }
    }
}
