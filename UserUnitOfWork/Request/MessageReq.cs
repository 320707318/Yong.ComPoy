using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.FreeIm;

namespace UserUnitOfWork.Request
{
    public class MessageReq
    {
        
    }
    public class PrivateLetterReq
    {
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public ImReq Content { get; set; }
    }
    public class MetionReq
    {
        public long SenderId { get; set; }
        public ImReq Content { get; set; }
    }
    public class AiReq
    {
        public long SenderId { get; set; }
        public string Content { get; set; }
    }
}
