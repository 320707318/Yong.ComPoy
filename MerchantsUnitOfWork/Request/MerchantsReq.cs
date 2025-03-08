using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantsUnitOfWork.Request
{
    public class MerchantsReq
    {
    }
    public class MerchantsLoginReq
    {
        public string Email { get; set; }
        public string PassWord { get; set; }
    }
    public class MerchantsRegReq
    {
        public string Email { get; set; }
        public string PassWord { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string ShopName { get; set; }
        public string IDCardPhoto { get; set; }
        public string BusinessLicense { get; set; }
        public string signature { get; set; }
    }
}
