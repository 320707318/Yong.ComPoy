using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.Cap.Mq;
using SendProvide.QQEmali.Models;

namespace Cap.EventBus
{
    public   interface IUserEventBus
    {
        public  Task BindEmail(string email);
        public Task SendBindCode(MailBindCode mbc);
    }
}
