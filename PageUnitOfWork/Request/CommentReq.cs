using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;

namespace PageUnitOfWork.Request
{
    public class CommentReq:BasePagination
    {
        public long ArticleId { get; set; }
        public long Uid { get; set; }
    }
    public class CommonAddReq
    {
        public long ArticleId { get; set; }
        public string Content { get; set; }
        public long Uid { get; set; }
    }

}
