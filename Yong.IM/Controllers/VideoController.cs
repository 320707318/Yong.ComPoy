using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Yong.IM.Req;
using DbModels.BaseModel;
using DotNetCore.CAP.Messages;

namespace Yong.IM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _clients = new();
        [HttpGet("ws")]
        public async Task Get(string id)
        {
            if(HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string clientId =id;
                _clients[clientId] = webSocket;

                await HandleWebSocketCommunication(webSocket, clientId);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
        [HttpPost("call")]
        public async Task< DefaultMesRes> Call(VideoReq video)
        {
            var client = _clients.Where(c => c.Key == video.receiverId.ToString()).FirstOrDefault();
            if(client.Key.Length>0)
            {
                await BroadcastMessage(JsonSerializer.Serialize( video), video.receiverId.ToString());
                return new DefaultMesRes
                {
                    Code = 200,
                    Message = "等待对方接通中..."
                };
            }
            else
            {
                return new DefaultMesRes
                {
                    Code=400,
                    Message="对方不在线"
                };
            }
        }
        [HttpPost("callback")]
        public async Task CallBack(VideoReq video)
        {
            await BroadcastMessage(JsonSerializer.Serialize(video), video.receiverId.ToString());
        }

        private async Task HandleWebSocketCommunication(WebSocket webSocket, string clientId)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while(webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // 处理接收到的信令消息  
                    await BroadcastMessage(message, clientId);
                }
            }
            catch(Exception e)
            {
                // 处理异常  
                Console.WriteLine(e);
            }
            finally
            {
                //_clients.TryRemove(clientId, out _);
                //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the server", CancellationToken.None);
            }
        }

        private async Task BroadcastMessage(string message, string senderId)
        {
            var mes=new VideoReq();
            try
            {
                 mes = JsonSerializer.Deserialize<VideoReq>(message);
            }catch(Exception e)
            {
                Console.WriteLine(e);
                return;
            }
            
            var client = _clients.Where(c=>c.Key==mes.receiverId.ToString()).FirstOrDefault();
            if(client.Key != senderId)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await client.Value.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
