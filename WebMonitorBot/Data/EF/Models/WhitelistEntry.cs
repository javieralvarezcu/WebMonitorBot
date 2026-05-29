using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMonitorBot.Data.EF.Models
{
    [Table("Whitelist")]
    public class WhitelistEntry
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public long ChatId { get; set; }
    }
}
