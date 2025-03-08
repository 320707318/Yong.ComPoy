using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DbModels.merchants;
using DotNetCore.CAP;
using FreeSql;
using MerchantsUnitOfWork.Request;
using Middleware.Cap.Mq;
using Middleware.Redis;

namespace MerchantsUnitOfWork.UnitOfWorks
{
    public class ProductUOW: IProductUOW
    {
        private readonly IBaseRepository<Product> _repository;
        private readonly ICapPublisher _capPublisher;

        public ProductUOW(IBaseRepository<Product> repository,ICapPublisher capPublisher)
        {
            this._repository = repository;
            this._capPublisher = capPublisher;
        }

        public async Task  AddProduct(ProductAddReq product)
        {
            ProductAddDto req= product;
           await _capPublisher.PublishAsync("Product.Add", req);
        }
    }
}
