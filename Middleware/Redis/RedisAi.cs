using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Redis
{
    public static class RedisAi
    {
        public async static Task AiPush(long uid)
        {
            //zset
            await RedisHelper.cli.ZIncrByAsync("user:ai",30,uid.ToString());
        }
        public async static Task<long> GetAiCount(long uid)
        {
            //zset
            if(!await RedisHelper.cli.ExistsAsync("user:ai"))
            {
                return 0;
            }
            return (long)await RedisHelper.cli.ZScoreAsync("user:ai",uid.ToString());
        }
        public async static Task decrAiCount(long uid)
        {
            //减少ai的数量
            await RedisHelper.cli.ZIncrByAsync("user:ai",-1,uid.ToString());
        }
    }
}
