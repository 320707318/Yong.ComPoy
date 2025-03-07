using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels;
using DbModels.BaseModel;
using Microsoft.AspNetCore.Http;
using SendProvide.QQEmali.Models;
using UserUnitOfWork.Request;
using UserUnitOfWork.Response;

namespace UserUnitOfWork.Interface
{
    public interface IOauthUOW
    {
        public Task<LoginRes> Login(LoginReq req);
        public Task<RegisterRes> Register(RegisterReq req);
        public  Task<LoginRes> LoginAdmin(LoginReq req);
        public Task SendLoginEmailCode(string email);
        public  Task<LoginRes> LoginByEmail(LoginByEmailReq req);
        public Task<int> CheckQRCode(string code);
        public  Task<string> AddQRCode(string? hasCode);
        public Task<LoginRes> LoginByQrCode(string qrcode);

    }
}
