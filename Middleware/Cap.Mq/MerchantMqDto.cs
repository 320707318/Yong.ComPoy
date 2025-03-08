using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Cap.Mq
{
    public class MerchantMqDto
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string PassWord { get; set; }
        public string Description { get; set; }
        public string ShopName { get; set; }
        public string IDCardPhoto { get; set; }
        public string BusinessLicense { get; set; }
        public string signature { get; set; }
    }
    public class AuditMerchantMqDto
    {
        public long Id { get; set; }
        public int Status { get; set; }

        public string Reason { get; set; }
    }
}
