using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiTemplateWinAuth.Models
{
    public sealed class Item
    {
        [Column("No_")]
        public string ItemNo { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
