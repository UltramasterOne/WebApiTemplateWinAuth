using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using WebApiTemplateWinAuth.Options;

namespace WebApiTemplateWinAuth.Data
{
    internal sealed class SqlConnectionFactory(IOptions<DatabaseOptions> options) : ISqlConnectionFactory
    {
        private readonly DatabaseOptions _options = options.Value;

        public async Task<SqlConnection> OpenAsync(SqlServer server, CancellationToken cancellationToken = default)
        {
            var connectionString = server switch
            {
                // SqlServer.Erp => _options.ErpConnectionString,
                SqlServer.App => _options.AppConnectionString,
                _ => throw new ArgumentOutOfRangeException(nameof(server), server, null)
            };

            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }
    }
}
