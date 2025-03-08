using System.IdentityModel.Tokens.Jwt;
using DbModels.BaseModel;
using MerchantsUnitOfWork;
using MerchantsUnitOfWork.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yong.Merchants.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductUOW _productUOW;
        private IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private int uid;
        public ProductController(IProductUOW productUOW)
        {
            this._productUOW = productUOW;
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string tokenStr = authHeader.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var payload = handler.ReadJwtToken(tokenStr).Payload;
            var claims = payload.Claims;
            uid = int.Parse(claims.First(claim => claim.Type == "id").Value);
        }
        [HttpPost]
        [Authorize(Roles = "Merchants")]
        public async Task<DefaultMesRes> AddProduct(ProductAddReq product)
        {
            product.MerchantId = uid;
            await _productUOW.AddProduct(product);
            return new DefaultMesRes() { Code = 200, Message = "添加成功" };
        }
    }
}
