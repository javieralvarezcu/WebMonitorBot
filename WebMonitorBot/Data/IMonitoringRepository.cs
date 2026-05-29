using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebMonitorBot.Data
{
    public interface IMonitoringRepository
    {
        Task<IEnumerable<MonitoringUrl>> GetAllAsync();
        Task<IEnumerable<MonitoringUrl>> GetByChatIdAsync(long chatId);
        Task AddAsync(MonitoringUrl item);
        Task UpdateAsync(MonitoringUrl item);
        Task RemoveAsync(string url, long chatId);
        Task<MonitoringUrl?> GetByUrlAsync(string url);
        Task<MonitoringUrl?> GetByUrlAndChatAsync(string url, long chatId);
    }
}
