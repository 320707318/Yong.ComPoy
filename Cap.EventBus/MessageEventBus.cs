using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Middleware.Cap.Mq;
using Middleware.FreeIm;
using Middleware.Redis;
using Nest;
using Org.BouncyCastle.Ocsp;

namespace Cap.EventBus
{
    public class MessageEventBus:IMessageEventBus, ICapSubscribe
    {
        private readonly ElasticClient _elasticsearch;

        public MessageEventBus(ElasticClient elasticsearch)
        {
            this._elasticsearch = elasticsearch;
        }
        [CapSubscribe("Message.ToBeSent")]
        public async Task AddMessageToBeSent(ImRes message)
        {
            await RedisHelper.cli.LPushAsync("Message:ToBeSent:"+message.to,JsonSerializer.Serialize(message));
        }
       [CapSubscribe("Message.SendToBe")]
        public async Task SendMessageToBe(long uid)
        {
            var messages=await RedisHelper.cli.LRangeAsync("Message:ToBeSent:"+uid,0,-1);
            foreach (var message in messages)
            {
                await RedisHelper.cli.LRemAsync("Message:ToBeSent:"+uid,1,message);
                var imRes=JsonSerializer.Deserialize<ImRes>(message);
                ImHelper.SendMessage(imRes.from.Id,new List<long> { imRes.to }, imRes);
            }
        }
        [CapSubscribe("Message.Metio")]
        public async Task SendMessage(MetionMqDto method)
        {
            var page=await _elasticsearch.SearchAsync<DbModels.Page>(s=>
                    s.Index("page")
                   .Query(q=>q.Match(m=>m.Field(f=>f.id).Query(method.PageId.ToString())))

                );
            foreach (var item in method.Ids)
            {
                if(await RedisHelper.IsFollowingAsync(method.Uid, item))
                {
                    var user = JsonSerializer.Deserialize<UserRes>(await RedisHelper.cli.GetAsync("UserDb." + method.Uid));
                    var message = new MetionRes
                    {
                        to = item,
                        content =new ImReq { content="你有一条好友的提到",type=ImReqType.Metion} ,
                        type = ImResType.Private,
                        from = user,
                        pagedesc=page.Documents.First().Content,
                        pageid =method.PageId,
                        
                    };
                    if(ImHelper.HasOnline(item))
                    {
                        ImHelper.SendMessage(method.Uid, new List<long>() { item }, JsonSerializer.Serialize(message));
                    }
                    else
                    {
                        await RedisHelper.cli.LPushAsync("Message:ToBeSent:" + message.to, JsonSerializer.Serialize(message));
                    }
                }
            }

        }
        [CapSubscribe("Message.Follow")]
        public async Task FollowMessage(FollowMqDto mqDto)
        {
            var border= JsonSerializer.Deserialize<UserRes>(await RedisHelper.cli.GetAsync("UserDb." +9));
            var user = JsonSerializer.Deserialize<UserRes>(await RedisHelper.cli.GetAsync("UserDb." + mqDto.Uid));
            var message = new ImRes
            {
                to = mqDto.FollowUid,
                from = border,
                content = new ImReq { type = ImReqType.Text, content = $"用户  {user.NickName}  关注了你" },
                type = ImResType.System

            };
            if (ImHelper.HasOnline(mqDto.FollowUid))
            {
                ImHelper.SendMessage(mqDto.Uid, new List<long>() { mqDto.FollowUid }, JsonSerializer.Serialize(message));
            }
            else
            {
                await RedisHelper.cli.LPushAsync("Message:ToBeSent:" + message.to, JsonSerializer.Serialize(message));
            }
        }

    }
}
