using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace DbModels.merchants
{
    public class Product
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDesc { get; set; }
        public string ProductContent { get; set; }
        public string ProducRemark { get; set; }
        public string ProductImgs { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; } = 0;
        public int Stock { get; set; }
        public long MerchantId { get; set; }
        public long CreateTime { get; set; }= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
