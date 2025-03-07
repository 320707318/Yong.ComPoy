using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels
{
    public class Resource
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public long PageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } 
        public string Description { get; set; }=string.Empty;
        public int Type { get; set; } = 1;
        public int Sort { get; set; }
        public int Status { get; set; } = 1;
        public long CreateTime { get; set; }= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        //public Page Page { get; set; }
    }
}
