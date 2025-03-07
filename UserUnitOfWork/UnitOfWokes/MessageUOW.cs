using System.Text;
using System.Text.Json;
using DotNetCore.CAP;
using DotNetCore.CAP.Messages;
using Middleware.FreeIm;
using Middleware.Redis;
using RestSharp;
using UserUnitOfWork.Interface;
using UserUnitOfWork.Request;

namespace UserUnitOfWork.UnitOfWokes
{
    public class MessageUOW : IMessageUOW
    {
        private readonly ICapPublisher _capPublisher;

        public MessageUOW(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }
        public async Task SendPrivateLetter(PrivateLetterReq req)
        {
            if(await RedisHelper.AreMutuallyFollowingAsync(req.ReceiverId, req.SenderId))
            {
                var user = JsonSerializer.Deserialize<UserRes>(await RedisHelper.cli.GetAsync("UserDb." + req.SenderId));
                var message = new ImRes
                {
                    to = req.ReceiverId,
                    content = req.Content,
                    type = ImResType.Private,
                    from = user
                };
                if(ImHelper.HasOnline(req.ReceiverId))
                {
                    ImHelper.SendMessage(req.SenderId, new List<long>() { req.ReceiverId }, JsonSerializer.Serialize(message));
                }
                else
                {
                    await _capPublisher.PublishAsync("Message.ToBeSent", message);
                }
            }
        }

        public async Task<string> SendAi(AiReq req)
        {
            if(await RedisAi.GetAiCount(req.SenderId)==0)
            {
                return "No Ai available";
            }
            var url = "https://spark-api-open.xf-yun.com/v1/chat/completions";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer gWvsNMJppmSBGLTHzDCl:qgnGrqlEipPtCJkCmerK");

            var jsonBody = new
            {
                model = "generalv3.5",
                messages = new[]
            {
            new { role = "user", content = req.Content }
            },
                stream = false,
                max_tokens=1024,
            };
            var content = new StringContent(JsonSerializer.Serialize(jsonBody), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);
            await RedisAi.decrAiCount(req.SenderId);
            if (response.IsSuccessStatusCode)
            {

                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "Error";
            }
           


        }
    }
}
