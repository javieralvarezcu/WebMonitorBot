using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using WebMonitorBot.Data;

namespace WebMonitorBot.Bot
{
    // Service that initializes the Telegram client and handles basic commands.
    public class TelegramBotHostedService : IHostedService
    {
        private readonly ILogger<TelegramBotHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Telegram.Bot.ITelegramBotClient _client;

        public TelegramBotHostedService(ILogger<TelegramBotHostedService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory, Telegram.Bot.ITelegramBotClient client)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Cliente de Telegram provisto por DI
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Recibir todos los tipos
            };

            _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cancellationToken);
            // Register commands so Telegram shows them in the input field
            var commands = new[]
            {
                new Telegram.Bot.Types.BotCommand { Command = "monitor", Description = "Registrar URL para monitoreo. Uso: /monitorear <url> [segundos]" },
                new Telegram.Bot.Types.BotCommand { Command = "setinterval", Description = "Actualizar intervalo de comprobación. Uso: /setinterval <url> <segundos>" },
                new Telegram.Bot.Types.BotCommand { Command = "list", Description = "Listar las URLs registradas y su chat asociado" },
                new Telegram.Bot.Types.BotCommand { Command = "remove", Description = "Eliminar una URL registrada. Uso: /eliminar <url>" },
                new Telegram.Bot.Types.BotCommand { Command = "help", Description = "Mostrar ayuda con los comandos disponibles" }
            };

            await _client.SetMyCommands(commands, cancellationToken: cancellationToken);
            _logger.LogInformation("Telegram bot started.");

            // Cliente inyectado por DI.
            return;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Telegram bot stopped.");
            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                if (update.Message is not { } message) return;
                if (message.Type != MessageType.Text) return;

                var text = message.Text!.Trim();
                // Comprobar whitelist
                using var scopeCheck = _scopeFactory.CreateScope();
                var whitelist = (WebMonitorBot.Services.ChatWhitelistDbAdapter)scopeCheck.ServiceProvider.GetService(typeof(WebMonitorBot.Services.ChatWhitelistDbAdapter))!;
                if (!await whitelist.IsAllowedAsync(message.Chat.Id))
                {
                    await botClient.SendMessage(message.Chat.Id, "Tu chat no está autorizado para usar este bot.", cancellationToken: ct);
                    return;
                }

                if (text.StartsWith("/monitor", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                    {
                        await botClient.SendMessage(message.Chat.Id, "Uso: /monitor <url>", cancellationToken: ct);
                        return;
                    }

                    var url = parts[1].Trim();
                    int? intervalSeconds = null;
                    if (parts.Length >= 3 && int.TryParse(parts[2], out var s)) intervalSeconds = s;

                    // Register in repository
                    using var scope = _scopeFactory.CreateScope();
                    var repo = (Data.IMonitoringRepository)scope.ServiceProvider.GetService(typeof(Data.IMonitoringRepository))!;
                    var existing = await repo.GetByUrlAndChatAsync(url, message.Chat.Id);
                    if (existing is not null)
                    {
                        await botClient.SendMessage(message.Chat.Id, "Ya estás monitoreando esa URL.", cancellationToken: ct);
                        return;
                    }

                    var item = new Data.MonitoringUrl
                    {
                        ChatId = message.Chat.Id,
                        Url = url,
                        LastHash = null,
                        LastCleanText = null,
                        CheckIntervalSeconds = intervalSeconds,
                        LastCheckedUtc = null
                    };

                    await repo.AddAsync(item);
                    await botClient.SendMessage(message.Chat.Id, $"Comenzando a monitorear {url}", cancellationToken: ct);
                }
                else if (text.StartsWith("/setinterval", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 3 || !int.TryParse(parts[2], out var seconds) || seconds <= 0)
                    {
                        await botClient.SendMessage(message.Chat.Id, "Uso: /setinterval <url> <seconds> (seconds debe ser entero y > 0)", cancellationToken: ct);
                        return;
                    }

                    var url = parts[1].Trim();
                    using var scope2 = _scopeFactory.CreateScope();
                    var repo2 = (Data.IMonitoringRepository)scope2.ServiceProvider.GetService(typeof(Data.IMonitoringRepository))!;
                    var existing2 = await repo2.GetByUrlAndChatAsync(url, message.Chat.Id);
                    if (existing2 is null)
                    {
                        await botClient.SendMessage(message.Chat.Id, "La URL no está registrada. Usa /monitorear <url> para agregarla primero.", cancellationToken: ct);
                        return;
                    }

                    existing2.CheckIntervalSeconds = seconds;
                    existing2.LastCheckedUtc = null; // allow immediate check with new interval
                    await repo2.UpdateAsync(existing2);
                    await botClient.SendMessage(message.Chat.Id, $"Intervalo actualizado para {url}: {seconds} segundos", cancellationToken: ct);
                }
                else if (text.StartsWith("/list", StringComparison.OrdinalIgnoreCase))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = (Data.IMonitoringRepository)scope.ServiceProvider.GetService(typeof(Data.IMonitoringRepository))!;
                    var items = await repo.GetByChatIdAsync(message.Chat.Id);
                    var sb = new System.Text.StringBuilder();
                    foreach (var it in items)
                    {
                        sb.AppendLine($"- {it.Url}");
                    }

                    var resp = sb.Length == 0 ? "No hay URLs registradas." : sb.ToString();
                    await botClient.SendMessage(message.Chat.Id, resp, cancellationToken: ct);
                }
                else if (text.StartsWith("/remove", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                    {
                        await botClient.SendMessage(message.Chat.Id, "Uso: /eliminar <url>", cancellationToken: ct);
                        return;
                    }

                    var url = parts[1].Trim();
                    using var scope3 = _scopeFactory.CreateScope();
                    var repo3 = (Data.IMonitoringRepository)scope3.ServiceProvider.GetService(typeof(Data.IMonitoringRepository))!;
                    var existing3 = await repo3.GetByUrlAndChatAsync(url, message.Chat.Id);
                    if (existing3 is null)
                    {
                        await botClient.SendMessage(message.Chat.Id, "La URL no está registrada.", cancellationToken: ct);
                        return;
                    }
                    await repo3.RemoveAsync(url, message.Chat.Id);
                    await botClient.SendMessage(message.Chat.Id, $"URL eliminada: {url}", cancellationToken: ct);
                }
                else if (text.Equals("/help", StringComparison.OrdinalIgnoreCase) || text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    var help = "Comandos disponibles:\n" +
                               "/monitor <url> [seconds] - Register URL for monitoring\n" +
                               "/setinterval <url> <seconds> - Update interval for a registered URL\n" +
                               "/list - List registered URLs\n" +
                               "/remove <url> - Remove a registered URL";
                    await botClient.SendMessage(message.Chat.Id, help, cancellationToken: ct);
                }
                else
                {
                    await botClient.SendMessage(message.Chat.Id, "Comando no reconocido. Usa /monitor <url>", cancellationToken: ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling update");
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            _logger.LogError(exception, "Telegram bot error");
            return Task.CompletedTask;
        }
    }
}
