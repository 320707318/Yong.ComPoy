
using System.Text.Json;
using DbModels;
using DbModels.BaseModel;
using DotNetCore.CAP;
using FreeSql;
using Microsoft.AspNetCore.Http;
using Middleware.Cap.Mq;
using Middleware.ES;
using Middleware.OBS;
using Middleware.Redis;
using Nest;
using RestSharp;
using SendProvide.QQEmali.Models;
using UserUnitOfWork.Interface;
using UserUnitOfWork.Request;
using UserUnitOfWork.Response;

namespace UserUnitOfWork.UnitOfWokes
{
    public class UsersUOW : IUsersUOW
    {
        private readonly IBaseRepository<Users> _usersRepository;
        private readonly ICapPublisher _capPublisher;
        private readonly ElasticClient _elasticsearch;
        private readonly IFreeSql _free;

        public UsersUOW(IBaseRepository<Users> UsersRepository, ICapPublisher capPublisher, ElasticClient elasticsearch, IFreeSql free)
        {
            this._usersRepository = UsersRepository;
            this._capPublisher = capPublisher;
            this._elasticsearch = elasticsearch;
            this._free = free;
        }
        #region 查询
        /// <summary>
        /// 根据Id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserRes> GetUserById(int id, long uid)
        {
            var udt = await RedisHelper.cli.GetAsync("UserDb." + id.ToString());
            var user = udt == null ? null : JsonSerializer.Deserialize<UserRes>(udt);
            if(user == null)
            {
                user = await _usersRepository.Select
                .Where(a => a.Id == id)
                .FirstAsync(s => new UserRes
                {
                    Id = s.Id,
                    UserName = s.UserName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Profile = s.Profile,
                    IsOnLine = ImHelper.HasOnline(id),
                    Avatar = s.Avatar,
                    Gender = s.Gender,
                    NickName = s.NickName,
                    Banner = s.Banner
                });
                await _capPublisher.PublishAsync("Add.UserDb", new RedisSetMqDto { Key = "UserDb." + id.ToString(), Value = JsonSerializer.Serialize(user) });
            }
            user.IsOnLine = ImHelper.HasOnline(id);
            user.IsFollowed = await RedisHelper.IsFollowingAsync(uid, user.Id);
            return user;
        }
        public async Task<List<UserRes>> SearchUser(BasePagination page, string keyWord, long uid)
        {
            var res = _elasticsearch.Search<UserEs>(s =>
            s.Index("user")
            .Query(q => q
                        .Bool(b => b
                            .Should(
                               sh => sh.Match(m => m.Field(f => f.UserName).Query(keyWord)),
                               sh => sh.Match(m => m.Field(f => f.NickName).Query(keyWord))
                               )
                            )
                        )
                .From((page.PageIndex - 1) * page.PageSize)
                .Size(page.PageSize)
            );
            var ids = res.Documents.Select(a => a.Id).ToList();
            var users = new List<UserRes>();
            foreach(var id in ids)
            {
                var db = await RedisHelper.cli.GetAsync("UserDb." + id.ToString());
                var user = JsonSerializer.Deserialize<UserRes>(db);
                user.FansCount = (await RedisHelper.GetFansAsync(id)).Count;
                user.IsFollowed = await RedisHelper.IsFollowingAsync(uid, id);
                users.Add(user);
            }
            return users;
        }
        public async Task<List<UserSimpleRes>> SearchSimplesUser(long uid)
        {
            var followIds = await RedisHelper.GetFollowersAsync(uid);
            if(followIds.Count == 0)
            {
                return new List<UserSimpleRes>();
            }
            var res = _elasticsearch.Search<UserEs>(s =>
                s.Index("user")
                .Query(q => q
                         .Terms(t => t
                        .Field(f => f.Id)
                        .Terms(followIds))
                 )
            );
            var ids = res.Documents.Select(s => new UserSimpleRes { Id = s.Id, UserName = s.UserName, Avatar = s.Avatar, NickName = s.NickName }).ToList();
            return ids;
        }
        public async Task<List<UserRes>> GetSuperTopicMembers(long id)
        {
            var uids = await RedisHelper.GetTopicMembersAsync(id);
            var res = new List<UserRes>();
            foreach(var uid in uids)
            {
                var db = await RedisHelper.cli.GetAsync("UserDb." + uid.ToString());
                var user = JsonSerializer.Deserialize<UserRes>(db);
                res.Add(user);
            }
            return res;
        }
        public async Task<List<UserSimpleRes>> UserSimples(UserSearchReq req)
        {
            return await _usersRepository.Select.
                Where(a => a.UserName.Contains(req.KeyWord) || a.NickName.Contains(req.KeyWord))
                .OrderByDescending(a => a.FansCount)
                .Page(req.PageIndex, req.PageSize)
                .ToListAsync(s => new UserSimpleRes
                {
                    Id = s.Id,
                    UserName = s.UserName,
                    Avatar = s.Avatar,
                    Gender = s.Gender,
                    NickName = s.NickName,
                });
        }
        /// <summary>
        /// 获取粉丝列表
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<List<UserRes>> GetFans(long uid)
        {
            var uids = await RedisHelper.GetFansAsync(uid);
            var userindex = await _elasticsearch.SearchAsync<UserEs>(s => s.Index("user").Query(q => q.Terms(t => t.Field(f => f.Id).Terms(uids))));
            var users = new List<UserRes>();
            foreach(var user in userindex.Documents)
            {
                var db = await RedisHelper.cli.GetAsync("UserDb." + user.Id.ToString());
                var userRes = JsonSerializer.Deserialize<UserRes>(db);
                userRes.IsFollowed = await RedisHelper.IsFollowingAsync(uid, user.Id);
                users.Add(userRes);
            }
            return users;
        }
        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<List<UserRes>> GetFollows(long uid)
        {
            var uids = await RedisHelper.GetFollowersAsync(uid);
            var userindex = await _elasticsearch.SearchAsync<UserEs>(s => s.Index("user").Query(q => q.Terms(t => t.Field(f => f.Id).Terms(uids))));
            var users = new List<UserRes>();
            foreach(var user in userindex.Documents)
            {
                var db = await RedisHelper.cli.GetAsync("UserDb." + user.Id.ToString());
                var userRes = JsonSerializer.Deserialize<UserRes>(db);
                userRes.IsFollowed = true;
                users.Add(userRes);
            }
            return users;
        }

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<List<UserRes>> GetFriends(long uid)
        {
            var uids = await RedisHelper.GetFriendsAsync(uid);
            var users = await _usersRepository.Select.Where(a => uids.Contains(a.Id)).ToListAsync(s => new UserRes
            {
                Id = s.Id,
                UserName = s.UserName,
                Email = s.Email,
                Phone = s.Phone,
                Avatar = s.Avatar,
                Gender = s.Gender,
                NickName = s.NickName,
                IsFollowed = true,
                Profile = s.Profile
            });
            users.ForEach(s => s.IsOnLine = ImHelper.HasOnline(s.Id));
            return users;
        }
        public async Task<RedisUserDataRes> GetUserData(long uid)
        {
            var userData = await RedisHelper.GetUserDataAsync(uid);
            return userData;
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="editRes"></param>
        /// <returns></returns>
        public async Task<DefaultMesRes> EditUser(UserEditReq editRes)
        {
            await _capPublisher.PublishAsync("Edit.User", editRes);
            return new DefaultMesRes { Code = 200, Message = "修改成功" };
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<DefaultMesRes> EditPassWord(UserChangePasswordReq req)
        {
            var redisCode = await RedisHelper.cli.GetAsync("User.Edit.PasswordCode." + req.Email);
            if(redisCode != null && req.EmailCode == redisCode)
            {
                var user = await _usersRepository.Select.Where(a => a.Email == req.Email).FirstAsync();
                user.Password = req.NewPassword;
                await _usersRepository.UpdateAsync(user);
                return new DefaultMesRes { Code = 200, Message = "修改成功" };
            }
            return new DefaultMesRes { Code = 400, Message = "验证码错误" };
        }
        /// <summary>
        /// 绑定邮箱
        /// </summary>
        /// <param name="emaliBind"></param>
        /// <returns></returns>
        public async Task<EmailBindRes> BindEmail(EmaliBindReq emaliBind)
        {
            var user = await _usersRepository.Select.Where(a => a.Id == emaliBind.Uid).FirstAsync();
            var emali = await _usersRepository.Select.Where(a => a.Email == emaliBind.Email).FirstAsync();
            if(!string.IsNullOrEmpty(user.Email))
            {
                return new EmailBindRes { Code = 401, Message = "邮箱已绑定" };
            }
            if(emali != null)
            {
                return new EmailBindRes { Code = 403, Message = "邮箱已被绑定" };
            }

            var code = await RedisHelper.cli.GetAsync("Login.Email.Code" + emaliBind.Email);
            if(code != emaliBind.Code.ToString())
            {
                return new EmailBindRes { Code = 402, Message = "验证码错误" };
            }
            user.Email = emaliBind.Email;
            using(var uow = _usersRepository.Orm.CreateUnitOfWork())
            {
                await uow.GetRepository<Users>().UpdateAsync(user);
                uow.Commit();
                _capPublisher.Publish("Email.Bind", emaliBind.Email);
            }

            return new EmailBindRes { Code = 200, Message = "邮箱绑定成功" };

        }
        #endregion

        #region 扫码登录
        public async Task ScanQrCode(string qrcode)
        {

            await RedisHelper.cli.SetAsync(qrcode, 1, 60);
        }
        public async Task ScanLogin( long uid, string qrcode)
        {
            await RedisHelper.cli.SetAsync(qrcode, "2,"+uid, 60);
        }
        #endregion

        #region Others
        public async Task<string> UploadResource(IFormFile file)
        {
            if(file.ContentType != "image/jpeg" && file.ContentType != "image/png" && file.ContentType != "video/mp4")
            {
                throw new Exception("资源格式不正确");
            }
            if(file.Length > 1024 * 1024 * 100)
            {
                throw new Exception("资源大小不能超过100M");
            }
            using var stream = file.OpenReadStream();
            var name = "resource/" + DateTimeOffset.Now.ToUnixTimeSeconds() + file.FileName;
            await ObsHelper.UploadImageFile(stream, name);
            //await _capPublisher.PublishAsync("Upload.Resource", new ObsMqReq { Name = name, Stream = (FileStream)stream });
            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;
        }
        public async Task<string> UplodBg(IFormFile file)
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
            var name = "bgimage/" + DateTimeOffset.Now.ToUnixTimeSeconds() + file.FileName;
            await ObsHelper.UploadImageFile(stream, name);
            //await _capPublisher.PublishAsync("Upload.Resource", new ObsMqReq { Name = name, Stream = (FileStream)stream });
            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;
        }
        public async Task<string> UplodSound(IFormFile file)
        {
            if(file.ContentType != "audio/mp3" && file.ContentType != "audio/mpeg")
            {
                throw new Exception("音频格式不正确");
            }
            if(file.Length > 1024 * 1024 * 10)
            {
                throw new Exception("音频大小不能超过10M");
            }
            using var stream = file.OpenReadStream();
            var name = "sound/" + DateTimeOffset.Now.ToUnixTimeSeconds() + file.FileName;
            await ObsHelper.UploadImageFile(stream, name);
            //await _capPublisher.PublishAsync("Upload.Resource", new ObsMqReq { Name = name, Stream = (FileStream)stream });
            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;
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
            var name = "chatimage/" + DateTimeOffset.Now.ToUnixTimeSeconds() + file.FileName;
            await ObsHelper.UploadImageFile(stream, name);
            //await _capPublisher.PublishAsync("Upload.Resource", new ObsMqReq { Name = name, Stream = (FileStream)stream });
            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;
        }
        public async Task<string> UpLoadAvatar(IFormFile file)
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
            var name = "avatar/" + DateTimeOffset.Now.ToUnixTimeSeconds() + file.FileName;
            await ObsHelper.UploadImageFile(stream, name);
            //await _capPublisher.PublishAsync("Upload.Resource", new ObsMqReq { Name = name, Stream = (FileStream)stream });
            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;

        }
        public async Task SendEmailCode(MailRequest mail)
        {
            //保存验证码到freeredis
            var EX_Time = await RedisHelper.cli.TtlAsync("Email.Bind" + mail.ToEmail);
            if(EX_Time <= 60 * 2)
            {
                //随机6位验证码
                var code = new Random().Next(100000, 999999).ToString();
                var user= await _usersRepository.Where(s=>s.Email == mail.ToEmail).FirstAsync();
                mail.Body = EmaliTemplet.GetTemplet(int.Parse(code),0,user.NickName);
                var que = new MailBindCode
                {
                    mail = mail,
                    code = code
                };
                await _capPublisher.PublishAsync("Email.Send.BindCode", que);
            }
            else
            {
                throw new Exception("验证码发送过于频繁，请稍后再试");
            }
        }
        public async Task SendForgetPasswordCode(MailRequest mail)
        {

            //保存验证码到freeredis
            var EX_Time = await RedisHelper.cli.TtlAsync("User.Edit.PasswordCode." + mail.ToEmail);
            if(EX_Time <= 60 * 2)
            {
                //随机6位验证码
                var code = new Random().Next(100000, 999999).ToString();
                var user = await _usersRepository.Where(s => s.Email == mail.ToEmail).FirstAsync();
                mail.Body = EmaliTemplet.GetTemplet(int.Parse(code), 1,user.NickName);
                var que = new MailBindCode
                {
                    mail = mail,
                    code = code
                };
                await _capPublisher.PublishAsync("Email.Send.ForgetCode", que);
            }
            else
            {
                throw new Exception("验证码发送过于频繁，请稍后再试");
            }


        }
        #endregion


    }
}
