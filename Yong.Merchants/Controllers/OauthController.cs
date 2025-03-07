using MerchantsUnitOfWork;
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
    }
}
