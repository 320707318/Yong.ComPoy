using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels;
using Middleware.Redis;

namespace PageUnitOfWork.Response
{
    public class PageRes
    {
        public long id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public long CreateTime { get; set; } = DateTime.Now.ToBinary();
        //点赞
        public long LikeCount { get; set; }
        //评论
        public long CommentCount { get; set; }
        //转发
        public long ForwardCount { get; set; }
        //收藏
        public long CollectCount { get; set; }
        //作者
        public int Status { get; set; } = 0;
        public string Ip { get; set; }
        public bool IsLike { get; set; }
        public long AuthorId { get; set; }
        public string Cover { get; set; }
        public string Tags { get; set; }
        public int CoverType { get; set; } = 0;
        public UserRes Author { get; set; }
        public List<Supers> SuperIds { get; set; }
        public List<ResourceRes> Resources { get; set; }
    }
    public class TypeRes
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; } = 1;
    }
    public class ResourceRes
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; }
        public int Type { get; set; } = 1;
        public int Sort { get; set; }
        public int Status { get; set; } = 1;
    }
    public class UserRes
    {
        public long id { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int Gender { get; set; }
        public bool IsAttention { get; set; }
    }
    public class CommentUserRes
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public int Gender { get; set; } = 0;
    }
    public class CommentRes
    {
        public CommonRedisRes common     { get; set; }
        public CommentUserRes user               { get; set; }
    }
}
