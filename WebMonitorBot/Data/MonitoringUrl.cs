using System;

namespace WebMonitorBot.Data
{
    public class MonitoringUrl
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long ChatId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? LastCleanText { get; set; }
        public string? LastHash { get; set; }
        public int? CheckIntervalSeconds { get; set; }

        public DateTime? LastCheckedUtc { get; set; }
    }
}
