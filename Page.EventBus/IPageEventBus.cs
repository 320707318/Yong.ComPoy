using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.Cap.Mq;

namespace Page.EventBus
{
    public interface IPageEventBus
    {
        public  Task SetPageType(SetPageTagMqDto setPageTagMqDto);
    }
}
