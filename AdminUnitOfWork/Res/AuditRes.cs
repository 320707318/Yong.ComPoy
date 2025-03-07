using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminUnitOfWork.Res
{
    public class AuditRes
    {
        public long Id { get; set; }
        public long PageId { get; set; }
        public long AdminId { get; set; }
        public bool IsAudited { get; set; }
        public long CreateTime { get; set; }
    }
}
