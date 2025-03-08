using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.Admin;
using DotNetCore.CAP;
using FreeSql;
using Middleware.Cap.Mq;
using SendProvide.QQEmali;
using SendProvide.QQEmali.Models;

namespace Admin.EventBus
{
    public class AdminEventBus: IAdminEventBus,ICapSubscribe
    {
        private readonly IBaseRepository<MerchantApplication> _mRepository;
        private readonly IMailService _mailService;
        private readonly ICapPublisher _capPublisher;

        public AdminEventBus(IBaseRepository<MerchantApplication> mRepository, IMailService mailService, ICapPublisher capPublisher)
        {
            this._mRepository = mRepository;
            this._mailService = mailService;
            this._capPublisher = capPublisher;
        }
        [CapSubscribe("Merchant.Application.Add")]
        public async Task AddMerchantApplication(MerchantMqDto mqDto)
        {
            await _mRepository.InsertAsync(new MerchantApplication
            {
                Email = mqDto.Email,
                BusinessLicense = mqDto.BusinessLicense,
                Description = mqDto.Description,
                IDCardPhoto = mqDto.IDCardPhoto,
                ShopName = mqDto.ShopName,
                signature = mqDto.signature
            });
        }
        [CapSubscribe("Merchant.Application.Audit")]
        public async Task AuditMerchantApplication(AuditMerchantMqDto mqDto)
        {
            var merchant = await _mRepository.Where(x => x.Id == mqDto.Id).FirstAsync();
            merchant.Status = mqDto.Status;
            await _mRepository.UpdateAsync(merchant);
            if(mqDto.Status==1)
            {
                await _mailService.SendEmailAsync(new MailRequest
                {
                    ToEmail = merchant.Email,
                    Subject = "审核结果通知",
                    Body = EmaliTemplet.GetAuditTemplet(merchant.ShopName, "审核通过", "您可以通过申请的邮箱账号登录")
                });
                await _capPublisher.PublishAsync("Merchant.AddShop", new MerchantMqDto
                {
                    Email = merchant.Email,
                    BusinessLicense = merchant.BusinessLicense,
                    Description = merchant.Description,
                    IDCardPhoto = merchant.IDCardPhoto,
                    ShopName = merchant.ShopName,
                    signature = merchant.signature,
                    Id=mqDto.Id
                });
            }
            else
            {
                await _mailService.SendEmailAsync(new MailRequest
                {
                    ToEmail = merchant.Email,
                    Subject = "审核结果通知",
                    Body = EmaliTemplet.GetAuditTemplet(merchant.ShopName, "审核未通过", mqDto.Reason)
                });
            }
        }
    }
}
