using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SendProvide.QQEmali.Models;

namespace SendProvide.QQEmali
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService()
        {
            _mailSettings = new MailSettings();
            _mailSettings.Host = "smtp.qq.com";
            _mailSettings.Port = 25;
            _mailSettings.Mail = "320707318@qq.com";
            _mailSettings.Password = "uyzmdsjjidswbggj";
            _mailSettings.DisplayName = "Yong.ComPoy";
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            //一定要使用下面这句代码添加发送人邮箱信息,否则QQ收件箱那边无法看到发送人的邮箱信息。
            email.From.Add(MailboxAddress.Parse(_mailSettings.Mail));
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if(mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach(var file in mailRequest.Attachments)
                {
                    if(file.Length > 0)
                    {
                        using(var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.Name, fileBytes, MimeKit.ContentType.Parse(file.GetType().ToString()));
                    }
                }
            }
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using(var smtp = new SmtpClient())
            {
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }
    }
}
