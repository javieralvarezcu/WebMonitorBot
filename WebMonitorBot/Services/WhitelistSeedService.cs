using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace WebMonitorBot.Services
{
    public class WhitelistSeedService : IHostedService
    {
        private readonly ILogger<WhitelistSeedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public WhitelistSeedService(IServiceScopeFactory scopeFactory, ILogger<WhitelistSeedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var adapter = scope.ServiceProvider.GetRequiredService<ChatWhitelistDbAdapter>();
                await adapter.EnsureInitialAsync(new long[] { 989674402 });
                _logger.LogInformation("Whitelist initial seed applied.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error seeding whitelist.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
