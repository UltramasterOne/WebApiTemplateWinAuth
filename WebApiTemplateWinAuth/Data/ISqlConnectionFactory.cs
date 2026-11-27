using Microsoft.Data.SqlClient;

namespace WebApiTemplateWinAuth.Data
{
    /// <summary>
    /// SQL Server instances the API talks to. Each one has its own connection string.
    /// </summary>
    public enum SqlServer
    {
        // Erp,
        App
    }

    public interface ISqlConnectionFactory
    {
        /// <summary>
        /// Creates and opens a new pooled connection to the requested server.
        /// The caller owns the connection and must dispose it (await using).
        /// </summary>
        Task<SqlConnection> OpenAsync(SqlServer server, CancellationToken cancellationToken = default);
    }
}
