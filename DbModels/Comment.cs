using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels
{
    public class Comment
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public long ArticleId { get; set; }
        [Column(DbType = "text")]
        public string Content { get; set; }
        public long CreateTime { get; set; }= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public long LikesCount { get; set; }
        public long Uid { get; set; }
    }
}
