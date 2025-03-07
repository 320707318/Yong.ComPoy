using System.Buffers;
using System.ComponentModel;
using System.Text.Json;
using DbModels;
using DbModels.BaseModel;
using DotNetCore.CAP;
using FreeSql;
using IPTools.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Middleware.Cap.Mq;
using Middleware.OBS;
using Middleware.Redis;
using Nest;
using Org.BouncyCastle.Ocsp;
using PageUnitOfWork.Inferface;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace PageUnitOfWork.UnitOfWokes
{
    public class PageUOW : IPageUOW
    {
        private readonly IBaseRepository<DbModels.Page> _repository;
        private readonly ICapPublisher _capPublisher;
        private readonly ElasticClient _client;
        private readonly IBaseRepository<SuperTopic> _srepository;

        public PageUOW(IBaseRepository<DbModels.Page> repository, ICapPublisher capPublisher, ElasticClient client,IBaseRepository<SuperTopic> srepository)
        {
            this._repository = repository;
            this._capPublisher = capPublisher;
            this._client = client;
            this._srepository = srepository;
        }
        #region 查询
        public async Task<PageRes> GetPage(long id, long userId)
        {
            var page = await _repository.Select
                .Include(a => a.Author)
                .IncludeMany(a => a.Resources)
                .Where(a => a.id == id)
                .ToOneAsync();
            var res = new PageRes
            {
                id = page.id,
                Title = page.Title,
                Content = page.Content,
                Author = new UserRes
                {
                    id = page.Author.Id,
                    Gender = page.Author.Gender,
                    Name = page.Author.NickName,
                    UserName = page.Author.UserName,
                    Avatar = page.Author.Avatar,
                    IsAttention = await RedisHelper.IsFollowingAsync(userId, page.Author.Id)
                },
                Resources = page.Resources.Select(r => new ResourceRes
                {
                    Url = r.Url,
                    Type = r.Type,
                    Sort = r.Sort,
                    Status = r.Status,
                    Id = r.Id,
                    Name = r.Name,
                }).OrderBy(r => r.Sort).ToList()
            };
            //await _capPublisher.PublishAsync("Set.UserType", new UserTypeMqDto { Tag = page.Tags, Uid = page.Author.Id });
            return res;
        }
        public async Task<PageRes> GetPage(long id)
        {
            var pageIndex=await _client.SearchAsync<PageRes>(s =>
                s.Index("page")
                .From(0)
                .Size(1)
                .Query(q => q.Match(m => m.Field("id").Query(id.ToString())))
            );
            if (!pageIndex.IsValid)
            {
                await Console.Out.WriteLineAsync();
            }
            var page = pageIndex.Documents.First();
            var user = JsonSerializer.Deserialize<UserRes>(await RedisHelper.cli.GetAsync("UserDb." + page.AuthorId));
            user.IsAttention = await RedisHelper.IsFollowingAsync(0, page.AuthorId);
            user.id = page.AuthorId;
            page.Author = user;
            page.IsLike = await RedisPage.IsPageLike(page.id, 0);
            page.LikeCount = await RedisPage.GetPageLikeCount(page.id);
            page.CommentCount = await RedisComment.GetCommentCountByArticleId(page.id);
            
            return page;
        }
        public async Task<List<PageRes>> GetPageAll(PageSearchReq req)
        {
            var page = await _repository.Select
                .Include(a => a.Author)
                .IncludeMany(a => a.Resources)
                .WhereIf(req.KeyWord.Length >= 1, a => a.Title.Contains(req.KeyWord) || a.Content.Contains(req.KeyWord))
                .OrderByDescending(a => a.CreateTime)
                .Page(req.PageIndex, req.PageSize)
                .ToListAsync();
            var res = new List<PageRes>();
            foreach(var item in page)
            {
                var pageRes = new PageRes
                {
                    id = item.id,
                    Title = item.Title,
                    Content = item.Content,
                    IsLike = await RedisPage.IsPageLike(item.id, req.Uid),
                    LikeCount = await RedisPage.GetPageLikeCount(item.id),
                    CreateTime = item.CreateTime,
                    CommentCount = await RedisComment.GetCommentCountByArticleId(item.id),
                    Author = new UserRes
                    {
                        id = item.Author.Id,
                        Gender = item.Author.Gender,
                        Name = item.Author.NickName,
                        UserName = item.Author.UserName,
                        Avatar = item.Author.Avatar,
                        IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, item.Author.Id)
                    },
                    Resources = item.Resources.Select(r => new ResourceRes
                    {
                        Url = r.Url,
                        Type = r.Type,
                        Sort = r.Sort,
                        Status = r.Status,
                        Id = r.Id,
                        Name = r.Name,
                    }).OrderBy(r => r.Sort).ToList()
                };
                res.Add(pageRes);
            }
            return res;
        }
                
        public async Task<List<PageRes>> GetPageByTag(PageTopicReq req)
        {
            var pageIndex = await _client.SearchAsync<PageRes>(s =>
               s.Index("page")
               .From((req.PageIndex - 1) * req.PageSize)
               .Size(req.PageSize)
               .Query(q=>q.Nested(n => n.Path(p => p.SuperIds)
                    .Query(nq => nq.Term(m => m.SuperIds.First().Id, req.TopicId.ToString())))
               )

           );
            if(!pageIndex.IsValid)
            {
                await Console.Out.WriteLineAsync();
            }
            foreach(var item in pageIndex.Documents)
            {
                var user = JsonSerializer.Deserialize<UserRes>(await RedisHelper.cli.GetAsync("UserDb." + item.AuthorId));
                user.IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, item.AuthorId);
                user.id = item.AuthorId;
                item.Author = user;
                item.IsLike = await RedisPage.IsPageLike(item.id, req.Uid);
                item.LikeCount = await RedisPage.GetPageLikeCount(item.id);
                item.CommentCount = await RedisComment.GetCommentCountByArticleId(item.id);
            }

            return pageIndex.Documents.ToList();
        }
        public async Task<List<PageRes>> GetHotRecom(PageHotReq req)
        {
            var tags = (await RedisHelper.cli.LRangeAsync($"UserType:{req.Uid}", 0, -1)).Where(s=>!string.IsNullOrEmpty(s)).Distinct().ToList() ;
            var pageIndex = await _client.SearchAsync<PageRes>(s =>
                s.Index("page")
                .From((req.PageIndex - 1) * req.PageSize)
                .Size(req.PageSize)
                .Sort(so => so.Descending(p => p.CreateTime))
                .Query(q => q.Bool(b=>
                b.Should(
                    tags.ConvertAll(value =>
                            (Func<QueryContainerDescriptor<PageRes>, QueryContainer>)
                                (qdc => qdc.Match(m => m.Field("tags").Query(value)))
                        ).ToArray()
                    )
                ) && q.Term(t => t.Status, 0))
            
            );
            if(!pageIndex.IsValid)
            {
                await Console.Out.WriteLineAsync();
            }
            foreach(var item in pageIndex.Documents)
            {
                var user = JsonSerializer.Deserialize<UserRes>(await RedisHelper.cli.GetAsync("UserDb." + item.AuthorId));
                user.IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, item.AuthorId);
                user.id = item.AuthorId;
                item.Author = user;

                item.IsLike = await RedisPage.IsPageLike(item.id, req.Uid);
                item.LikeCount = await RedisPage.GetPageLikeCount(item.id);
                item.CommentCount = await RedisComment.GetCommentCountByArticleId(item.id);
            }

            return pageIndex.Documents.ToList();
        }

        public async Task<List<PageRes>> GetPageByIp(PageIpReq req)
        {
            var pageIndex = await _client.SearchAsync<PageRes>(s =>
                s.Index("page")
                .From((req.PageIndex-1) * req.PageSize)
                .Size(req.PageSize)
                .Sort(so=>so.Descending(p=>p.CreateTime))
                .Query(q => q.Match(m => m.Field("ip").Query(req.Ip)) && q.Term(t=>t.Status,0))

            );
            if(!pageIndex.IsValid)
            {
                await Console.Out.WriteLineAsync();
            }
            foreach(var item in pageIndex.Documents)
            {
                var user = JsonSerializer.Deserialize<UserRes>( await RedisHelper.cli.GetAsync("UserDb." + item.AuthorId));
                user.IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, item.AuthorId);
                user.id = item.AuthorId;
                item.Author = user;

                item.IsLike = await RedisPage.IsPageLike(item.id, req.Uid);
                item.LikeCount = await RedisPage.GetPageLikeCount(item.id);
                item.CommentCount = await RedisComment.GetCommentCountByArticleId(item.id);
            }

            return pageIndex.Documents.ToList();
        }
        public async Task<List<PageRes>> GetPageByRealTime(PageSearchReq req)
        {
            if(req.KeyWord.Length >= 3 && req.PageIndex == 1)
            {
                await _capPublisher.PublishAsync("Add.KeyWord", req.KeyWord);
            }
            var res = string.IsNullOrEmpty(req.KeyWord) ? await _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                .Query(q=>q.Term(t => t.Status, 0))
               .Sort(so => so.Descending(p => p.CreateTime))
               .From((req.PageIndex-1) * req.PageSize)
               .Size(req.PageSize)
            ) :
            await _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                .Query(q => q.Match(m => m.Field(f => f.Content).Query(req.KeyWord)) && q.Term(t => t.Status, 0))
               .Sort(so => so.Descending(p => p.CreateTime))
               .From((req.PageIndex-1) * req.PageSize)
               .Size(req.PageSize)
            );
            var pageRes = new List<PageRes>();
            foreach(var item in res.Documents)
            {
                var user = JsonSerializer.Deserialize<Users>(await RedisHelper.cli.GetAsync("UserDb." + item.AuthorId));
                var userRes = new UserRes
                {
                    id = user.Id,
                    Gender = user.Gender,
                    Name = user.NickName,
                    UserName = user.UserName,
                    Avatar = user.Avatar,
                    IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, user.Id)
                };
                var page = new PageRes
                {
                    id = item.id,
                    Title = item.Title,
                    Ip = item.Ip,
                    Content = item.Content,
                    LikeCount = await RedisPage.GetPageLikeCount(item.id),
                    Cover = item.Cover,
                    CoverType = item.CoverType,
                    IsLike = await RedisPage.IsPageLike(item.id, req.Uid),
                    CreateTime = item.CreateTime,
                    CommentCount = await RedisComment.GetCommentCountByArticleId(item.id),
                    Resources = item.Resources.Select(r => new ResourceRes
                    {
                        Url = r.Url,
                        Type = r.Type,
                        Sort = r.Sort,
                        Status = r.Status,
                        Id = r.Id,
                        Name = r.Name,
                    }).OrderBy(r => r.Sort).ToList(),
                    Author = userRes
                };
                pageRes.Add(page);
            }
            return pageRes;
        }
        public async Task<List<PageRes>> GetPageIntegrated(PageSearchReq req)
        {
            if(req.KeyWord.Length >= 3 && req.PageIndex == 1)
            {
                await _capPublisher.PublishAsync("Add.KeyWord", req.KeyWord);
            }
            var res = await _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                .Query(q => q.Match(m => m.Field(f => f.Content).Query(req.KeyWord)) && q.Term(t => t.Status, 0))
               .Sort(so => so.Descending(p => p.LikeCount))
               .From((req.PageIndex-1) * req.PageSize)
               .Size(req.PageSize)
            );
            var pageRes = new List<PageRes>();
            foreach(var item in res.Documents)
            {
                var user = JsonSerializer.Deserialize<Users>(await RedisHelper.cli.GetAsync("UserDb." + item.AuthorId));
                var userRes = new UserRes
                {
                    id = user.Id,
                    Gender = user.Gender,
                    Name = user.NickName,
                    UserName = user.UserName,
                    Avatar = user.Avatar,
                    IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, user.Id)
                };
                var page = new PageRes
                {
                    id = item.id,
                    Title = item.Title,
                    Content = item.Content,
                    Ip = item.Ip,
                    Cover = item.Cover,
                    CoverType = item.CoverType,
                    LikeCount = await RedisPage.GetPageLikeCount(item.id),
                    IsLike = await RedisPage.IsPageLike(item.id, req.Uid),
                    CreateTime = item.CreateTime,
                    CommentCount = await RedisComment.GetCommentCountByArticleId(item.id),
                    Resources = item.Resources.Select(r => new ResourceRes
                    {
                        Url = r.Url,
                        Type = r.Type,
                        Sort = r.Sort,
                        Status = r.Status,
                        Id = r.Id,
                        Name = r.Name,
                    }).OrderBy(r => r.Sort).ToList(),
                    Author = userRes,
                };
                pageRes.Add(page);
            }
            return pageRes;
     }
        public async Task<List<PageRes>> GetPageByFollow(PageSearchReq req)
    {
            if(req.KeyWord.Length >= 3 && req.PageIndex == 1)
            {
                await _capPublisher.PublishAsync("Add.KeyWord", req.KeyWord);
            }
            var followIds = await RedisHelper.GetFollowersAsync(req.Uid);
            if(followIds.Count == 0)
            {
                return new List<PageRes>();
            }
            var res = string.IsNullOrEmpty(req.KeyWord) ? await _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.AuthorId) 
                        .Terms(followIds)  )
                    && q.Term(t => t.Status, 0)
                )
               .Sort(so => so.Descending(p => p.CreateTime))
               .From((req.PageIndex-1) * req.PageSize)
               .Size(req.PageSize)
            ) :
            await _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                .Query(q => q
                        .Bool(b => b
                            .Must(
                                mu => mu.Terms(t => t
                                    .Field(f => f.AuthorId)
                                    .Terms(followIds)
                                ),
                                mu=>mu.Term(t=>t.Status,0),
                                mu => mu.Match(m => m
                                    .Field(f => f.Content)
                                    .Query(req.KeyWord)
                                )))
                )
               .Sort(so => so.Descending(p => p.CreateTime))
               .From((req.PageIndex-1) * req.PageSize)
               .Size(req.PageSize)
            );
            var pageRes = new List<PageRes>();
            foreach(var item in res.Documents)
            {
                var user = JsonSerializer.Deserialize<Users>(await RedisHelper.cli.GetAsync("UserDb." + item.AuthorId));
                var userRes = new UserRes
                {
                    id = user.Id,
                    Gender = user.Gender,
                    Name = user.NickName,
                    UserName = user.UserName,

                    Avatar = user.Avatar,
                    IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, user.Id)
                };
                var page = new PageRes
                {
                    id = item.id,
                    Title = item.Title,
                    Ip = item.Ip,
                    Content = item.Content,
                    LikeCount = await RedisPage.GetPageLikeCount(item.id),
                    IsLike = await RedisPage.IsPageLike(item.id, req.Uid),
                    CreateTime = item.CreateTime,
                    Cover = item.Cover,
                    CommentCount = await RedisComment.GetCommentCountByArticleId(item.id),
                    Resources = item.Resources.Select(r => new ResourceRes
                    {
                        Url = r.Url,
                        Type = r.Type,
                        Sort = r.Sort,
                        Status = r.Status,
                        Id = r.Id,
                        Name = r.Name,
                    }).OrderBy(r => r.Sort).ToList(),
                    Author = userRes,
                };
                pageRes.Add(page);
            }
            return pageRes;
        }
        public async Task<List<PageRes>> GetPageByFriends(PageSearchReq req)
        {
            if(req.KeyWord.Length >= 3 && req.PageIndex == 1)
            {
                await _capPublisher.PublishAsync("Add.KeyWord", req.KeyWord);
            }
            var followIds = await RedisHelper.GetFriendsAsync(req.Uid);
            if(followIds.Count == 0)
            {
                return new List<PageRes>();
            }
            var res = string.IsNullOrEmpty(req.KeyWord) ? await _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.AuthorId)
                        .Terms(followIds))
                    && q.Term(t => t.Status, 0)
                )
               .Sort(so => so.Descending(p => p.CreateTime))
               .From((req.PageIndex - 1) * req.PageSize)
               .Size(req.PageSize)
            ) :
            await _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                .Query(q => q
                        .Bool(b => b
                            .Must(
                                mu => mu.Terms(t => t
                                    .Field(f => f.AuthorId)
                                    .Terms(followIds)
                                ),
                                mu => mu.Match(m => m
                                    .Field(f => f.Content)
                                    .Query(req.KeyWord)
                                )))
                )
               .Sort(so => so.Descending(p => p.CreateTime))
               .From((req.PageIndex) * req.PageSize)
               .Size(req.PageSize)
            );
            var pageRes = new List<PageRes>();
            foreach(var item in res.Documents)
            {
                var user = JsonSerializer.Deserialize<Users>(await RedisHelper.cli.GetAsync("UserDb." + item.AuthorId));
                var userRes = new UserRes
                {
                    id = user.Id,
                    Gender = user.Gender,
                    Name = user.NickName,
                    UserName = user.UserName,

                    Avatar = user.Avatar,
                    IsAttention = await RedisHelper.IsFollowingAsync(req.Uid, user.Id)
                };
                var page = new PageRes
                {
                    id = item.id,
                    Title = item.Title,
                    Ip = item.Ip,
                    Content = item.Content,
                    Cover = item.Cover,
                    LikeCount = await RedisPage.GetPageLikeCount(item.id),
                    IsLike = await RedisPage.IsPageLike(item.id, req.Uid),
                    CreateTime = item.CreateTime,
                    CommentCount = await RedisComment.GetCommentCountByArticleId(item.id),
                    Resources = item.Resources.Select(r => new ResourceRes
                    {
                        Url = r.Url,
                        Type = r.Type,
                        Sort = r.Sort,
                        Status = r.Status,
                        Id = r.Id,
                        Name = r.Name,
                    }).OrderBy(r => r.Sort).ToList(),
                    Author = userRes,
                };
                pageRes.Add(page);
            }
            return pageRes;
        }
        public async Task<List<string>> GetSerchKeyRank()
        {
            return await RedisSearchKey.GetSearchKeyRank();
        }
        public async Task<List<PageRes>> GetPageByUid(BasePagination pagination,long id,long uid)
        {
            var  Page=await  _client.SearchAsync<DbModels.Page>(s => s.Index("page")
                
                .Query(q => q.Match(m => m.Field(f => f.AuthorId).Query(id.ToString())))
                .Sort(so => so.Descending(p => p.CreateTime))
                .From((pagination.PageIndex - 1) * pagination.PageSize)
                .Size(pagination.PageSize)
            );
            var res=new List<PageRes>();
            if(Page.Documents.Count == 0)
            {
                return res;
            }
            var user = JsonSerializer.Deserialize<Users>(await RedisHelper.cli.GetAsync("UserDb." +id));
            var userRes = new UserRes
            {
                id = user.Id,
                Gender = user.Gender,
                Name = user.NickName,
                UserName = user.UserName,
                Avatar = user.Avatar,
                IsAttention = await RedisHelper.IsFollowingAsync(uid, user.Id)
            };
            foreach (var item in Page.Documents)
            {
                var page = new PageRes
                {
                    id = item.id,
                    Title = item.Title,
                    Ip = item.Ip,
                    Content = item.Content,
                    Status=item.Status,
                    LikeCount = await RedisPage.GetPageLikeCount(item.id),
                    IsLike = await RedisPage.IsPageLike(item.id, 0),
                    CreateTime = item.CreateTime,
                    Cover = item.Cover,
                    CommentCount = await RedisComment.GetCommentCountByArticleId(item.id),
                    Resources = item.Resources.Select(r => new ResourceRes
                    {
                        Url = r.Url,
                        Type = r.Type,
                        Sort = r.Sort,
                        Status = r.Status,
                        Id = r.Id,
                        Name = r.Name,
                    }).OrderBy(r => r.Sort).ToList(),
                    Author = userRes,
                };
                res.Add(page);
            }
            return res.OrderByDescending(p => p.CreateTime).ToList();
        }
        #endregion

        #region 添加
        public async Task<DefaultMesRes> AddPage(PageAddReq addReq)
        {
            
            using(var uow = _repository.Orm.CreateUnitOfWork())
            {
                try
                {
                    var specialTopics = addReq.SuperTopics.Where(s =>! RedisHelper.IsInTopicAsync(addReq.AuthorId, s)).ToList();
                    var taskAll=specialTopics.Select(s=>RedisHelper.JoinTopicAsync(addReq.AuthorId,s));

                    Task.WaitAll(taskAll.ToArray());

                    var page = new DbModels.Page
                    {
                        Title = addReq.Title,
                        Content = addReq.Content,
                        AuthorId = addReq.AuthorId,
                        Cover=addReq.Cover,
                        CoverType=addReq.CoverType,
                        SuperTopics = string.Join(",", addReq.SuperTopics),
                        Topics = string.Join(",", addReq.Topics),
                        Ip=addReq.Ip,
                    };
                    var pageDto = await uow.GetRepository<DbModels.Page>().InsertAsync(page);
                    var resouces = addReq.Resources.Select(r => new Resource
                    {
                        Url = r.Url,
                        PageId = pageDto.id,
                        Type = r.Type,
                        Description = r.Description,
                        Name = r.Name,
                    });
                    await uow.GetRepository<Resource>().InsertAsync(resouces);

                    uow.Commit();
                    await _capPublisher.PublishAsync("Set.PageType", new SetPageTagMqDto
                    {
                        PageId = pageDto.id,
                        Urls = addReq.Resources.Where(s => s.Type == 0).Select(r => r.Url).ToList()
                    });
                    return new DefaultMesRes
                    {
                        Code = 200,
                        Message = "发布成功"
                    };
                }
                catch(Exception ex)
                {
                    uow.Rollback();
                    return new DefaultMesRes
                    {
                        Code = 500,
                        Message = ex.Message
                    };
                }
            }
        }
        #endregion

        #region 删除
        public async Task<DefaultMesRes> DeletePage(long id,long userId)
        {
            await _capPublisher.PublishAsync("Delete.Page", new DeletePageMqDto
            {
                PageId = id,
                Uid = userId
            });
            return new DefaultMesRes
            {
                Code = 200,
                Message = "删除成功"
            };
        }
        #endregion

        #region 修改
        public async Task<DefaultMesRes> UpdatePage(PageUpdateReq updateReq)
        {
            await _capPublisher.PublishAsync("Update.Page", updateReq);
            return new DefaultMesRes
            {
                Code = 200,
                Message = "修改成功"
            };
        }
        #endregion

        #region Other
        public async Task AccessPage(long id, long userId)
        {
            await _capPublisher.PublishAsync("Set.UserType", new UserTypeMqDto { Uid = userId, PageId = id });
        }
        public async Task<string> UpLoadResource(IFormFile file)
        {
            if(file.ContentType != "image/jpeg" && file.ContentType != "image/png" && file.ContentType != "video/mp4" && file.ContentType != "video/x-ms-wmv" && file.ContentType != "video/x-msvideo")
            {
                throw new Exception("资源格式不正确");
            }
            if(file.Length > 1024 * 1024 * 6)
            {
                throw new Exception("资源大小不能超过11M");
            }
            byte[] stream = new byte[file.Length];
            await file.OpenReadStream().ReadAsync(stream, 0, (int)file.Length);
            var name = "resource/" + new Random().Next(0, 999999) + file.FileName;
            await _capPublisher.PublishAsync("Upload.Resource", new ObsMqReq
            {
                File=stream,
                Name=name
            });



            return "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/" + name;

        }
        public async Task<DefaultMesRes> PageLike( long pageId, long userId)
        {
            await _capPublisher.PublishAsync("Like.Page",new LikePageMqDto
            {
                PageId = pageId,
                Uid = userId
            });
            return new DefaultMesRes
            {
                Code = 200,
                Message = "点赞成功"
            };
        }
        public async Task<DefaultMesRes> PageDisLike(long pageId, long userId)
        { 
            await _capPublisher.PublishAsync("DisLike.Page", new LikePageMqDto
            {
                PageId = pageId,
                Uid = userId
            });
            return new DefaultMesRes
            {
                Code = 200,
                Message = "取消点赞成功"
            };
        }
        #endregion
    }
}
