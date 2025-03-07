using System.Text.Json;
using DbModels;
using DotNetCore.CAP;
using FreeSql;
using Middleware.Cap.Mq;
using Middleware.Redis;
using RestSharp;
using SendProvide.QQEmali;
using SendProvide.QQEmali.Models;
using UserUnitOfWork.Interface;
using UserUnitOfWork.Request;
using UserUnitOfWork.Response;
using Yitter.IdGenerator;
namespace UserUnitOfWork.UnitOfWokes
{
    public class OauthUOW : IOauthUOW
    {
        private readonly IBaseRepository<Users> _usersRepository;
        private readonly IMailService _mailService;
        private readonly ICapPublisher _capPublisher;

        public OauthUOW(IBaseRepository<Users> UsersRepository, IMailService mailService, ICapPublisher capPublisher)
        {
            this._usersRepository = UsersRepository;
            this._mailService = mailService;
            this._capPublisher = capPublisher;
        }
        
        public async Task<LoginRes> Login(LoginReq req)
        {
            using(var uow = _usersRepository.Orm.CreateUnitOfWork())
            {
                var user = await uow.GetRepository<Users>().Select.Where(a => a.UserName == req.UserName).FirstAsync();
                if(user == null)
                {
                    return new LoginRes { Code = 401, Message = "用户名或密码错误" };
                }
                if(user.Password != req.Password)
                {
                    return new LoginRes { Code = 402, Message = "用户名或密码错误" };
                }
                if(user.IsDeleted == 1)
                {
                    return new LoginRes { Code = 403, Message = "用户已被删除" };
                }
                using(var client = new RestClient("http://localhost:5001/auth/connect/token"))
                {
                    var request = new RestRequest();
                    request.AddHeader("Accept", "*/*");
                    request.AddHeader("Host", "localhost:5001");
                    request.AddHeader("Connection", "keep-alive");
                    request.AddParameter("grant_type", "password");
                    request.AddParameter("client_id", "web_client");
                    request.AddParameter("client_secret", "123456");
                    request.AddParameter("username", user.Id.ToString());
                    request.AddParameter("password", user.Role == 1 ? "Admin" : "User");
                    RestResponse response = await client.PostAsync(request);
                    var content = response.Content.Split(",")[0];
                    var accessToken = content.Substring(content.IndexOf("access_token") + 15).Replace("\"", "").Replace("\\", "").Trim();
                    
                    return new LoginRes
                    {
                        Code = 200,
                        Message = "登录成功",
                        Token = accessToken,
                        User = new UserRes
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            Phone = user.Phone,
                            Avatar = user.Avatar,
                            Gender = user.Gender,
                            NickName = user.NickName,
                        }
                    };
                }
            }

        }

