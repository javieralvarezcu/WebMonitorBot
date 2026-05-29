using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebMonitorBot.Data.EF;
using WebMonitorBot.Data.EF.Models;

namespace WebMonitorBot.Data
{
    public class EfWhitelistRepository
    {
        private readonly WebMonitorContext _ctx;

        public EfWhitelistRepository(WebMonitorContext ctx)
        {
            _ctx = ctx;
        }

        public async Task EnsureInitialAsync(long[] initial)
        {
            foreach (var id in initial)
            {
                var found = await _ctx.Whitelist.FindAsync(id);
                if (found == null)
                {
                    _ctx.Whitelist.Add(new WhitelistEntry { ChatId = id });
                }
            }

            await _ctx.SaveChangesAsync();
        }

        public async Task<bool> IsAllowedAsync(long chatId)
        {
            var ent = await _ctx.Whitelist.FindAsync(chatId);
            return ent != null;
        }
    }
}
