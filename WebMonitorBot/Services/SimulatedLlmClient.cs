using System.Threading.Tasks;

namespace WebMonitorBot.Services
{
    // Implementación de ejemplo que simula una llamada a un LLM. Reemplazar por Microsoft.SemanticKernel o una API real.
    public class SimulatedLlmClient : ILlmClient
    {
        public Task<string> DescribeDifferenceAsync(string previousText, string newText)
        {
            // Simple heurística: muestra longitud y primeros/últimos 200 caracteres
            string Summarize(string t)
            {
                if (string.IsNullOrEmpty(t)) return "(vacío)";
                if (t.Length <= 200) return t;
                return t[..100] + " ... " + t[^100..];
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Se detectó un cambio en la página web.");
            sb.AppendLine($"Texto anterior (longitud {previousText?.Length ?? 0}): {Summarize(previousText ?? string.Empty)}");
            sb.AppendLine($"Texto nuevo (longitud {newText?.Length ?? 0}): {Summarize(newText ?? string.Empty)}");
            sb.AppendLine();
            sb.AppendLine("Diferencias destacadas:");
            sb.AppendLine("- Esta es una descripción simulada. Reemplaza este cliente por uno real si deseas un análisis semántico.");

            return Task.FromResult(sb.ToString());
        }
    }
}
