using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebMonitorBot.Bot;
using WebMonitorBot.Data;
using WebMonitorBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebMonitorBot.Data.EF;
using Telegram.Bot;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Telegram bot token must be set in configuration or environment variable: Telegram:BotToken or TELEGRAM_BOT_TOKEN
        services.AddHttpClient();

        // Registrar EF DbContext y repositorios
        var conn = context.Configuration.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("DATABASE_CONN") ?? "";
        services.AddDbContext<WebMonitorBot.Data.EF.WebMonitorContext>(options => options.UseSqlServer(conn));
        services.AddScoped<IMonitoringRepository, EfMonitoringRepository>();
        services.AddScoped<EfWhitelistRepository>();
        services.AddScoped<ChatWhitelistDbAdapter>();
        services.AddHostedService<WhitelistSeedService>();
        services.AddSingleton<IHtmlExtractor, AngleSharpHtmlExtractor>();
        // Registrar cliente LLM (Deepseek SDK). DeepseekLlmClient crea internamente DeepSeekClient usando la API key desde la configuración; lanzará excepción si no hay API key.
        services.AddSingleton<ILlmClient, DeepseekLlmClient>();

        // Adapter para whitelist en DB (ya registrado arriba)

        // Registrar cliente de Telegram
        var token = context.Configuration["Telegram:BotToken"] ?? Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Token de Telegram no configurado. Establece Telegram:BotToken o la variable de entorno TELEGRAM_BOT_TOKEN.");
        }

        services.AddSingleton<ITelegramBotClient>(sp => new TelegramBotClient(token));

        // Preflight: validar Deepseek API key antes de arrancar el resto de servicios
        services.AddHostedService<DeepseekPreflightService>();

        // Hosted services
        services.AddHostedService<TelegramBotHostedService>();
        services.AddHostedService<UrlMonitorService>();
    });

var host = builder.Build();

// Aplicar migraciones al arrancar la aplicación
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WebMonitorBot.Data.EF.WebMonitorContext>();
    db.Database.Migrate();
}

await host.RunAsync();
