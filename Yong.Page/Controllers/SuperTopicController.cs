using System.IdentityModel.Tokens.Jwt;
using DbModels.BaseModel;
using IPTools.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Cap.Mq;
using Middleware.Redis;
using PageUnitOfWork.Inferface;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace Yong.Page.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SuperTopicController : ControllerBase
    {
        private int uid;
        private IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private readonly ISuperTopicUOW _unitOfWork;

        public SuperTopicController(ISuperTopicUOW unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string tokenStr = authHeader.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var payload = handler.ReadJwtToken(tokenStr).Payload;
            var claims = payload.Claims;
            uid = int.Parse(claims.First(claim => claim.Type == "id").Value);

        }
        [HttpPost]
        public async Task<DefaultMesRes> AddSuperTopic(SuperTopicAddMqDto superTopic)
        {
            superTopic.CreatorId = uid;
            return await _unitOfWork.AddSuperTopic(superTopic);
        }
        [HttpGet]
        public async Task<SuperTopicRes> GetSuperTopic(long id)
        {
            return await _unitOfWork.GetSuperTopic(id,uid);
        }
        [HttpPost]
        public async Task<DefaultMesRes> AddSuperTopicType(SuperTopicTypeAddMqDto superTopicType)
        {
            return await _unitOfWork.AddSuperTopicType(superTopicType);
        }
        [HttpGet]
        public async Task<List<TypeRes>> GetSuperTopicType()
        {
            return await _unitOfWork.GetSuperTopicType();
        }
        [HttpGet]
        public async Task<List<string>> RedisGetSuperTopicType()
        {
            return (await RedisHelper.cli.LRangeAsync("SuperTopicType", 0, -1)).ToList();
        }
        [HttpPost]
        public async Task<List<SuperTopicRes>> GetSuperTopicSearch([FromQuery]BasePagination page, SuperTopicSearchReq super)
        {
            super.PageSize = page.PageSize;
            super.PageIndex = page.PageIndex;
            return await _unitOfWork.GetSuperTopicSearch(super,uid);
        }
        [HttpGet]
        public async Task<List<SuperTopicRes>> GetSuperTopByCreater()
        {
            return await _unitOfWork.GetSuperTopByCreater(uid);
        }
        [HttpGet]
        public async Task<List<SuperTopicRes>> GetSuperTopByJoin()
        {
            return await _unitOfWork.GetSuperTopByJoin(uid);
        }
        [HttpPut]
        public async Task<DefaultMesRes> JoinSuperTopic(long pageId)
        {
            return await _unitOfWork.JoinSuperTopic(pageId, uid);
        }
        [HttpPut]
        public async Task<DefaultMesRes> QuitSuperTopic(long pageId)
        {
            return await _unitOfWork.QuitSuperTopic(pageId, uid);
        }
        [HttpPut]
        public async Task<DefaultMesRes> DeleteSuperTopic(long pageId, long userId)
        {
            return await _unitOfWork.DeleteSuperTopic(pageId, uid);
        }
        [HttpPost]
        public async Task<List<SuperTopicSimpleRes>> GetSuperTopicByIds(SuperTopicIdsReq req)
        {
            return await _unitOfWork.GetSuperTopicByIds(req);
        }

    }
}
