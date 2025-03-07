using System.IdentityModel.Tokens.Jwt;
using DbModels.BaseModel;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Cap.Mq;
using Middleware.Redis;
using SendProvide.QQEmali.Models;
using UserUnitOfWork.Interface;
using UserUnitOfWork.Request;
using UserUnitOfWork.Response;

namespace Yong.Users.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly IOauthUOW _userUOW;
        private readonly IUsersUOW _usersUOW;
        private readonly ICapPublisher _capPublisher;
        private IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private int uid;
        public UsersController(IOauthUOW userUOW,IUsersUOW usersUOW,ICapPublisher capPublisher)
        {
            this._userUOW = userUOW;
            this._usersUOW = usersUOW;
            this._capPublisher = capPublisher;
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string tokenStr = authHeader.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var payload = handler.ReadJwtToken(tokenStr).Payload;
            var claims = payload.Claims;
            uid = int.Parse(claims.First(claim => claim.Type == "id").Value);
        }
        [HttpGet]
        
        public Task<UserRes> GetUserById(int id)
        {
            return _usersUOW.GetUserById(id,uid);
        }
        [HttpPost]
        public async Task<List<UserSimpleRes>> SearchSimplesUser(long id)
        {
            return await _usersUOW.SearchSimplesUser(uid);
        }
        [HttpGet]
        public async Task<List<UserRes>> SearchUser([FromQuery]BasePagination page, string keyWord)
        {
            return await _usersUOW.SearchUser(page, keyWord, uid);
        }
        [HttpGet]
        public async Task<List<UserRes>> GetSuperTopicMembers(long id)
        {
            return await _usersUOW.GetSuperTopicMembers(id);
        }
        [HttpPost]
        public async Task<EmailBindRes> BindEmail(EmaliBindReq req)
        {
            req.Uid = uid;

            var res= await _usersUOW.BindEmail(req);
            return res;
        }
        [HttpPost]
        public async Task SendEmaliCode(string email)
        {
            await _usersUOW.SendEmailCode(new MailRequest
            {
                ToEmail = email,
                Subject = "Yong.ComPoy 邮箱验证",
            });
        }
        [HttpPost]
        public async Task<DefaultMesRes> EditUser(UserEditReq editRes)
        {
            editRes.Uid = uid;
            return await _usersUOW.EditUser(editRes);
        }
        [HttpPost]
        public async Task SendForgetCode(string email)
        {
            await _usersUOW.SendForgetPasswordCode(new MailRequest
            {
                ToEmail = email,
                Subject = "Yong.ComPoy 重置密码",
            });
        }
        [HttpPost]
        public async Task<DefaultMesRes> EditPassWord(UserChangePasswordReq req)
        {
            return await _usersUOW.EditPassWord(req);
        }
        [HttpPost]
        public async Task<List<UserSimpleRes>> UserSimples(UserSearchReq req)
        {
            return await _usersUOW.UserSimples(req);
        }
        [HttpGet]
        public async Task<List<UserRes>> GetFans()
        {
            return await _usersUOW.GetFans(uid);
        }
        [HttpGet]
        public async Task<List<UserRes>> GetFollows()
        {
            return await _usersUOW.GetFollows(uid);
        }
        [HttpGet]
        public async Task<RedisUserDataRes> GetUserData(long id)
        {
            return await _usersUOW.GetUserData(id>0?id:uid);
        }
        [HttpGet]
        public async Task<List<UserRes>> GetFriends( )
        {
            return await _usersUOW.GetFriends(uid);
        }
        [HttpPut]
        public async Task<DefaultMesRes> Follow(int fid)
        {
            if(await RedisHelper.IsFollowingAsync(uid, fid))
            {
                return new DefaultMesRes { Code = 400, Message = "已关注" };
            }
            await RedisHelper.FollowAsync(uid, fid);
            await _capPublisher.PublishAsync("Message.Follow",new FollowMqDto { Uid=uid, FollowUid=fid });
            return new DefaultMesRes { Code = 200, Message = "关注成功" };
        }
        [HttpPut]
        public async Task<DefaultMesRes> UnFollow(int fid)
        {
            if(!await RedisHelper.IsFollowingAsync(uid, fid))
            {
                return new DefaultMesRes { Code = 400, Message = "未关注" };
            }
            await RedisHelper.UnfollowAsync(uid, fid);
            return new DefaultMesRes { Code = 200, Message = "取消关注成功" };
        }
        [HttpPost]
        public async Task<string> UpLoadAvatar(IFormFile file)
        {
            return await _usersUOW.UpLoadAvatar(file);
        }
        [HttpPost]
        public async Task<string> UplodSound(IFormFile file)
        {
            return await _usersUOW.UplodSound(file);
        }
        [HttpPost]
        public async Task<string> UpLoadImage(IFormFile file)
        {
            return await _usersUOW.UpLoadImage(file);
        }
        [HttpPost]
        public async Task<string> UplodBg(IFormFile file)
        {
            return await _usersUOW.UplodBg(file);
        }
        [HttpPost]
        public async Task<string> UploadResource(IFormFile file)
        {
            return await _usersUOW.UploadResource(file);
        }
        [HttpPut]
        public async Task ScanLogin(string qrcode)
        {
             await _usersUOW.ScanLogin(uid,qrcode);
        }
        [HttpPut]
        public async Task ScanQrCode(string qrcode)
        {
            await _usersUOW.ScanQrCode(qrcode);
        }
    }
}
