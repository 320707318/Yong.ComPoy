using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.Cap.Mq;

namespace Admin.EventBus
{
   public interface IAdminEventBus
    {
        public  Task AuditMerchantApplication(AuditMerchantMqDto mqDto);
        public Task AddMerchantApplication(MerchantMqDto mqDto);
    }
}
