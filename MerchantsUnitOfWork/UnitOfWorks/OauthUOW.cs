using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Middleware.OBS;
using UserUnitOfWork.Interface;

namespace MerchantsUnitOfWork.UnitOfWorks
{
    public class OauthUOW: IOauthUOW
    {
        public async Task<string> UpLoadImage(IFormFile file)
        {
            if(file.ContentType != "image/jpeg" && file.ContentType != "image/png")
            {
                throw new Exception("图片格式不正确");
            }
            if(file.Length > 1024 * 1024 * 10)
            {
                throw new Exception("图片大小不能超过10M");
            }
            using var stream = file.OpenReadStream();
            var name = "Merchants/" + DateTimeOffset.Now.ToUnixTimeSeconds() + file.FileName;
            await ObsHelper.UploadImageFile(stream, name);
            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;
        }
    }
}
