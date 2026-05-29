using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebMonitorBot.Services
{
    // Servicio simple para gestionar una whitelist de chatIds persistida en JSON
    public class ChatWhitelist
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private readonly HashSet<long> _allowed = new();
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

        public ChatWhitelist(string filePath, IEnumerable<long>? initial = null)
        {
            _filePath = filePath;

            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    var items = JsonSerializer.Deserialize<List<long>>(json, _jsonOptions) ?? new List<long>();
                    foreach (var id in items) _allowed.Add(id);
                }
                catch
                {
                    // ignore parse errors, start empty
                }
            }
            else
            {
                if (initial != null)
                {
                    foreach (var id in initial) _allowed.Add(id);
                }
                Save();
            }
        }

        public Task<bool> IsAllowedAsync(long chatId)
        {
            lock (_lock) return Task.FromResult(_allowed.Contains(chatId));
        }

        public Task AddAsync(long chatId)
        {
            lock (_lock)
            {
                _allowed.Add(chatId);
                Save();
            }
            return Task.CompletedTask;
        }

        public Task RemoveAsync(long chatId)
        {
            lock (_lock)
            {
                _allowed.Remove(chatId);
                Save();
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<long>> GetAllAsync()
        {
            lock (_lock) return Task.FromResult(_allowed.AsEnumerable());
        }

        private void Save()
        {
            try
            {
                var list = _allowed.ToList();
                var json = JsonSerializer.Serialize(list, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // ignore
            }
        }
    }
}
