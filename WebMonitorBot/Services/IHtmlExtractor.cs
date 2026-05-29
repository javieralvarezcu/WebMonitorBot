using System.Threading.Tasks;

namespace WebMonitorBot.Services
{
    public interface IHtmlExtractor
    {
        /// <summary>
        /// Descarga la URL y devuelve el texto limpio (sin script/style/nav/footer y sin etiquetas).
        /// </summary>
        Task<string> ExtractTextAsync(string url);
    }
}
