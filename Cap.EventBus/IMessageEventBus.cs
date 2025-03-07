using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.FreeIm;

namespace Cap.EventBus
{
    public interface IMessageEventBus
    {
        public Task AddMessageToBeSent(ImRes message);
    }
}
