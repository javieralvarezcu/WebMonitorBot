using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMonitorBot.Data.EF.Models
{
    [Table("MonitoringUrls")]
    public class MonitoringUrlEntity
    {
        [Key]
        public Guid Id { get; set; }
        public long ChatId { get; set; }
        [Required]
        public string Url { get; set; } = string.Empty;
        public string? LastCleanText { get; set; }
        public string? LastHash { get; set; }
        public int? CheckIntervalSeconds { get; set; }
        public DateTime? LastCheckedUtc { get; set; }
    }
}
