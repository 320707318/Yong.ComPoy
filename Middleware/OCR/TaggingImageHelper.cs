using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AlibabaCloud.SDK.Imagerecog20190930;
using AlibabaCloud.SDK.Imagerecog20190930.Models;
using Tea;

namespace Middleware.OCR
{
    public static class TaggingImageHelper
    {
        public static Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
            };
            config.Endpoint = "imagerecog.cn-shanghai.aliyuncs.com";
            return new Client(config);
        }
        public static async Task<List<TaggingImageRes>> TaggingImage(string url)
        {
            Client client = CreateClient("LTAI5t9EUnLP9JeQ6J91WiXm", "e0LqrwAoWWTBf8XWqe7H29dzOlkUTM");         
            TaggingImageAdvanceRequest taggingImageRequest = new TaggingImageAdvanceRequest();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            taggingImageRequest.ImageURLObject = stream;
            AlibabaCloud.TeaUtil.Models.RuntimeOptions runtime = new AlibabaCloud.TeaUtil.Models.RuntimeOptions();
            try
            {
                var tagResponse =await client.TaggingImageAdvanceAsync(taggingImageRequest, runtime);
                var tags =JsonSerializer.Deserialize<List<TaggingImageRes>>(AlibabaCloud.TeaUtil.Common.ToJSONString(tagResponse.Body.Data.Tags)) ;
                return tags;
            }
            catch(TeaException error)
            {
                // 如有需要，请打印 error
                Console.WriteLine(error.Message);
                throw new Exception(error.Message);
            }
        }
    }
}
