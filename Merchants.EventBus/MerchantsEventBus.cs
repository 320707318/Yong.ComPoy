using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels;
using DotNetCore.CAP;
using FreeSql;
using Middleware.Cap.Mq;
using Middleware.Redis;
using Nest;
using SendProvide.QQEmali;

namespace Merchants.EventBus
{
    public class MerchantsEventBus:IMerchantsEventBus, ICapSubscribe
    {
        private readonly IMailService _mailService;
        private readonly IBaseRepository<DbModels.merchants.Merchants> _mRepository;
        //private readonly ElasticClient _elasticsearch;

        public MerchantsEventBus(IMailService mailService, IBaseRepository<DbModels.merchants.Merchants> mRepository)
        {
            this._mailService = mailService;
            this._mRepository = mRepository;
            //this._elasticsearch = elasticsearch;
        }
        [CapSubscribe("Reg.Email.Code")]
        public async Task SendLoginCode(MailLoginCode mbc)
        {
            await _mailService.SendEmailAsync(mbc.mail);
            await RedisHelper.cli.SetAsync("Reg.Email.Code" + mbc.mail.ToEmail, mbc.code, 60 * 10);
        }
    }
}
