using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;
using MerchantsUnitOfWork.Request;
using MerchantsUnitOfWork.Response;
using Microsoft.AspNetCore.Http;

namespace MerchantsUnitOfWork
{
    public interface IOauthUOW
    {
        public  Task<string> UpLoadImage(IFormFile file);
        public Task<DefaultMesRes<MerchantsLoginRes>> MerchantsLogin(MerchantsLoginReq req);
        public Task SendRegEmailCode(string email);
        public Task<DefaultMesRes> MerchantsReg(MerchantsRegReq req);
    }
}
