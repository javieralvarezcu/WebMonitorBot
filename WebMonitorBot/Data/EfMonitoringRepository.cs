using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebMonitorBot.Data.EF;
using WebMonitorBot.Data.EF.Models;

namespace WebMonitorBot.Data
{
    public class EfMonitoringRepository : IMonitoringRepository
    {
        private readonly WebMonitorContext _ctx;

        public EfMonitoringRepository(WebMonitorContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(MonitoringUrl item)
        {
            var entity = new MonitoringUrlEntity
            {
                Id = item.Id,
                ChatId = item.ChatId,
                Url = item.Url,
                LastCleanText = item.LastCleanText,
                LastHash = item.LastHash,
                CheckIntervalSeconds = item.CheckIntervalSeconds,
                LastCheckedUtc = item.LastCheckedUtc
            };

            _ctx.MonitoringUrls.Add(entity);
            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<MonitoringUrl>> GetAllAsync()
        {
            return await _ctx.MonitoringUrls.Select(e => new MonitoringUrl
            {
                Id = e.Id,
                ChatId = e.ChatId,
                Url = e.Url,
                LastCleanText = e.LastCleanText,
                LastHash = e.LastHash,
                CheckIntervalSeconds = e.CheckIntervalSeconds,
                LastCheckedUtc = e.LastCheckedUtc
            }).ToListAsync();
        }

        public async Task<IEnumerable<MonitoringUrl>> GetByChatIdAsync(long chatId)
        {
            return await _ctx.MonitoringUrls.Where(e => e.ChatId == chatId).Select(e => new MonitoringUrl
            {
                Id = e.Id,
                ChatId = e.ChatId,
                Url = e.Url,
                LastCleanText = e.LastCleanText,
                LastHash = e.LastHash,
                CheckIntervalSeconds = e.CheckIntervalSeconds,
                LastCheckedUtc = e.LastCheckedUtc
            }).ToListAsync();
        }

        public async Task<MonitoringUrl?> GetByUrlAsync(string url)
        {
            var e = await _ctx.MonitoringUrls.FirstOrDefaultAsync(x => x.Url == url);
            if (e == null) return null;
            return new MonitoringUrl
            {
                Id = e.Id,
                ChatId = e.ChatId,
                Url = e.Url,
                LastCleanText = e.LastCleanText,
                LastHash = e.LastHash,
                CheckIntervalSeconds = e.CheckIntervalSeconds,
                LastCheckedUtc = e.LastCheckedUtc
            };
        }

        public async Task<MonitoringUrl?> GetByUrlAndChatAsync(string url, long chatId)
        {
            var e = await _ctx.MonitoringUrls.FirstOrDefaultAsync(x => x.Url == url && x.ChatId == chatId);
            if (e == null) return null;
            return new MonitoringUrl
            {
                Id = e.Id,
                ChatId = e.ChatId,
                Url = e.Url,
                LastCleanText = e.LastCleanText,
                LastHash = e.LastHash,
                CheckIntervalSeconds = e.CheckIntervalSeconds,
                LastCheckedUtc = e.LastCheckedUtc
            };
        }

        public async Task RemoveAsync(string url, long chatId)
        {
            var e = await _ctx.MonitoringUrls.FirstOrDefaultAsync(x => x.Url == url && x.ChatId == chatId);
            if (e != null)
            {
                _ctx.MonitoringUrls.Remove(e);
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(MonitoringUrl item)
        {
            var e = await _ctx.MonitoringUrls.FirstOrDefaultAsync(x => x.Id == item.Id);
            if (e != null)
            {
                e.LastCleanText = item.LastCleanText;
                e.LastHash = item.LastHash;
                e.CheckIntervalSeconds = item.CheckIntervalSeconds;
                e.LastCheckedUtc = item.LastCheckedUtc;
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
