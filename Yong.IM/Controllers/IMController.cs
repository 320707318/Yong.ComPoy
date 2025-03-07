using System.IdentityModel.Tokens.Jwt;
using DbModels.BaseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.FreeIm;

namespace Yong.IM.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class IMController : ControllerBase
    {
        private IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private int uid;
        public IMController()
        {
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string tokenStr = authHeader.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var payload = handler.ReadJwtToken(tokenStr).Payload;
            var claims = payload.Claims;
            uid = int.Parse(claims.First(claim => claim.Type == "id").Value);
        }
        [HttpGet]
        public  string GetUrl()
        {
            var url = ImHelper.PrevConnectServer(uid, "");
            return url.Replace("127.0.0.1", "192.168.0.100");
        }
        [HttpGet]
        public List<long> ImGet()
        {
            ImHelper.SendMessage(0, new List<long> { 1 }, "Hello");

            return ImHelper.GetClientListByOnline().ToList();
        }
        [HttpPost]
        public void ImSend()
        {
            ImHelper.SendMessage(uid, new List<long> { uid }, "hello");
        }
        //public DefaultMesRes ImSend(ImReq imReq)
        //{

        //}
    }
}
