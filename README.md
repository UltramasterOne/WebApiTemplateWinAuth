# WebApiTemplateWinAuth

Starting point for .NET 10 ASP.NET Core Web API projects **with Windows Authentication**
(intranet, domain users). Same layout as `WebApiTemplate`.
`ItemsController` / `ItemRepository` / `Item` are example code — replace them.

## Layout

| Folder | Purpose |
|---|---|
| `Controllers` | `[ApiController]` classes, `[Route("[controller]")]`, explicit action routes |
| `Data` | `ISqlConnectionFactory`, `DataReaderMapper` (SqlCommand → POCO) |
| `Extensions` | `IServiceCollection` / `IApplicationBuilder` setup used by `Program.cs` |
| `Models` | DTOs / entities (`[Column]` maps a property to a differently named column) |
| `Options` | Options-pattern classes bound from configuration |
| `Repositories/<Server>` | Raw SQL per server (`App`, `Erp`, …) |
| `Services` | Business logic between controllers and repositories (`UserDirectoryService`: AD display name lookup) |

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

Scalar UI (Development only): `https://localhost:7257/scalar` (anonymous)

Platform: Windows x64 only (`win-x64`, framework-dependent).

## Windows Authentication

- Every endpoint requires a signed-in Windows user (fallback authorization policy).
  Opt out with `[AllowAnonymous]` on a controller/action, or `.AllowAnonymous()` on a mapped endpoint.
- `GET /User/Login` returns the current user:

  ```json
  { "name": "DOMAIN\\username", "displayName": "Jon Doe" }
  ```

  `displayName` comes from Active Directory (cached 1 h). If AD can't be reached or has no display name,
  it falls back to the username and logs a warning. Local accounts (`MACHINE\user`) are looked up on the machine.
- `User.Identity.Name` gives the same `DOMAIN\username` in any controller.

### Frontend (Vue / fetch / axios)

The frontend on another origin (Vite dev server `http://localhost:5173`) must send credentials,
and its origin must be listed in `Cors:Origins`:

```ts
fetch(`${apiUrl}/User/Login`, { credentials: 'include' })
// axios: axios.create({ baseURL: apiUrl, withCredentials: true })
```

Edge / Chrome sign in automatically for intranet hosts and `localhost`.
Firefox needs the API host in `about:config` → `network.negotiate-auth.trusted-uris`.

### Hosting

| Host | What to configure |
|---|---|
| Kestrel (`dotnet run`, `http`/`https` profiles) | Nothing. Works on a domain-joined machine. HTTP/1.1 only (Windows auth doesn't work over HTTP/2). |
| IIS Express (Visual Studio) | Already enabled in `launchSettings.json` (`iisSettings`). |
| IIS | Install the *Windows Authentication* role service. In the site's **Authentication**: enable **Windows Authentication** and keep **Anonymous Authentication** enabled — the app itself rejects anonymous requests, and CORS preflight (`OPTIONS`) requests are always anonymous. If Windows Authentication is disabled in IIS, the app throws at the first request. |

The app pool identity (`ApplicationPoolIdentity` on a domain-joined server = the computer account)
must be able to read users in AD — which domain computers can by default.

## New project from this template

1. Rename the solution, project folder, `.csproj` and the `WebApiTemplateWinAuth` root namespace.
2. Fill in the `TODO` connection strings. If you use user secrets, run `dotnet user-secrets init` (or put a new GUID in `<UserSecretsId>`) so projects don't share secrets.
3. Add a server: uncomment `Erp` in `SqlServer`, `DatabaseOptions`, `SqlConnectionFactory`
   and `AddSqlData` (connection string), then fill `ConnectionStrings:Erp` in both appsettings files.
4. Set `Cors:Origins` for every environment where the frontend runs on a different origin.

## Settings

| Key | Default | |
|---|---|---|
| `ConnectionStrings:App` | — | required |
| `Cors:Origins` | `[]` (Dev: `http://localhost:5173`) | allowed frontend origins |
