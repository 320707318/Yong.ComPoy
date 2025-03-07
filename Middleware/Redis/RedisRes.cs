using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Redis
{
    public class RedisRes
    {
    }
    public class RedisUserDataRes
    {
        public long followerCount { get; set; }
        public long fansCount { get; set; }
        public long friendCount { get; set; }

    }
    public class CommonRedisRes
    {
        public long Id { get; set; }
        public long ArticleId { get; set; }
        public string Content { get; set; }
        public long CreateTime { get; set; } 
        public long LikesCount { get; set; }
        public long Uid { get; set; }
        public bool IsLike { get; set;}
    }
}
