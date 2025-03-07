using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yong.Merchants.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        public OauthController() { }

        [HttpGet]
        public string GetAll()
        {
            return "This is a test";
        }
    }
}
