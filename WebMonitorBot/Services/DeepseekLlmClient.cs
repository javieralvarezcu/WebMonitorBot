using System;
using System.Threading.Tasks;
using DeepSeek.Core;
using DeepSeek.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace WebMonitorBot.Services
{
    public class DeepseekLlmClient : ILlmClient
    {
        private readonly DeepSeekClient _client;
        private readonly ILogger<DeepseekLlmClient> _logger;

        public DeepseekLlmClient(IConfiguration configuration, ILogger<DeepseekLlmClient> logger)
        {
            _logger = logger;
            var apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? configuration["Deepseek:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("Deepseek API key no configurada. Establece la variable de entorno DEEPSEEK_API_KEY.");

            _client = new DeepSeekClient(apiKey);
        }

        public async Task<string> DescribeDifferenceAsync(string previousText, string newText)
        {
            var prompt = BuildPrompt(previousText ?? string.Empty, newText ?? string.Empty);

            var request = new ChatRequest
            {
                Messages = new List<Message>
                {
                    Message.NewSystemMessage("Eres un asistente que resume diferencias entre textos."),
                    Message.NewUserMessage(prompt)
                },
                Model = DeepSeekModels.Flash,
                MaxTokens = 512
            };

            try
            {
                var resp = await _client.ChatAsync(request, new System.Threading.CancellationToken());
                if (resp is null)
                {
                    _logger.LogWarning("Deepseek ChatAsync returned null: {Err}", _client.ErrorMsg);
                    return await new SimulatedLlmClient().DescribeDifferenceAsync(previousText, newText);
                }

                var choice = resp.Choices?.Count > 0 ? resp.Choices[0] : null;
                var content = choice?.Message?.Content;
                return content ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando a Deepseek SDK");
                return await new SimulatedLlmClient().DescribeDifferenceAsync(previousText, newText);
            }
        }

        private static string BuildPrompt(string previous, string current)
        {
            return $"[Texto Anterior]\n{previous}\n\n[Texto Nuevo]\n{current}\n\nResume brevemente los cambios relevantes en español.";
        }
    }
}
