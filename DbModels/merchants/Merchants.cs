using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels.merchants
{
    public class Merchants
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public string ShopName { get; set; }
        public string Avatar { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string IDCardPhoto { get; set; }
        public string BusinessLicense { get; set; }
        public string signature { get; set; }
    }
}
