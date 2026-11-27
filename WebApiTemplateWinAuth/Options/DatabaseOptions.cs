using System.ComponentModel.DataAnnotations;

namespace WebApiTemplateWinAuth.Options
{
    public sealed class DatabaseOptions
    {
        [Required]
        public string AppConnectionString { get; set; } = string.Empty;

        [Range(1, 3600)]
        public int CommandTimeoutSeconds { get; set; } = 30;
    }
}
