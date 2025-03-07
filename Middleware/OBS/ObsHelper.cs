using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBS;
using OBS.Model;

namespace Middleware.OBS
{
    public static  class ObsHelper
    {
        public static ObsClient obs=new ObsClient("SSNWSQTKVYEFREWNHHMV", "Ki34AkR8DGOTt5Xm18olRJtk0YWLl2xye1KUBgzm", "https://obs.cn-south-1.myhuaweicloud.com");

        public async static Task UploadImageFile(Stream file, string name)
        {
            if(file == null || file.Length == 0)
            {
                throw new ArgumentException("文件不能为空");
            }

            try
            {
                var uploadRequest = new PutObjectRequest
                {
                    BucketName = "obs-bucked",
                    ObjectKey = name,
                    InputStream = file
                };
                //var s= ObsHelper.obs.GetBucketAcl(new GetBucketAclRequest { BucketName = "obs-bucked" });
                // 上传文件  
                var response = ObsHelper.obs.PutObject(uploadRequest);
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await Console.Out.WriteLineAsync($"文件上传成功: {name}");
                }
                else
                {
                    throw new Exception($"文件上传失败: {response.StatusCode}");
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
    
}
