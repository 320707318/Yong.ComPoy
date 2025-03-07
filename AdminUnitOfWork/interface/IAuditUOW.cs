using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminUnitOfWork.Res;

namespace AdminUnitOfWork
{
    public interface IAuditUOW
    {
        public Task<List<AuditRes>> GetAllAsync();
    }
}
