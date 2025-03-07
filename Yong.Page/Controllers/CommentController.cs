using System.IdentityModel.Tokens.Jwt;
using DbModels.BaseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PageUnitOfWork.Inferface;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace Yong.Page.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentUOW _commentUOW;
        private IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private int uid;
        public CommentController(ICommentUOW commentUOW)
        {
            this._commentUOW = commentUOW; 
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string tokenStr = authHeader.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var payload = handler.ReadJwtToken(tokenStr).Payload;
            var claims = payload.Claims;
            uid = int.Parse(claims.First(claim => claim.Type == "id").Value);
        }
        [HttpPost]
        public async Task<List<CommentRes>> GetCommentByArticleId([FromQuery] BasePagination pagination, CommentReq req)
        {
            req.Uid = uid;
            req.PageIndex = pagination.PageIndex;
            req.PageSize = pagination.PageSize;
            return await _commentUOW.GetCommentByArticleId(req);
        }
        [HttpPut]
        public async Task<DefaultMesRes> AddComment(CommonAddReq addReq)
        {
            addReq.Uid = uid;
            return await _commentUOW.AddComment(addReq);
        }
        [HttpPut]
        public async Task<DefaultMesRes> LikeComment(long commentId)
        {
            return await _commentUOW.LikeComment(uid, commentId);
        }
        [HttpPut]
        public async Task<DefaultMesRes> UnLikeComment( long commentId)
        {
            return await _commentUOW.UnLikeComment(uid, commentId);
        }
        [HttpDelete]
        public async Task<DefaultMesRes> DeleteComment(long commentId)
        {
            return await _commentUOW.DeleteComment(uid, commentId);
        }
        [HttpGet]
        public async Task<long> GetCommentCount(long pageid)
        {
            return await _commentUOW.GetCommentCount(pageid);
        }
    }
}
