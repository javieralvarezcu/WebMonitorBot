using Microsoft.EntityFrameworkCore;

namespace WebMonitorBot.Data.EF
{
    public class WebMonitorContext : DbContext
    {
        public WebMonitorContext(DbContextOptions<WebMonitorContext> options) : base(options) { }

        public DbSet<Models.MonitoringUrlEntity> MonitoringUrls { get; set; }
        public DbSet<Models.WhitelistEntry> Whitelist { get; set; }
    }
}
