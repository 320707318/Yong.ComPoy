using Admin.DataContracts;
using DbModels.Admin;
using FreeSql;
using Grpc.Core;
using static Admin.DataContracts.AdminApi;

namespace AdminUnitOfWork.gRpcServices
{
    public class AdminService : AdminApiBase
    {
        private readonly IBaseRepository<MerchantApplication> _repository;

        public AdminService(IBaseRepository<MerchantApplication> repository)
        {
            this._repository = repository;
        }
        public override async Task<MerchantsRes> GetMerchantById(GetMerchantIdReq request, ServerCallContext context)
        {
            var merchant =await _repository.Where(s => s.Email == request.Email || s.Status!=3).FirstAsync<MerchantsRes>();
            return merchant?? new MerchantsRes { };
        }
    }
}
