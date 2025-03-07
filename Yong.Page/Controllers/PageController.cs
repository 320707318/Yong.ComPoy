using System.IdentityModel.Tokens.Jwt;
using DbModels.BaseModel;
using IPTools.Core;
using IPTools.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageUnitOfWork.Inferface;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace Yong.Page.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class PageController : ControllerBase
    {
        private readonly IPageUOW _pageUOW;
        private IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private int uid=0;
        private string IP;
        public PageController(IPageUOW pageUOW)
        {
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if(authHeader != null)
            {
                string tokenStr = authHeader.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var payload = handler.ReadJwtToken(tokenStr).Payload;
                var claims = payload.Claims;
                uid = int.Parse(claims.First(claim => claim.Type == "id").Value);
                //IP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString().Replace("::ffff:", "");
            }

            this._pageUOW = pageUOW;
        }
        [HttpPost]
        public async Task<DefaultMesRes> AddPage(PageAddReq addReq)
        {
            if(addReq.SuperTopics.Count >= 6)
            {
                throw new ArgumentException("标签或超话数量不能超过6个");
            }
            addReq.AuthorId = uid==0?addReq.AuthorId:uid;
           // addReq.Ip = IpTool.Search(IP).City=="内网IP"? IpTool.Search(addReq.Ip).City : IpTool.Search(IP).City;
            return await _pageUOW.AddPage(addReq);
        }
        [HttpGet]
        public async Task<PageRes> GetPage(long id)
        {
            return await _pageUOW.GetPage(id);
        }

        [HttpPost]
        public async Task<List<PageRes>> GetHotRecom([FromQuery] BasePagination pagination, PageHotReq pageHotReq)
        {
            pageHotReq.Uid = uid;
            pageHotReq.PageSize=pagination.PageSize;
            pageHotReq.PageIndex = pagination.PageIndex;
            return await _pageUOW.GetHotRecom(pageHotReq);
        }
        [HttpPost]
        public async Task<List<PageRes>> GetPageByIp([FromQuery] BasePagination pagination, PageIpReq req)
        {
            req.Ip = req.Ip; //IpTool.Search(req.Ip).City;
            req.Uid = uid;
            req.PageSize = pagination.PageSize;
            req.PageIndex = pagination.PageIndex;
            return await _pageUOW.GetPageByIp(req);
        }
        [HttpPost]
        public async Task<List<PageRes>> GetPageByTag([FromQuery] BasePagination pagination, PageTopicReq req)
        {
            req.Uid = uid;
            req.PageSize = pagination.PageSize;
            req.PageIndex = pagination.PageIndex;
            return await _pageUOW.GetPageByTag(req);
        }
        
        [HttpPut]
        public async Task<DefaultMesRes> PageLike(long pageId)
        {
            return await _pageUOW.PageLike(pageId, uid);
        }
        [HttpPut]
        public async Task<DefaultMesRes> PageDisLike(long pageId)
        {
            return await _pageUOW.PageDisLike(pageId, uid);
        }
        [HttpDelete]
        public async Task<DefaultMesRes> DeletePage(long pageId)
        {
            return await _pageUOW.DeletePage(pageId, uid);
        }
        [HttpPost]
        public async Task<DefaultMesRes> UpdatePage(PageUpdateReq updateReq)
        {
            updateReq.AuthorId = uid;

            return await _pageUOW.UpdatePage(updateReq);
        }
        [HttpGet]
        public async Task<List<string>> GetSerchKeyRank()
        {
            return await _pageUOW.GetSerchKeyRank();
        }
        [HttpPost]
        public async Task<List<PageRes>> GetPageByRealTime([FromQuery] BasePagination pagination, PageSearchReq req)
        {
            req.Uid = uid;
            req.PageSize = pagination.PageSize;
            req.PageIndex = pagination.PageIndex;
            return await _pageUOW.GetPageByRealTime(req);
        }
        [HttpPost]
        public async Task<List<PageRes>> GetPageIntegrated([FromQuery] BasePagination pagination, PageSearchReq req)
        {
            req.Uid = uid;
            req.PageSize = pagination.PageSize;
            req.PageIndex = pagination.PageIndex;
            return await _pageUOW.GetPageIntegrated(req);
        }
        [HttpPost]
        public async Task<List<PageRes>> GetPageByFollow([FromQuery]BasePagination pagination,PageSearchReq req)
        {
            req.PageSize = pagination.PageSize;
            req.PageIndex = pagination.PageIndex;
            req.Uid = uid;
            return await _pageUOW.GetPageByFollow(req);
        }
        [HttpPost]
        public async Task<List<PageRes>> GetPageByFriends([FromQuery]BasePagination pagination, PageSearchReq req)
        {
            req.PageSize = pagination.PageSize;
            req.PageIndex = pagination.PageIndex;
            req.Uid = uid;
            return await _pageUOW.GetPageByFriends(req);
        }
        [HttpPut]
        public async Task<string> UpLoadResource(IFormFile file)
        {
            return await _pageUOW.UpLoadResource(file);
        }
        [HttpPost]
        public async Task<List<PageRes>> GetPageAll([FromQuery]BasePagination pagination, PageSearchReq req)
        {
            req.Uid = uid;
            req.PageSize = pagination.PageSize;
            req.PageIndex = pagination.PageIndex;
            return await _pageUOW.GetPageAll(req);
        }
        [HttpPut]
        public async Task<bool> AccessPage(long id)
        {
            await _pageUOW.AccessPage(id, uid);
            return true;
        }
        [HttpGet]
        public async Task<List<PageRes>> GetPageByUid([FromQuery] BasePagination pagination,long id)
        {
            return await _pageUOW.GetPageByUid(pagination,id,uid);
        }

    }
}
