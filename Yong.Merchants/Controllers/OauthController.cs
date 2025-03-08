using DbModels.BaseModel;
using MerchantsUnitOfWork;
using MerchantsUnitOfWork.Request;
using MerchantsUnitOfWork.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yong.Merchants.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        private readonly IOauthUOW _oauthUOW;

        public OauthController(IOauthUOW oauthUOW)
        {
            this._oauthUOW = oauthUOW;
        }

        [HttpPost]
        public async Task<string> UpLoadImage(IFormFile file) 
        {
            return await _oauthUOW.UpLoadImage(file);
        }

        [HttpPost]
        public async Task<DefaultMesRes<MerchantsLoginRes>> MerchantsLogin(MerchantsLoginReq req)
        {
            return await _oauthUOW.MerchantsLogin(req);
        }

        [HttpPost]
        public async Task<DefaultMesRes> SendRegEmailCode(string email)
        {
            await _oauthUOW.SendRegEmailCode(email);
            return new DefaultMesRes { Code = 200, Message = "发送成功" };
        }
        [HttpPost]
        public async Task<DefaultMesRes> MerchantsReg(MerchantsRegReq req)
        {
            return await _oauthUOW.MerchantsReg(req);
        }

    }
}
