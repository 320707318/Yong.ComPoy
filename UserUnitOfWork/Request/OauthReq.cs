using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserUnitOfWork.Request
{
    public class OauthReq
    {
    }
    public class EmaliBindReq
    {
        public long Uid { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        public int Code { get; set; }
    }
    public class LoginByEmailRes
    {

    }
}
