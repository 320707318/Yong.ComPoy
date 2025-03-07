using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminUnitOfWork.Res;
using DbModels;
using FreeSql;
using Yong.Page.Api.DataContracts;
using static Yong.Page.Api.DataContracts.PageApi;

namespace AdminUnitOfWork.UnitOfWork
{
    public class AuditUOW: IAuditUOW
    {
        private readonly IBaseRepository<Audit> _repository;
        private readonly PageApiClient _pageApi;

        public AuditUOW(IBaseRepository<Audit> repository,PageApi.PageApiClient pageApi)
        {
            this._repository = repository;
            this._pageApi = pageApi;
        }
        public async Task<List<AuditRes>> GetAllAsync()
        {
             var res= _pageApi.GetPageById(new GetPageReq { Id = 20 });
            return await _repository.Select.ToListAsync<AuditRes>();
        }
    }
}
