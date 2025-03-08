using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminUnitOfWork.Res;
using Middleware.Cap.Mq;

namespace AdminUnitOfWork
{
    public interface IAuditUOW
    {
        public Task<List<AuditRes>> GetAllAsync();
        public Task AuditMerchant(AuditMerchantMqDto req);
    }
}
