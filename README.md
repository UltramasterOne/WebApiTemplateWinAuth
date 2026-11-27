# WebApiTemplateWinAuth

Starting point for .NET 10 ASP.NET Core Web API projects.
`ItemsController` / `ItemRepository` / `Item` are example code — replace them.

## Layout

| Folder | Purpose |
|---|---|
| `Controllers` | `[ApiController]` classes, `[Route("[controller]")]`, explicit action routes |
| `Data` | `ISqlConnectionFactory`, `DataReaderMapper` (SqlCommand → POCO), SQL health check |
| `Extensions` | `IServiceCollection` / `IApplicationBuilder` setup used by `Program.cs` |
| `Models` | DTOs / entities (`[Column]` maps a property to a differently named column) |
| `Options` | Options-pattern classes bound from configuration |
| `Repositories/<Server>` | Raw SQL per server (`App`, `Erp`, …) |
| `Services` | Business logic between controllers and repositories |

## First run

Fill in the connection strings (marked `TODO`):

| File | Used when |
|---|---|
| `WebApiTemplateWinAuth/appsettings.Development.json` → `ConnectionStrings:App` | running locally (Development) |
| `WebApiTemplateWinAuth/appsettings.json` → `ConnectionStrings:App` | production |

The app refuses to start while a required connection string is empty (`ValidateOnStart`),
and the error message says which one is missing.

Optional — keep passwords out of git instead: leave the value empty and use
`dotnet user-secrets set "ConnectionStrings:App" "..."` (dev) or an environment variable
`ConnectionStrings__App` (production). Both override appsettings.

Scalar UI (Development only): `https://localhost:7257/scalar` · Health: `/health`

Platform: Windows x64 only (`win-x64`, framework-dependent).

## New project from this template

1. Rename the solution, project folder, `.csproj` and the `WebApiTemplateWinAuth` root namespace.
2. Fill in the `TODO` connection strings. If you use user secrets, run `dotnet user-secrets init` (or put a new GUID in `<UserSecretsId>`) so projects don't share secrets.
3. Add a server: uncomment `Erp` in `SqlServer`, `DatabaseOptions`, `SqlConnectionFactory`
   and `AddSqlData` (connection string + health check), then fill `ConnectionStrings:Erp` in both appsettings files.
4. Set `Cors:Origins` for every environment where the frontend runs on a different origin.

## Settings

| Key | Default | |
|---|---|---|
| `ConnectionStrings:App` | — | required |
| `Cors:Origins` | `[]` (Dev: `http://localhost:5173`) | allowed frontend origins |
