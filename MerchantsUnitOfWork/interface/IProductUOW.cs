using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MerchantsUnitOfWork.Request;

namespace MerchantsUnitOfWork
{
    public interface IProductUOW
    {
        public Task AddProduct(ProductAddReq product);
    }
}
