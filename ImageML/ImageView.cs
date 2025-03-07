using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageML
{
    public static class ImageView
    {
      
       private static async Task<byte[]> DownloadImageAsync(string url)
        {
            using(HttpClient client = new HttpClient())
            {
                // 获取图片的字节数组  
                return await client.GetByteArrayAsync(url);
            }
        }
        public static async Task<string> GetImageType(string url)
        {
            byte[] imageBytes = await DownloadImageAsync(url);
            TypeView.ModelInput sampleData = new TypeView.ModelInput()
            {
                ImageSource = imageBytes,
            };
            var result =  TypeView.Predict(sampleData);
            var lable = result.PredictedLabel;
            return lable;
        }
    }
}
