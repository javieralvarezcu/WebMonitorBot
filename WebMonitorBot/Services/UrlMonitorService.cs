using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using WebMonitorBot.Data;

namespace WebMonitorBot.Services
{
    // Hosted service that runs a PeriodicTimer and processes monitored URLs.
    public class UrlMonitorService : BackgroundService
    {
        private readonly IHtmlExtractor _extractor;
        private readonly ILlmClient _llmClient;
        private readonly ILogger<UrlMonitorService> _logger;
        private readonly Telegram.Bot.ITelegramBotClient _telegramClient;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly int _defaultIntervalSeconds;

        public UrlMonitorService(
            IHtmlExtractor extractor,
            ILlmClient llmClient,
            ILogger<UrlMonitorService> logger,
            Telegram.Bot.ITelegramBotClient telegramClient,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IServiceScopeFactory scopeFactory)
        {
            _extractor = extractor;
            _llmClient = llmClient;
            _logger = logger;
            _telegramClient = telegramClient;
            _scopeFactory = scopeFactory;
            // Read default interval (seconds) from configuration
            if (!int.TryParse(configuration["Monitoring:DefaultIntervalSeconds"], out _defaultIntervalSeconds))
            {
                _defaultIntervalSeconds = 300; // default 5 minutes
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UrlMonitorService started.");

            // Ejecutar en ticks cortos y procesar solo las URLs cuyo intervalo haya expirado
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monitoring loop");
                }

                try
                {
                    // Espera el siguiente tick; si el token se canceló, cancelará
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                        break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("UrlMonitorService stopped.");
        }

        private async Task ProcessOnceAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = (IMonitoringRepository)scope.ServiceProvider.GetService(typeof(IMonitoringRepository))!;
            var items = (await repo.GetAllAsync()).ToList();
            if (!items.Any())
            {
                _logger.LogDebug("No registered URLs to monitor.");
                return;
            }

            var now = DateTime.UtcNow;

            // Seleccionar solo las URLs cuya próxima comprobación ha llegado
            var due = items.Where(item =>
            {
                var interval = item.CheckIntervalSeconds ?? _defaultIntervalSeconds;
                if (interval <= 0) interval = _defaultIntervalSeconds;
                if (!item.LastCheckedUtc.HasValue) return true;
                return item.LastCheckedUtc.Value.AddSeconds(interval) <= now;
            }).ToList();

            if (!due.Any())
            {
                _logger.LogDebug("No URLs due for checking in this tick.");
                return;
            }

            // Marcar LastCheckedUtc antes de procesar para evitar dobles ejecuciones
            foreach (var item in due)
            {
                item.LastCheckedUtc = now;
                using var s2 = _scopeFactory.CreateScope();
                var repo2 = (IMonitoringRepository)s2.ServiceProvider.GetService(typeof(IMonitoringRepository))!;
                await repo2.UpdateAsync(item);
            }

            var tasks = due.Select(item => ProcessItemAsync(item, ct));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessItemAsync(MonitoringUrl item, CancellationToken ct)
        {
            try
            {
                var newText = await _extractor.ExtractTextAsync(item.Url);
                var newHash = ComputeSha256(newText);
                if (newHash != item.LastHash)
                {
                    _logger.LogInformation("Change detected in {Url}", item.Url);

                    var previousText = item.LastCleanText ?? string.Empty;
                    var description = $"Detected change on url: {item.Url}\n" + await _llmClient.DescribeDifferenceAsync(previousText, newText);
                    // Enviar mensaje al chat sólo si está en whitelist (resolver adapter por scope)
                    using var sCheck = _scopeFactory.CreateScope();
                    var whitelistAdapter = (ChatWhitelistDbAdapter)sCheck.ServiceProvider.GetService(typeof(ChatWhitelistDbAdapter))!;
                    if (await whitelistAdapter.IsAllowedAsync(item.ChatId))
                    {
                        await _telegramClient.SendMessage(item.ChatId, description, cancellationToken: ct);
                    }

                    // Actualizar repositorio
                    item.LastCleanText = newText;
                    item.LastHash = newHash;
                    using var s3 = _scopeFactory.CreateScope();
                    var repo3 = (IMonitoringRepository)s3.ServiceProvider.GetService(typeof(IMonitoringRepository))!;
                    await repo3.UpdateAsync(item);
                }
                else
                {
                    _logger.LogDebug("No changes in {Url}", item.Url);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {Url}", item.Url);
            }
        }

        private static string ComputeSha256(string text)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
