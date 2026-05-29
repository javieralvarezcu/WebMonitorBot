using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DeepSeek.Core;

namespace WebMonitorBot.Services
{
    // Servicio que verifica la validez de la API key de Deepseek al iniciar.
    public class DeepseekPreflightService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DeepseekPreflightService> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        public DeepseekPreflightService(IConfiguration configuration, ILogger<DeepseekPreflightService> logger, IHostApplicationLifetime lifetime)
        {
            _configuration = configuration;
            _logger = logger;
            _lifetime = lifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var apiKey = _configuration["Deepseek:ApiKey"] ?? Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Deepseek API key no encontrada en Deepseek:ApiKey ni DEEPSEEK_API_KEY. Cancelando arranque.");
                // Parar la aplicación
                _lifetime.StopApplication();
                return;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
                var client = new DeepSeekClient(apiKey);
                var models = await client.ListModelsAsync(linked.Token);
                if (models == null || models.Data == null)
                {
                    _logger.LogError("Deepseek: no se pudieron listar modelos. Respuesta nula. Mensaje: {Err}", client.ErrorMsg);
                    _lifetime.StopApplication();
                    return;
                }

                _logger.LogInformation("Deepseek: modelos disponibles: {Count}", models.Data.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando Deepseek API key; deteniendo la aplicación.");
                _lifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Nothing to do
            return Task.CompletedTask;
        }
    }
}