        public async Task<LoginRes> LoginByQrCode(string qrcode)
        {
            var uid=( await RedisHelper.cli.GetAsync(qrcode)).Split(",")[1];
            var user = await _usersRepository.Select.Where(a => a.Id == int.Parse(uid)).FirstAsync();
            using(var client = new RestClient("http://localhost:5001/auth/connect/token"))
            {
                var request = new RestRequest();
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Host", "localhost:5001");
                request.AddHeader("Connection", "keep-alive");
                request.AddParameter("grant_type", "password");
                request.AddParameter("client_id", "web_client");
                request.AddParameter("client_secret", "123456");
                request.AddParameter("username", user.Id.ToString());
                request.AddParameter("password", user.Role == 1 ? "Admin" : "User");
                RestResponse response = await client.PostAsync(request);
                var content = response.Content.Split(",")[0];
                var accessToken = content.Substring(content.IndexOf("access_token") + 15).Replace("\"", "").Replace("\\", "").Trim();

                return new LoginRes
                {
                    Code = 200,
                    Message = "登录成功",
                    Token = accessToken,
                    User = new UserRes
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Phone = user.Phone,
                        Avatar = user.Avatar,
                        Gender = user.Gender,
                        NickName = user.NickName,
                    }
                };
            }
        }
        public async Task<LoginRes> LoginAdmin(LoginReq req)
        {
            using(var uow = _usersRepository.Orm.CreateUnitOfWork())
            {
                var user = await uow.GetRepository<Users>().Select.Where(a => a.UserName == req.UserName).FirstAsync();
                if(user == null)
                {
                    return new LoginRes { Code = 401, Message = "用户名或密码错误" };
                }
                if(user.Password != req.Password)
                {
                    return new LoginRes { Code = 402, Message = "用户名或密码错误" };
                }
                if(user.Role <1)
                {
                    return new LoginRes { Code = 403, Message = "用户权限不足" };
                }
                using(var client = new RestClient("http://localhost:5001/auth/connect/token"))
                {
                    var request = new RestRequest();
                    request.AddHeader("Accept", "*/*");
                    request.AddHeader("Host", "localhost:5001");
                    request.AddHeader("Connection", "keep-alive");
                    request.AddParameter("grant_type", "password");
                    request.AddParameter("client_id", "web_client");
                    request.AddParameter("client_secret", "123456");
                    request.AddParameter("username", user.Id.ToString());
                    request.AddParameter("password", user.Role == 1 ? "Admin" : "User");
                    RestResponse response = await client.PostAsync(request);
                    var content = response.Content.Split(",")[0];
                    var accessToken = content.Substring(content.IndexOf("access_token") + 15).Replace("\"", "").Replace("\\", "").Trim();

                    return new LoginRes
                    {
                        Code = 200,
                        Message = "登录成功",
                        Token = accessToken,
                        User = new UserRes
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            Phone = user.Phone,
                            Avatar = user.Avatar,
                            Gender = user.Gender,
                            NickName = user.NickName,
                        }
                    };
                }
            }

        }
        public async Task<RegisterRes> Register(RegisterReq req)
        {
            var any = await _usersRepository.Select.Where(a => a.UserName == req.UserName).FirstAsync();
            if(any != null)
            {
                return new RegisterRes { Code = 401, Message = "用户名已存在" };
            }
            var user = new Users
            {
                UserName = req.UserName,
                Password = req.Password,
                Phone = req.Phone,
                Avatar = req.Avatar??"https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/V1Image/icx0y9ngsea1.jpg",
                Gender = req.Gender,
                NickName = req.NickName,
            };
            var res= await _usersRepository.InsertAsync(user);
            await _capPublisher.PublishAsync("Add.UserDb",new RedisSetMqDto { 
                Key = "UserDb."+ res.Id.ToString(),
                Value= JsonSerializer.Serialize(user)
            });
            return new RegisterRes
            {
                Code = 200,
                Message = "注册成功",
                User = new UserRes
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    Gender = user.Gender,
                    NickName = user.NickName,
                }
            };
        }

        public async Task<LoginRes> LoginByEmail(LoginByEmailReq req)
        {
            var user = await _usersRepository.Select.Where(a => a.Email == req.Email).FirstAsync();
            if(user == null)
            {
                return new LoginRes { Code = 401, Message = "邮箱或验证码错误" };
            }
            if(req.EmailCode != await RedisHelper.cli.GetAsync("Login.Email.Code"+req.Email))
            {
                return new LoginRes { Code = 401, Message = "邮箱或验证码错误" };
            }
            using(var client = new RestClient("http://localhost:5001/auth/connect/token"))
            {
                var request = new RestRequest();
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Host", "localhost:5001");
                request.AddHeader("Connection", "keep-alive");
                request.AddParameter("grant_type", "password");
                request.AddParameter("client_id", "web_client");
                request.AddParameter("client_secret", "123456");
                request.AddParameter("username", user.Id.ToString());
                request.AddParameter("password", user.Role == 1 ? "Admin" : "User");
                RestResponse response = await client.PostAsync(request);
                var content = response.Content.Split(",")[0];
                var accessToken = content.Substring(content.IndexOf("access_token") + 15).Replace("\"", "").Replace("\\", "").Trim();
                return new LoginRes
                {
                    Code = 200,
                    Message = "登录成功",
                    Token = accessToken,
                    User = new UserRes
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Phone = user.Phone,
                        Avatar = user.Avatar,
                        Gender = user.Gender,
                        NickName = user.NickName,
                    }
                };
            }

         }

        public async Task SendLoginEmailCode(string email)
        {
            //保存验证码到freeredis
            var EX_Time = await RedisHelper.cli.TtlAsync("Login.Email.Code" + email);
            if(EX_Time <= 60 * 2)
            {
                //随机6位验证码
                var code = new Random().Next(100000, 999999).ToString();
                var user=await _usersRepository.Select.Where(a => a.Email == email).FirstAsync();
                if(user == null)
                {
                    throw new Exception("邮箱不存在");
                }
                var que = new MailLoginCode
                {
                    mail = new MailRequest { ToEmail = email , Subject = "登录验证码", Body =  EmaliTemplet.GetTemplet(int.Parse(code),0,user.NickName) },
                    code = code
                };
                await _capPublisher.PublishAsync("Login.Email.Code", que);
            }
            else
            {
                throw new Exception("验证码发送过于频繁，请稍后再试");
            }
        }

        public async Task<string> AddQRCode(string? hasCode)
        {   
            if(hasCode == null)
            {
                await RedisHelper.cli.UnLinkAsync(hasCode);
            }
            string code = "qrcode:" + YitIdHelper.NextId();
            await RedisHelper.cli.SetAsync(code,0, 60);
            return code;
        }
        public async Task<int> CheckQRCode(string code)
        {
            var res = await RedisHelper.cli.GetAsync(code);
            if(res == null)
            {
                return -1;
            }
            var status= int.Parse(res.Split(",")[0]);
            return status;
        }
    }
}
