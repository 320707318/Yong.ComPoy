using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Redis
{
    public class RedisReq
    {
    }
    public class PageTypeWeight
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
    public class PageIdsReq
    {
        public List<PageTypeWeight> pageTypes { get; set; }
        public int PageIndex { get; set; }
    }
    public class CommonAddRedisReq
    {
        public long Id { get; set; }
        public long ArticleId { get; set; }
        public string Content { get; set; }
        public long LikesCount { get; set; }
        public long CreateTime { get; set; }
        public long Uid { get; set; }
    }

}
