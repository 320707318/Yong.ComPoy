using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserUnitOfWork.Request;

namespace UserUnitOfWork.Interface
{
    public interface IMessageUOW
    {
        public Task SendPrivateLetter(PrivateLetterReq req);
        public Task<string> SendAi(AiReq req);
    }
}
