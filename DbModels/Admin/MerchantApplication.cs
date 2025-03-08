using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels.Admin
{
    public class MerchantApplication
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public string ShopName { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string IDCardPhoto { get; set; }
        public string BusinessLicense { get; set; }
        public string signature { get; set; }

        public int Status { get; set; } = 0;
        public long CreateTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
