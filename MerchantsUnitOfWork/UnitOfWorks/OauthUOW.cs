using Admin.DataContracts;
using DbModels.BaseModel;
using DbModels.merchants;
using DotNetCore.CAP;
using FreeSql;
using MerchantsUnitOfWork.Request;
using MerchantsUnitOfWork.Response;
using Microsoft.AspNetCore.Http;
using Middleware.Cap.Mq;
using Middleware.OBS;
using Middleware.Redis;
using Org.BouncyCastle.Ocsp;
using RestSharp;
using SendProvide.QQEmali.Models;

namespace MerchantsUnitOfWork.UnitOfWorks
{
    public class OauthUOW : IOauthUOW
    {
        private readonly IBaseRepository<Merchants> _repository;
        private readonly ICapPublisher _capPublisher;
        private readonly AdminApi.AdminApiClient _adminApi;

        public OauthUOW(IBaseRepository<Merchants> repository, ICapPublisher capPublisher, AdminApi.AdminApiClient adminApi)
        {
            this._repository = repository;
            this._capPublisher = capPublisher;
            this._adminApi = adminApi;
        }
        public async Task<string> UpLoadImage(IFormFile file)
        {
            if(file.ContentType != "image/jpeg" && file.ContentType != "image/png")
            {
                throw new Exception("图片格式不正确");
            }
            if(file.Length > 1024 * 1024 * 10)
            {
                throw new Exception("图片大小不能超过10M");
            }
            using var stream = file.OpenReadStream();
            var name = "Merchants/" + DateTimeOffset.Now.ToUnixTimeSeconds() + file.FileName;
            await ObsHelper.UploadImageFile(stream, name);
            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;
        }

        public async Task<DefaultMesRes<MerchantsLoginRes>> MerchantsLogin(MerchantsLoginReq req)
        {
            var user = await _repository
                .Where(m => m.Email == req.Email && m.PassWord == req.PassWord)
                .FirstAsync();
            if(user == null)
            {
                return new DefaultMesRes<MerchantsLoginRes> { Code = 401, Message = "用户名或密码错误", Data = new MerchantsLoginRes() };
            }
            using var client = new RestClient("http://localhost:5001/auth/connect/token");
            var request = new RestRequest();
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Host", "localhost:5001");
            request.AddHeader("Connection", "keep-alive");
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", "web_client");
            request.AddParameter("client_secret", "123456");
            request.AddParameter("username", user.Id.ToString());
            request.AddParameter("password", "Merchants");
            RestResponse response = await client.PostAsync(request);
            var content = response.Content.Split(",")[0];
            var accessToken = content.Substring(content.IndexOf("access_token") + 15).Replace("\"", "").Replace("\\", "").Trim();
            return new DefaultMesRes<MerchantsLoginRes>
            {
                Code = 200,
                Message = "登录成功",
                Data = new MerchantsLoginRes
                {
                    Id = user.Id,
                    ShopName = user.ShopName,
                    Avatar = user.Avatar,
                    Description = user.Description,
                    Email = user.Email,
                    UserName = user.UserName,
                    Token = accessToken
                }
            };
        }

        public async Task<DefaultMesRes> MerchantsReg(MerchantsRegReq req)
        {
            var mreg =await  _adminApi.GetMerchantByIdAsync(new GetMerchantIdReq { Email = req.Email });
            if(mreg.Id != 0)
            {
                return new DefaultMesRes { Code = 401, Message = "已有相同申请" };
            }
            await _capPublisher.PublishAsync("Merchant.Application.Add", new MerchantMqDto
            {
                Email = req.Email,
                PassWord = req.PassWord,
                Description = req.Description,
                ShopName = req.ShopName,
                IDCardPhoto = req.IDCardPhoto,
                BusinessLicense = req.BusinessLicense,
                signature = req.signature
            });
            return new DefaultMesRes { Code = 200, Message = "申请成功" };

        }

        public async Task SendRegEmailCode(string email)
        {
            //保存验证码到freeredis
            var EX_Time = await RedisHelper.cli.TtlAsync("Reg.Email.Code" + email);
            if(EX_Time <= 60 * 2)
            {
                //随机6位验证码
                var code = new Random().Next(100000, 999999).ToString();
                var user = await _repository.Where(a => a.Email == email).FirstAsync();
                if(user != null)
                {
                    throw new Exception("邮箱已被使用");
                }
                var que = new MailLoginCode
                {
                    mail = new MailRequest { ToEmail = email, Subject = "申请验证码", Body = EmaliTemplet.GetTemplet(int.Parse(code), 0,"商家") },
                    code = code
                };
                await _capPublisher.PublishAsync("Reg.Email.Code", que);
            }
            else
            {
                throw new Exception("验证码发送过于频繁，请稍后再试");
            }
        }
    }
}
