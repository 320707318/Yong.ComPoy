using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHelper
{
    public static class ImageHelper
    {
      
       private static async Task<byte[]> DownloadImageAsync(string url)
        {
            using(HttpClient client = new HttpClient())
            {
                // 获取图片的字节数组  
                return await client.GetByteArrayAsync(url);
            }
        }
        public static async Task<List<string>> GetImageType( List< string> urls)
        {
            
            List< byte[]> imageBytes =new List<byte[]>();
            foreach(string url in urls)
            {
                imageBytes.Add(await DownloadImageAsync(url));
            }
           List<string>tags = new List<string>();
            foreach(byte[] image in imageBytes)
            {
                TypeView.ModelInput sampleData = new TypeView.ModelInput()
                {
                    ImageSource = image,
                };
                var result = TypeView.Predict(sampleData);
                var lable = result.PredictedLabel;
                tags.Add(lable);
            }

            return tags;
        }
    }
}
