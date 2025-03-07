using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;

namespace UserUnitOfWork.Request
{
    public class UserReq
    {
       
    }
    public class LoginReq
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class LoginByEmailReq
    {
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; } 
        public string EmailCode { get; set; }
    }
    public class LoginByPhoneReq
    {
        [RegularExpression(@"^1[34578]\d{9}$", ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; }
        public string PhoneCode { get; set; }
    }
    public class RegisterReq
    {
        [Required(ErrorMessage = "Please enter your username"),StringLength(maximumLength:10,MinimumLength =6, ErrorMessage = "Username must be between 6 and 10 characters")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must be letters or numbers only")]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter your email"),StringLength(maximumLength:10,MinimumLength =6, ErrorMessage = "Email must be between 6 and 10 characters")]
        public string Password { get; set; } = string.Empty;

        [RegularExpression(@"^1[34578]\d{9}$", ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your nickname"),StringLength(maximumLength:10,MinimumLength =1, ErrorMessage = "Nickname must be between 2 and 10 characters    ")]
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public int Gender { get; set; } = 0;

    }
    public class UserEditReq 
    {
        public long Uid { get; set; }
        [Required(ErrorMessage = "Please enter your nickname"), StringLength(maximumLength: 10, MinimumLength = 2, ErrorMessage = "Nickname must be between 2 and 10 characters    ")]
        public string NickName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;

        public string Banner { get; set; }

        public string Profile { get; set; } = string.Empty;

    }
    public class UserChangePasswordReq: LoginByEmailReq
    {
        [Required(ErrorMessage = "Please enter your password"), StringLength(maximumLength: 10, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 10 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
    public class UserSearchReq : BasePagination
    {
        [Required(ErrorMessage = "Please enter your keyword")]
        public string KeyWord { get; set; } 
    }

}
