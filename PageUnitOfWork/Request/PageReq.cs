using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;
using Middleware.Cap.Mq;

namespace PageUnitOfWork.Request
{
    public class PageReq
    {
    }
    public class PageAddReq
    {
        public string Content { get; set; }
        public string Title { get; set; }
        //作者
        public long AuthorId { get; set; }
        //话题
        public List<string> Topics { get; set; }
        //超话
        public string Ip { get; set; } = "未知";
        public List<long> SuperTopics { get; set; }
        public string Cover { get; set; }

        public int CoverType { get; set; }
        public List< ResouseAddReq> Resources { get; set; }
    }
    public class ResouseAddReq
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; } = 1;
        public int Sort { get; set; }
    }
    public class PageHotReq:BasePagination
    {
        public long Uid { get; set; }
    }
    public class PageTopicReq : BasePagination
    {
        public long Uid { get; set; }
        public long TopicId { get; set; }
    }
    public class PageIpReq:BasePagination
    {
        public long Uid { get; set; }
        public string Ip { get; set; }
    }
    public class PageSearchReq : BasePagination
    {
        public string KeyWord { get; set; }
        public long Uid { get; set; }
    }
    public class PageUpdateReq: UpdatePageMqDto
    {
        
    }
}
