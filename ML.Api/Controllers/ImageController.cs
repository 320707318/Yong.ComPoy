using ImageHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ML.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost]
        public async Task<string> GetImageType(List<string> url)
        {
            return (await ImageHelper.ImageHelper.GetImageType(url)).GroupBy(x => x).OrderByDescending(x=>x.Count()).First().Key;
        }
    }
}
