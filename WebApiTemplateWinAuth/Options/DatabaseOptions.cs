using System.ComponentModel.DataAnnotations;

namespace WebApiTemplateWinAuth.Options
{
    /// <summary>
    /// Connection strings, filled from the "ConnectionStrings" section in AddSqlData.
    /// </summary>
    public sealed class DatabaseOptions
    {
        // [Required(ErrorMessage = "ConnectionStrings:Erp is empty. Fill it in appsettings.json / appsettings.Development.json.")]
        // public string ErpConnectionString { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConnectionStrings:App is empty. Fill it in appsettings.json / appsettings.Development.json.")]
        public string AppConnectionString { get; set; } = string.Empty;
    }
}
