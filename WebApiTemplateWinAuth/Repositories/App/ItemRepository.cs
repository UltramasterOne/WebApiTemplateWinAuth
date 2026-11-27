using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using WebApiTemplateWinAuth.Data;
using WebApiTemplateWinAuth.Models;
using WebApiTemplateWinAuth.Options;

namespace WebApiTemplateWinAuth.Repositories.App
{
    public sealed class ItemRepository(ISqlConnectionFactory db, IOptions<DatabaseOptions> options)
    {
        private readonly int _commandTimeout = options.Value.CommandTimeoutSeconds;

        public async Task<List<Item>> GetAllAsync(CancellationToken ct)
        {
            await using var command = new SqlCommand(QUERY_INDEX) { CommandTimeout = _commandTimeout };

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
