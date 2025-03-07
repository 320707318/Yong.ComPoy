using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Cap.Mq
{
    public class PageMqDto
    {
    }
    public class SetPageTagMqDto
    {
        public List<string> Urls { get; set; }
        public long PageId { get; set; }
    }
    public class LikePageMqDto
    {
        public long PageId { get; set; }
        public long Uid { get; set; }
    }
    public class DeletePageMqDto
    {
        public long PageId { get; set; }
        public long Uid { get; set; }
    }
    public class UpdatePageMqDto
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public long AuthorId { get; set; }
        public string Title { get; set; }
        //话题
        public List<string> Topics { get; set; }
        //超话
        public List<long> SuperTopics { get; set; }

        public string Cover { get; set; }

        public int CoverType { get; set; }

        public List<ResouseUpdateReq> Resources { get; set; }
    }
    public class ResouseUpdateReq
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; } = 1;
        public int Sort { get; set; }
    }
}
