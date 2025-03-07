using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Cap.Mq
{
    internal class CommonMqDto
    {
    }
    public class RedisSetMqDto
    {
      public string Key { get; set; }
      public string Value { get; set; }
      public int ExpireTime { get; set; } = -1;
    }
    public class DeleteCommentMqDto
    {
        public long CommentId { get; set; }
        public long Uid { get; set; }
    }
}
