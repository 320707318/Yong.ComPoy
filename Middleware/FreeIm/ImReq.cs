using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.FreeIm
{
    public class ImReq
    {
        public ImReqType type { get; set; }
        public string content { get; set; }
        
    }
    public  enum ImReqType
    {
        Text,
        Image,
        Fefer,
        Page,
        Metion,
        Sound,
        Video,
    }
}
