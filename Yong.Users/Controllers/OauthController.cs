using System.IdentityModel.Tokens.Jwt;
using DbModels.BaseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendProvide.QQEmali.Models;
using UserUnitOfWork.Interface;
using UserUnitOfWork.Request;
using UserUnitOfWork.Response;

namespace Yong.Users.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        private readonly IOauthUOW _userUOW;
        private readonly IUsersUOW _usersUOW;

        public OauthController(IOauthUOW userUOW,IUsersUOW usersUOW)
        {
            this._userUOW = userUOW;
            this._usersUOW = usersUOW;
        }
        [HttpPost]
        public async Task<LoginRes> Login(LoginReq req)
        {
            return await _userUOW.Login(req);
        }
        [HttpPost]
        public async Task<LoginRes> LoginAdmin(LoginReq req)
        {
            return await _userUOW.LoginAdmin(req);
        }
        [HttpPost]
        public async Task<RegisterRes> Register(RegisterReq req)
        {
            return await _userUOW.Register(req);
        }
        [HttpPost]
        public async Task<LoginRes> LoginByEmail(LoginByEmailReq req)
        {
            return await _userUOW.LoginByEmail(req);
        }
        [HttpPost]
        public async Task<LoginRes> LoginByQrCode(string qrcode)
        {
            return await _userUOW.LoginByQrCode(qrcode);
        }
        [HttpPost]
        public async Task<DefaultMesRes> SendLoginEmailCode(string email)
        {
            await _userUOW.SendLoginEmailCode(email);
            return new DefaultMesRes() { Code = 200,Message = "验证码已发送至邮箱" };
        }
        [HttpGet]
        public  string TestTT()
        {
            return "test";
        }
        [HttpPut]
        public async Task<string> AddQRCode(string? qrcode)
        {
            return await _userUOW.AddQRCode(qrcode);
        }
        [HttpPut]
        public async Task<int> CheckQRCode(string code)
        {
            return await _userUOW.CheckQRCode(code);
        }
        [HttpPut]
        public async Task<string> UploadResource(IFormFile file)
        {
            return await _usersUOW.UploadResource(file);
        }

    }
}
