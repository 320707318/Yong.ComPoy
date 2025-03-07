
using System.Text.Json;
using DbModels;
using DotNetCore.CAP;
using FreeSql;
using Middleware.Cap.Mq;
using Middleware.ES;
using Middleware.Redis;
using Nest;
using RestSharp;
using SendProvide.QQEmali;
using SendProvide.QQEmali.Models;
namespace Cap.EventBus
{
    public class UserEventBus : IUserEventBus, ICapSubscribe
    {
        private readonly IMailService _mailService;
        private readonly IBaseRepository<Users> _usersRepository;
        private readonly ElasticClient _elasticsearch;

        public UserEventBus(IMailService mailService, IBaseRepository<Users> UsersRepository, ElasticClient elasticsearch)
        {
            this._mailService = mailService;
            this._usersRepository = UsersRepository;
            this._elasticsearch = elasticsearch;
        }
        [CapSubscribe("Email.Bind")]
        public async Task BindEmail(string email)
        {
            await Console.Out.WriteLineAsync($"收到绑定邮箱事件，邮箱：{email}");
            await _mailService.SendEmailAsync(new MailRequest { ToEmail = email, Subject = "绑定邮箱成功" });
            var id=_usersRepository.Select.Where(s=>s.Email==email).First().Id;
            await RedisHelper.cli.DelAsync("UserDb." +id);
        }
        [CapSubscribe("Email.Send.BindCode")]
        public async Task SendBindCode(MailBindCode mbc)
        {
            await _mailService.SendEmailAsync(mbc.mail);
            await RedisHelper.cli.SetAsync("Email.Bind" + mbc.mail.ToEmail, mbc.code, 60 * 5);
        }
        [CapSubscribe("Email.Send.ForgetCode")]
        public async Task SendForgetCode(MailBindCode mfc)
        {
            await _mailService.SendEmailAsync(mfc.mail);
            await RedisHelper.cli.SetAsync("User.Edit.PasswordCode." + mfc.mail.ToEmail, mfc.code, 60 * 5);
        }
        [CapSubscribe("Login.Email.Code")]
        public async Task SendLoginCode(MailLoginCode mbc)
        {
            await _mailService.SendEmailAsync(mbc.mail);
            await RedisHelper.cli.SetAsync("Login.Email.Code" + mbc.mail.ToEmail, mbc.code, 60 * 5);
        }
        [CapSubscribe("Edit.User")]
        public async Task EditUser(EditUserMqDtio  editRes)
        {
            await  RedisHelper.cli.DelAsync("UserDb."+editRes.Uid);
            var user= await _usersRepository.Where(s=>s.Id==editRes.Uid).FirstAsync();
            user.NickName = editRes.NickName;
            user.Avatar = editRes.Avatar;
            user.Banner=editRes.Banner;
            user.Profile=editRes.Profile;
            await _usersRepository.UpdateAsync(user);
            //延时
            await Task.Delay(500);
            await RedisHelper.cli.DelAsync("UserDb." + editRes.Uid);
        }
        [CapSubscribe("Add.UserDb")]
        public async Task AddUser(RedisSetMqDto user)
        {
            await _elasticsearch.IndexAsync(JsonSerializer.Deserialize<UserEs>(user.Value), idx => idx.Index("user"));
            await RedisHelper.cli.SetAsync(user.Key, user.Value, user.ExpireTime);
        }

        
        
    }
}
