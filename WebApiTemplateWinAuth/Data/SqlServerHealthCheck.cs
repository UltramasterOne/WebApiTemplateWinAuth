using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApiTemplateWinAuth.Data
{
    /// <summary>
    /// Opens a connection to the given server and runs SELECT 1.
    /// Registered once per <see cref="SqlServer"/> in AddSqlData.
    /// </summary>
    internal sealed class SqlServerHealthCheck(ISqlConnectionFactory connectionFactory, SqlServer server) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = await connectionFactory.OpenAsync(server, cancellationToken);
                await using var command = new SqlCommand("SELECT 1", connection) { CommandTimeout = 5 };
                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, $"SQL server '{server}' is not reachable.", ex);
            }
        }
    }
}
