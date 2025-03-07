using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Job.Page.Interface;
using FreeSql;
using System.Text.Json;
using Nest;
using System.Reflection.Metadata;
using Elasticsearch.Net;
using Middleware.Redis;
using DbModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Job.Page.Jobs
{
    public class PageJob: IPageJob
    {
        private readonly IBaseRepository<Comment> _commentRepository;
        private readonly IBaseRepository<DbModels.Page> _repository;
        private readonly ElasticClient _elasticsearch;

        public PageJob(
            IBaseRepository<Comment> commentRepository,
            IBaseRepository<DbModels.Page> repository, 
            ElasticClient elasticsearch)
        {
            this._commentRepository = commentRepository;
            this._repository = repository;
            this._elasticsearch = elasticsearch;
        }
        public async Task PageDbToEs()
        {
            var count = await _repository.Select.CountAsync();
            var index = 1;
            var pageSize = 1000;
            while(count >=0)
            {
                var pages = await _repository.Select
                    .IncludeMany(a => a.Resources)
                    .Page(index, pageSize).ToListAsync();
                pages.ForEach(s => 
                    s.SuperIds=s.SuperTopics.Split(',').Select(a=>new Supers { Id=a}).ToList()
                    
                );
                var bulkResponse = await _elasticsearch.BulkAsync(b => b
                    .Index("page") // 替换为您的索引名称  
                    .IndexMany(pages) // YourPageType 应该替换为你的实体类型
                    .Refresh(Refresh.WaitFor)
                );

                if(!bulkResponse.IsValid)
                {
                    throw new Exception("bulk failed");
                }
                count -= pageSize;
                index++;
            }

        }
        public async Task CommentDbToRedis()
        {
            var count = await _repository.Select.CountAsync();
            var index = 1;
            var pageSize = 1000;
            while (count >= 0)
            {
                var pages = await _repository.Select
                    .IncludeMany(a => a.Resources)
                    .Page(index, pageSize).ToListAsync();
                foreach (var page in pages)
                {
                    var comments =await _commentRepository.Select.Page(index, pageSize).ToListAsync();
                    foreach (var comment in comments)
                    {
                        await RedisComment.AddComment(new CommonAddRedisReq
                        {
                            ArticleId=comment.ArticleId,
                            Id=comment.Id,
                            Content=comment.Content,
                            CreateTime=comment.CreateTime,
                            LikesCount=comment.LikesCount,
                            Uid=comment.Uid
                        });
                    }
                    
                }
                count -= pageSize;
                index++;
            }
        }
        public async Task ClearSearchKey()
        {
            await RedisSearchKey.CleanUpExpiredTerms(new TimeSpan(24, 0, 0));
        }
        public async Task InitPageType()
        {
            var pages = await _repository.Select.ToListAsync();
            foreach (var page in pages)
            {
                await RedisHelper.cli.LPushAsync($"PageType:{page.Tags}", page.id);
            }
            
        }
    }
}
