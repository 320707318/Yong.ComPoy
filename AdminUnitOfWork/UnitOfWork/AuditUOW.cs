using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminUnitOfWork.Req;
using AdminUnitOfWork.Res;
using DbModels;
using DbModels.Admin;
using DotNetCore.CAP;
using FreeSql;
using Middleware.Cap.Mq;
using Yong.Page.Api.DataContracts;
using static Yong.Page.Api.DataContracts.PageApi;

namespace AdminUnitOfWork.UnitOfWork
{
    public class AuditUOW : IAuditUOW
    {
        private readonly IBaseRepository<Audit> _repository;
        private readonly PageApiClient _pageApi;
        private readonly IBaseRepository<MerchantApplication> _mRepository;
        private readonly ICapPublisher _capPublisher;

        public AuditUOW(
            IBaseRepository<Audit> repository,
            PageApi.PageApiClient pageApi,
            IBaseRepository<MerchantApplication> mRepository,
            ICapPublisher  capPublisher)
        {
            this._repository = repository;
            this._pageApi = pageApi;
            this._mRepository = mRepository;
            this._capPublisher = capPublisher;
        }
        public async Task<List<AuditRes>> GetAllAsync()
        {
            var res = _pageApi.GetPageById(new GetPageReq { Id = 20 });
            return await _repository.Select.ToListAsync<AuditRes>();
        }
        public async Task AuditMerchant(AuditMerchantMqDto req)
        {
            await _capPublisher.PublishAsync("MerchantAudit", req);
        }
    }
}
