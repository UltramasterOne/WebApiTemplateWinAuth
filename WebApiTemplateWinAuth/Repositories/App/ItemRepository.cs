using Microsoft.Data.SqlClient;
using WebApiTemplateWinAuth.Data;
using WebApiTemplateWinAuth.Models;

namespace WebApiTemplateWinAuth.Repositories.App
{
    public sealed class ItemRepository(ISqlConnectionFactory db)
    {
        public async Task<List<Item>> GetAllAsync(CancellationToken ct)
        {
            // Default CommandTimeout is 30 s. For a slow query set it here, e.g.:
            // new SqlCommand(QUERY_INDEX) { CommandTimeout = 120 }
            await using var command = new SqlCommand(QUERY_INDEX);

            return await command.GetDataAsync<Item>(db, SqlServer.App, cancellationToken: ct);
        }

        private const string QUERY_INDEX = @"
SELECT TOP 100
       [No_],
       [Description]
FROM dbo.[Item]
ORDER BY [No_] ASC
";
    }
}
