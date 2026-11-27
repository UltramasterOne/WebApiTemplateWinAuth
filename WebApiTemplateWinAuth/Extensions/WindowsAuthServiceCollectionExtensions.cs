using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using WebApiTemplateWinAuth.Services;

namespace WebApiTemplateWinAuth.Extensions
{
    public static class WindowsAuthServiceCollectionExtensions
    {
        /// <summary>
        /// Windows Authentication (Negotiate: Kerberos, NTLM fallback).
        /// On Kestrel the handler authenticates itself; on IIS it defers to IIS Windows Authentication,
        /// which must then be enabled for the site (see README).
        /// </summary>
        public static IServiceCollection AddWindowsAuth(this IServiceCollection services)
        {
            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            // Every endpoint requires a signed-in Windows user,
            // unless it is marked [AllowAnonymous] (controllers) or .AllowAnonymous() (mapped endpoints).
            services.AddAuthorization(o => o.FallbackPolicy = o.DefaultPolicy);

            // Active Directory lookups (display name, ...), cached
            services.AddMemoryCache();
            services.AddSingleton<UserDirectoryService>();

            return services;
        }

        /// <summary>
        /// Windows Authentication doesn't work over HTTP/2, so Kestrel serves HTTP/1.1 only.
        /// Ignored when hosted in IIS (IIS handles the protocol there).
        /// </summary>
        public static IWebHostBuilder UseHttp1ForWindowsAuth(this IWebHostBuilder webHost) =>
            webHost.ConfigureKestrel(k => k.ConfigureEndpointDefaults(l => l.Protocols = HttpProtocols.Http1));
    }
}
