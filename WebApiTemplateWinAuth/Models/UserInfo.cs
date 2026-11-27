namespace WebApiTemplateWinAuth.Models
{
    public sealed class UserInfo
    {
        /// <summary>Windows account, e.g. "DOMAIN\username".</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Display name from Active Directory, e.g. "Jon Doe".</summary>
        public string DisplayName { get; set; } = string.Empty;
    }
}
