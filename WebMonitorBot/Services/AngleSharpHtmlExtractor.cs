using AngleSharp;
using AngleSharp.Dom;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebMonitorBot.Services
{
    public class AngleSharpHtmlExtractor : IHtmlExtractor
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AngleSharpHtmlExtractor(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> ExtractTextAsync(string url)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = System.TimeSpan.FromSeconds(30);

            const int maxAttempts = 3;
            int attempt = 0;
            string html = string.Empty;
            while (true)
            {
                attempt++;
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Some servers reject empty/user-agent-less requests
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; WebMonitorBot/1.0)");
                request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                try
                {
                    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    html = await response.Content.ReadAsStringAsync();
                    break;
                }
                catch (System.Net.Http.HttpRequestException) when (attempt < maxAttempts)
                {
                    // retry transient network errors
                    await Task.Delay(200 * attempt);
                    continue;
                }
                catch (System.OperationCanceledException) when (attempt < maxAttempts)
                {
                    // timeout, try again
                    await Task.Delay(200 * attempt);
                    continue;
                }
            }

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(html));

            // Remover elementos no deseados
            var selectorsToRemove = new[] { "script", "style", "nav", "footer" };
            foreach (var sel in selectorsToRemove)
            {
                var nodes = document.QuerySelectorAll(sel).ToArray();
                foreach (var n in nodes)
                {
                    n.Remove();
                }
            }

            // Obtener texto visible
            var text = document.Body?.TextContent ?? string.Empty;

            // Normalizar espaciado
            var normalized = string.Join(' ', text.Split('\n', '\r').SelectMany(l => l.Split('\t')).Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)));
            return normalized;
        }
    }
}
