using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels
{
    public class Page
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long id { get; set; }

        [Column(DbType = "text")]
        public string Content { get; set; }
        public string Title { get; set; }
        //当前时间戳秒
        public long CreateTime { get; set; }= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        //点赞

        public long LikeCount {  get; set; }
        //评论
        public long CommentCount { get; set; }
        //转发
        public long ForwardCount { get; set; }
        //收藏
        public long CollectCount { get; set; }
        //作者
        public long AuthorId { get; set; }
        public string Tags { get; set; }

        public string Cover { get; set; }

        public int CoverType { get; set; }
        public int Status { get; set; } = 0;

        public string Ip { get; set; } = "未知";

        [Navigate(nameof(AuthorId))]
        public Users Author { get; set; }
        //话题
        public string Topics { get; set; }
        //超话
        public string SuperTopics { get; set; }

        [Column(IsIgnore =true)]
        public List<Supers> SuperIds { get; set; }
        [Navigate(nameof(Resource.PageId))]
        public  List<Resource> Resources { get; set; }

    }
    public class Supers
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
