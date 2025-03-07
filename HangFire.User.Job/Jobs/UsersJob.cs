using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DbModels;
using Elasticsearch.Net;
using FreeSql;
using HangFire.User.Job.Interface;
using Middleware.ES;
using Middleware.Redis;
using Nest;

namespace HangFire.User.Job.Jobs
{
    public class UsersJob : IUsersJob
    {
        private readonly IBaseRepository<Users> _usersRepository;
        private readonly ElasticClient _elasticsearch;
        public UsersJob(IBaseRepository<Users> UsersRepository, ElasticClient elasticsearch)
        {
            this._usersRepository = UsersRepository;
            this._elasticsearch = elasticsearch;
        }
        public async Task UserDbToRedis()
        {
            var count = await _usersRepository.Select.CountAsync();
            var index = 1;
            var pageSize = 1000;

            //TODO:将用户信息写入Redis
            while(count > 1000)
            {
                var users = await _usersRepository.Select.OrderBy(a => a.Id).Page(index, pageSize).ToListAsync();
                index++;
                count -= pageSize;
                //TODO:将用户信息写入Redis
                users.ForEach(async a =>
                {
                    await RedisHelper.cli.SetAsync("UserDb." + a.Id, a);
                });
            }
            var usersEnd = await _usersRepository.Select.OrderBy(a => a.Id).Page(index, pageSize).ToListAsync();
            for(int i = 0; i < usersEnd.Count; i++)
            {
                await RedisHelper.cli.SetAsync("UserDb." + usersEnd[i].Id, JsonSerializer.Serialize<Users>(usersEnd[i]));
            }

        }
        public async Task UserRedisToDb()
        {
            var count = await _usersRepository.Select.CountAsync();
            var index = 1;
            var pageSize = 1000;

            //TODO:将用户信息写入Redis
            while(count > 0)
            {
                var users = await _usersRepository.Select.OrderBy(a => a.Id).Page(index, pageSize).ToListAsync();
                index++;
                count -= pageSize;
                //TODO:将redis写入用户数据库
                users.ForEach(async a =>
                {
                    var num = (await RedisHelper.GetFansAsync(a.Id)).Count();
                    if(a.FansCount< num)
                    {
                        await _usersRepository.UpdateDiy.Where(b => b.Id == a.Id).Set( a => a.FansCount, num).ExecuteAffrowsAsync();
                    }

                });
            }
        }
        public async Task UserToEs()
        {
            //TODO:将用户信息写入ES
            var count = await _usersRepository.Select.CountAsync();
            var index = 1;
            var pageSize = 1000;

            //TODO:将用户信息写入Redis
            while(count > 0)
            {
                var users = await _usersRepository.Select.OrderBy(a => a.Id).Page(index, pageSize).ToListAsync<UserEs>();
                index++;
                count -= pageSize;
                //TODO:将redis写入用户数据库
                var bulkRequest = new BulkRequest("user") // 替换为您的索引名称  
                {
                    //Operations = pages.Select(page => new BulkIndexOperation<DbModels.Page>(page)) // YourPageType 应该替换为你的实体类型  
                    //                  .Cast<IBulkOperation>() // 显式转型确保类型一致  
                    //                  .ToList()
                };
                var bulkResponse = await _elasticsearch.BulkAsync(b => b
                    .Index("user") // 替换为您的索引名称  
                    .IndexMany(users) // YourPageType 应该替换为你的实体类型
                    .Refresh(Refresh.WaitFor)
                );

                if(!bulkResponse.IsValid)
                {
                    throw new Exception("bulk failed");
                }
            }

        }
        public async Task UserAiPush()
        {
            var count = await _usersRepository.Select.CountAsync();
            var index = 1;
            var pageSize = 1000;

            //TODO:将用户信息写入Redis
            while(count > 0)
            {
                var users = await _usersRepository.Select.OrderBy(a => a.Id).Page(index, pageSize).ToListAsync();
                index++;
                count -= pageSize;
                foreach(var user in users)
                {
                    await RedisAi.AiPush(user.Id);
                }


            }
        }
        

    }
  }

