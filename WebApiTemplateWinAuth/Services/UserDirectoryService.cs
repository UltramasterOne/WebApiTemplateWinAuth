using Microsoft.Extensions.Caching.Memory;
using System.DirectoryServices.AccountManagement;

namespace WebApiTemplateWinAuth.Services
{
    /// <summary>
    /// Reads user details from Active Directory (or the local machine for local accounts).
    /// Results are cached, so the directory is queried at most once per user per hour.
    /// </summary>
    public sealed class UserDirectoryService(IMemoryCache cache, ILogger<UserDirectoryService> logger)
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        // A failed lookup (AD unreachable, ...) is retried sooner.
        private static readonly TimeSpan FailedLookupCacheDuration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Display name ("Jon Doe") for an account in "DOMAIN\username" form (User.Identity.Name).
        /// Falls back to the username when the directory has no display name or can't be reached.
        /// </summary>
        public string GetDisplayName(string accountName)
        {
            return cache.GetOrCreate($"user-display-name:{accountName.ToUpperInvariant()}", entry =>
            {
                var displayName = LookupDisplayName(accountName);

                entry.AbsoluteExpirationRelativeToNow = displayName is null ? FailedLookupCacheDuration : CacheDuration;

                return displayName ?? UserNamePart(accountName);
            })!;
        }

        private string? LookupDisplayName(string accountName)
        {
            try
            {
                using var context = CreateContext(accountName);
                using var user = UserPrincipal.FindByIdentity(context, accountName);

                if (string.IsNullOrWhiteSpace(user?.DisplayName))
                {
                    logger.LogWarning("No display name found in the directory for {AccountName}.", accountName);
                    return null;
                }

                return user.DisplayName;
            }
            catch (Exception ex)
            {
                // Login must not fail just because the directory lookup did.
                logger.LogWarning(ex, "Directory lookup failed for {AccountName}.", accountName);
                return null;
            }
        }

        /// <summary>
        /// Local accounts ("MACHINE\user") are looked up on the machine, everything else in the account's domain.
        /// </summary>
        private static PrincipalContext CreateContext(string accountName)
        {
            var separator = accountName.IndexOf('\\');
            var domain = separator > 0 ? accountName[..separator] : null;

            return string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase)
                ? new PrincipalContext(ContextType.Machine)
                : new PrincipalContext(ContextType.Domain, domain);
        }

        private static string UserNamePart(string accountName)
            => accountName[(accountName.IndexOf('\\') + 1)..];
    }
}
