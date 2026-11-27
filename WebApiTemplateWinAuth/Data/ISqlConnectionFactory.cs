using Microsoft.Data.SqlClient;

namespace WebApiTemplateWinAuth.Data
{
    public enum SqlServer
    {
        // Erp,
        App
    }

    /// <summary>
    /// Creates and opens a new pooled connection to the requested database.
    /// The caller owns the connection and must dispose it (await using).
    /// </summary>
    public interface ISqlConnectionFactory
    {
        Task<SqlConnection> OpenAsync(SqlServer database, CancellationToken cancellationToken = default);
    }
}