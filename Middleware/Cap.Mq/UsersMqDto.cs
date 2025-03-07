using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendProvide.QQEmali.Models;

namespace Middleware.Cap.Mq
{
    public class UsersMqDto
    {
       
    }
    public class MailBindCode
    {
        public MailRequest mail { get; set; }
        public string code { get; set; }
    }
    public class MailLoginCode
    {
        public MailRequest mail { get; set; }
        public string code { get; set; }
    }
    public class EditUserMqDtio
    {
        public long Uid { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public  string Banner { get; set; }
        public string Profile { get; set; } = string.Empty;
    }
    public class UserTypeMqDto
    {
        public long Uid { get; set;}
        public long PageId { get; set; }
    }
    public class FollowMqDto
    {
        public long Uid { get; set; }
        public long FollowUid { get; set; }
    }
}
