using AdminUnitOfWork;
using AdminUnitOfWork.Res;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
