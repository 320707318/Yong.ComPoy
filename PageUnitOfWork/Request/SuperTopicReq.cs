using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;

namespace PageUnitOfWork.Request
{
    public class SuperTopicReq
    {

    }
    public class SuperTopicSearchReq:BasePagination
    {
        public string search { get; set; }
        public long type { get; set; }
    }
    public class SuperTopicIdsReq
    {
        public List<long> ids { get; set; }
    }
}
