using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DbModels;
using DbModels.BaseModel;
using DotNetCore.CAP;
using FreeRedis;
using FreeSql;
using Middleware.Cap.Mq;
using Middleware.Redis;
using PageUnitOfWork.Inferface;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace PageUnitOfWork.UnitOfWokes
{
    public class SuperTopicUOW:ISuperTopicUOW
    {
        private readonly IBaseRepository<SuperTopic> _repository;
        private readonly ICapPublisher _capPublisher;
        private readonly IBaseRepository<SuperTopicType> _typerepository;

        public SuperTopicUOW(IBaseRepository<SuperTopic> repository, ICapPublisher capPublisher,IBaseRepository<SuperTopicType> typerepository)
        {
            this._repository = repository;
            this._capPublisher = capPublisher;
            this._typerepository = typerepository;
        }
        #region 添加
        public async Task<DefaultMesRes> JoinSuperTopic( long pageId,long userId)
        {
            await RedisHelper.JoinTopicAsync(userId, pageId);
            return new DefaultMesRes()
            {
                Message = "加入成功",
                Code = 200
            };
        }
        public async Task<DefaultMesRes> QuitSuperTopic(long pageId, long userId)
        {
            await RedisHelper.OutTopicAsync(userId, pageId);
            return new DefaultMesRes()
            {
                Message = "退出成功",
                Code = 200
            };
        }
        public async Task<DefaultMesRes> DeleteSuperTopic(long pageId, long userId)
        {
            var topic = await _repository.Select.Where(s => s.Id == pageId).ToOneAsync();
            if (topic == null)
            {
                return new DefaultMesRes()
                {
                    Message = "超话不存在",
                    Code = 400
                };
            }
            if (topic.CreatorId!= userId)
            {
                return new DefaultMesRes()
                {
                    Message = "只能删除自己创建的超话",
                    Code = 400
                };
            }
            await _repository.DeleteAsync(topic);
            await RedisHelper.DissolutionAsync(pageId);
            return new DefaultMesRes()
            {
                Message = "解散成功",
                Code = 200
            };
        }
        public async Task<DefaultMesRes> AddSuperTopic(SuperTopicAddMqDto superTopic)
        {
            // 获取当前年份  
            int currentYear = DateTime.UtcNow.Year;

            // 构造当前年份的1月1日UTC时间（午夜）的DateTimeOffset对象  
            DateTimeOffset utcYearStart = new DateTimeOffset(currentYear, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // 使用DateTimeOffset的ToUnixTimeSeconds方法获取时间戳  
            long timestamp = utcYearStart.ToUnixTimeSeconds();
            if(await _repository.Select.Where(s=>s.CreateTime>= timestamp&&s.CreatorId==superTopic.CreatorId).CountAsync()==3)
            {
                return new DefaultMesRes()
                {
                    Message = "每年只能创建3个超话",
                    Code = 400
                };
            }
            _capPublisher.Publish("SuperTopic.Add", superTopic);
            return new DefaultMesRes()
            {
                Message = "添加成功",
                Code = 200
            };
        }
        public async Task<DefaultMesRes> AddSuperTopicType(SuperTopicTypeAddMqDto superTopicType)
        { 
            await _typerepository.InsertAsync(new SuperTopicType()
            {
                Name = superTopicType.Name,
                Sort = superTopicType.Sort
            });
            var tags=await _typerepository.Select.Where(s=>s.Status==1).ToListAsync();
            foreach(var tag in tags)
            {
                await RedisHelper.cli.LPushAsync("SuperTopicType", tag.Name);
            }
            return new DefaultMesRes()
            {
                Message = "添加成功",
                Code = 200
            };
        }

        #endregion

        #region 查询
        public async Task<SuperTopicRes> GetSuperTopic(long id,long uid)
        {
            var res=await _repository.Select.Where(s => s.Id == id).ToOneAsync<SuperTopicRes>();
            res.IsJoin = RedisHelper.IsInTopicAsync(uid, id);
            return res;
        }
        public async Task<List<SuperTopicSimpleRes>> GetSuperTopicByIds(SuperTopicIdsReq req)
        {
            var res = await _repository.Select.Where(s => req.ids.Contains(s.Id)).ToListAsync<SuperTopicSimpleRes>();
            return res;
        }
        public async Task<List<TypeRes>> GetSuperTopicType()
        {
            var redisData= (await RedisHelper.cli.LRangeAsync("SuperTopicType", 0, -1)).ToList();
            if(redisData.Count > 0)
            {
                return redisData.Select(s => JsonSerializer.Deserialize<TypeRes>(s)).ToList();
            }
            else
            {
                var tags= await _typerepository.Select.OrderBy(s => s.Sort).ToListAsync<TypeRes>();
                
                await RedisHelper.cli.LPushAsync("SuperTopicType", tags.Select(s => JsonSerializer.Serialize(s)).ToArray());
               
                return tags;
            }
            
        }
        public async Task<List<SuperTopicRes>> GetSuperTopByJoin(long uid)
        {
            var ids=(await RedisHelper.cli.SMembersAsync("user:jointopic:"+uid)).Select(s=>long.Parse(s));
            var res= await _repository.Select.Where(s => ids.Contains(s.Id)).OrderByDescending(s => s.Number).ToListAsync<SuperTopicRes>();
            var types=await GetSuperTopicType();
            foreach(var item in res)
            {
                item.Number = (await RedisHelper.GetTopicCountAsync(item.Id)).ToString();
                item.IsJoin = RedisHelper.IsInTopicAsync(uid, item.Id);
                item.type = types.FirstOrDefault(s => s.Id == item.TypeId);
            }
            return res;
        }
        public async Task<List<SuperTopicRes>> GetSuperTopByCreater(long uid)
        {
            var res = await _repository.Select.Where(s =>s.CreatorId==uid).OrderByDescending(s => s.Number).ToListAsync<SuperTopicRes>();
            var types = await GetSuperTopicType();
            foreach(var item in res)
            {
                item.Number = (await RedisHelper.GetTopicCountAsync(item.Id)).ToString();
                item.IsJoin = RedisHelper.IsInTopicAsync(uid, item.Id);
                item.type = types.FirstOrDefault(s => s.Id == item.TypeId);
            }
            return res;
        }
        public async Task<List<SuperTopicRes>> GetSuperTopicSearch(SuperTopicSearchReq super,long uid)
        {
            var res= await _repository.Select
                .WhereIf(!string.IsNullOrEmpty(super.search), s => s.Name.Contains(super.search))
                .WhereIf(super.type != 0, s => s.TypeId == super.type)
                .Where(s=>s.Status==0)
                .OrderByDescending(s => s.Number)
                .Page(super.PageIndex, super.PageIndex)
                .ToListAsync<SuperTopicRes>();
            foreach (var item in res)
            {
                item.Number=(await RedisHelper.GetTopicCountAsync(item.Id)).ToString();
                item.IsJoin =  RedisHelper.IsInTopicAsync(uid,item.Id);
            }
            return res;
        }
        //public async Task<SuperTopicDetailRes> GetSuperTopic(long id,long uid)
        //{
        //    var res= await _repository.Select.Where(s => s.Id == id).ToOneAsync<SuperTopicDetailRes>();
        //    res.User = await RedisHelper.cli.GetAsync<UserRes>("UserDb."+res.CreatorId);
        //    return res;
        //}
        #endregion
    }
}
