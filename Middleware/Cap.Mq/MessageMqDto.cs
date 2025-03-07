using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Cap.Mq
{
    public class MessageMqDto
    {

    }
    public class MetionMqDto
    {
        public long Uid { get; set; }
        public List<long> Ids { get; set; }
        public long PageId { get; set; }
        public string PageDesc { get; set; }
    }
}
