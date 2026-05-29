using System.Threading.Tasks;

namespace WebMonitorBot.Services
{
    public interface ILlmClient
    {
        /// <summary>
        /// Dado el texto anterior y el nuevo, devuelve un resumen de las diferencias.
        /// En esta implementación puede ser una llamada real a un LLM o una simulación.
        /// </summary>
        Task<string> DescribeDifferenceAsync(string previousText, string newText);
    }
}
