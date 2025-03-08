using AdminUnitOfWork;
using AdminUnitOfWork.Res;
using DbModels.BaseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Cap.Mq;

namespace Yong.Admin.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditUOW _audit;

        public AuditController(IAuditUOW audit)
        {
            this._audit = audit;
        }
        [HttpGet]
        public async Task<List<AuditRes>> GetAllAsync()
        {
            return await _audit.GetAllAsync();
        }
        [HttpPost]
        public async Task<DefaultMesRes> AuditMerchant(AuditMerchantMqDto req)
        {
            await _audit.AuditMerchant(req);
            return new DefaultMesRes
            {
                Code = 200,
                Message = "审核成功"
            };
        }
    }
}
