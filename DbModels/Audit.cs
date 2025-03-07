using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels
{
    public class Audit
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public long PageId { get; set; }
        public long AdminId { get; set; }
        public bool IsAudited { get; set; }
        public long CreateTime { get; set; }
    }
}
