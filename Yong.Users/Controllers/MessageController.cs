using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Redis;
using UserUnitOfWork.Interface;
using UserUnitOfWork.Request;

namespace Yong.Users.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageUOW _unitOfWork;
        private IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private int uid;
        public MessageController(IMessageUOW unitOfWork)
        {
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string tokenStr = authHeader.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var payload = handler.ReadJwtToken(tokenStr).Payload;
            var claims = payload.Claims;
            uid = int.Parse(claims.First(claim => claim.Type == "id").Value);
            this._unitOfWork = unitOfWork;
        }
        [HttpPost]
        public async Task SendPrivateLetter(PrivateLetterReq req)
        {
            req.SenderId = uid;
            await _unitOfWork.SendPrivateLetter(req);
        }
        [HttpPost]
        public async Task<string> SendAiMessage(AiReq req)
        {
            req.SenderId = uid;
             return await _unitOfWork.SendAi(req);
        }
        [HttpGet]
        public async Task<long> GetAiCount()
        {
            return await RedisAi.GetAiCount(uid);
        }
    }
}
