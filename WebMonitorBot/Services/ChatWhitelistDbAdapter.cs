using System.Collections.Generic;
using System.Threading.Tasks;
using WebMonitorBot.Data;

namespace WebMonitorBot.Services
{
    public class ChatWhitelistDbAdapter
    {
        private readonly EfWhitelistRepository _repo;

        public ChatWhitelistDbAdapter(EfWhitelistRepository repo)
        {
            _repo = repo;
        }

        public Task<bool> IsAllowedAsync(long chatId) => _repo.IsAllowedAsync(chatId);

        public Task EnsureInitialAsync(long[] initial) => _repo.EnsureInitialAsync(initial);
    }
}
