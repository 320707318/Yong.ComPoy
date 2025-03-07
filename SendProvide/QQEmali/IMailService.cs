using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendProvide.QQEmali.Models;

namespace SendProvide.QQEmali
{
    public interface IMailService
    {
       public Task SendEmailAsync(MailRequest mailRequest);
    }
}
