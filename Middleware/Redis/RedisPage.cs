using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Redis
{
    public static class RedisPage
    {
        public static async Task<bool> IsPageLike(long pageid,long uid)
        {
            return await RedisHelper.cli.SIsMemberAsync($"user:likes:page:{uid}", pageid);
        }
        public static async Task LikePage(long pageid, long uid)
        {
             await RedisHelper.cli.SAddAsync($"user:likes:page:{uid}", pageid);
            await RedisHelper.cli.SAddAsync($"page:likes:{pageid}", uid);
        }
        public static async Task UnlikePage(long pageid, long uid)
        {
            await RedisHelper.cli.SRemAsync($"user:likes:page:{uid}", pageid);
            await RedisHelper.cli.SRemAsync($"page:likes:{pageid}", uid);
        }
        public static async Task<long> GetPageLikeCount(long pageid)
        {
            return await RedisHelper.cli.SCardAsync($"page:likes:{pageid}");
        }

    }
}
