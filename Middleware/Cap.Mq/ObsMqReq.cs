using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Cap.Mq
{
    public class ObsMqReq
    {
        public byte[] File { get; set; }
        public string Name { get; set; }
    }
}
