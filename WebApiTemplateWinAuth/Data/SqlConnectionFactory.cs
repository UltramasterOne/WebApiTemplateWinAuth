using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using WebApiTemplateWinAuth.Options;

namespace WebApiTemplateWinAuth.Data
{
    internal sealed class SqlConnectionFactory(IOptions<DatabaseOptions> options) : ISqlConnectionFactory
    {
        private readonly DatabaseOptions _options = options.Value;

        public async Task<SqlConnection> OpenAsync(SqlServer database, CancellationToken cancellationToken = default)
        {
            var connectionString = database switch
            {
                // SqlDatabase.Erp => _options.ErpConnectionString,
                SqlServer.App => _options.AppConnectionString,
                _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
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