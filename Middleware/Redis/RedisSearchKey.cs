using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace Middleware.Redis
{
    public static class RedisSearchKey
    {
        public static async Task AddSearchKey(string key)
        {
            await RedisHelper.cli.ZIncrByAsync("search_key", 1, key);
            await RedisHelper.cli.HSetAsync("last_access_key", key, DateTime.UtcNow.Ticks);
        }
        public static async Task<List<string>> GetSearchKeyRank()
        {
            var keys = await RedisHelper.cli.ZRevRangeAsync("search_key", 0,33);
            return keys.ToList();
        }
        public static async Task CleanUpExpiredTerms(TimeSpan expiry)
        {
            var now = DateTime.UtcNow.Ticks;
            var expiredTerms = RedisHelper.cli.HGetAll("last_access_key")
                .Where(x => (now - long.Parse(x.Value)) > expiry.Ticks)
                .Select(x => x.Key);

            foreach(var term in expiredTerms)
            {
                await RedisHelper.cli.ZRemAsync("search_key", term); // 从 Sorted Set 中删除
                await RedisHelper.cli.HDelAsync("last_access_key", term); // 从 last_access Hash 中删除  
            }
        }
    }
}
