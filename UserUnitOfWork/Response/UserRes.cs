using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserUnitOfWork.Response
{
    public class UserRes
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsOnLine { get; set; } = false;
        public long FansCount { get; set; } = 0;
        public bool IsFollowed { get; set; } = false;
        public int Gender { get; set; } = 0;
        public string Banner { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
    }
    public class UserSimpleRes
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public int Gender { get; set; }
    }
    public class LoginRes
    {
        public string Token { get; set; } = string.Empty;
        public UserRes User { get; set; } = new UserRes();
        public int Code { get; set; } = 400;
        public string Message { get; set; } = string.Empty;
    }
    public class RegisterRes
    {
        public UserRes User { get; set; } = new UserRes();
        public int Code { get; set; } = 400;
        public string Message { get; set; } = string.Empty;
    }

    
}
