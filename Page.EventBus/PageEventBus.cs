using DbModels;
using DotNetCore.CAP;
using FreeSql;
using Middleware.Cap.Mq;
using Middleware.OBS;
using Middleware.OCR;
using Middleware.Redis;
using Nest;
using RestSharp;
using static FreeSql.Internal.GlobalFilter;

namespace Page.EventBus
{
    public class PageEventBus : ICapSubscribe, IPageEventBus
    {
        private readonly IBaseRepository<DbModels.Page> _repository;
        private readonly IBaseRepository<SuperTopic> _baseRepository;
        private readonly IBaseRepository<Comment> _comrepository;
        private readonly IBaseRepository<Resource> _resourceRepository;
        private readonly ElasticClient _elasticClient;

        public PageEventBus(IBaseRepository<DbModels.Page> repository,
            IBaseRepository<SuperTopic> baseRepository,
            IBaseRepository<Comment> comrepository,
            IBaseRepository<Resource> resourceRepository,
            ElasticClient elasticClient
            )
        {
            this._repository = repository;
            this._baseRepository = baseRepository;
            this._comrepository = comrepository;
            this._resourceRepository = resourceRepository;
            this._elasticClient = elasticClient;
        }
        [CapSubscribe("Set.PageType")]
        public async Task SetPageType(SetPageTagMqDto setPage)
        {
            var page = await _repository.Select.Where(s => s.id == setPage.PageId)
                    .Include(s => s.Author)
                    .IncludeMany(s => s.Resources)
                    .FirstAsync();
            page.Tags = string.Join(",", (await TaggingImageHelper.TaggingImage(page.Cover)).Take(2).Select(s => s.Value)); //response.Content;
            var sids = page.SuperTopics.Split(",").Where(s => !string.IsNullOrEmpty(s)).Select(a => long.Parse(a)).ToList();
            page.SuperIds = await _baseRepository.Select.Where(a => sids.Contains(a.Id)).ToListAsync(s => new Supers { Id = s.Id.ToString(), Name = s.Name });
            await _repository.UpdateAsync(page);
            await _elasticClient.IndexAsync<DbModels.Page>(page, d => d.Index("page"));

            var pageList = await _repository.Select.Where(s => s.id == 230)
                    .Include(s => s.Author)
                    .IncludeMany(s => s.Resources)
                    .ToListAsync();
            foreach(var item in pageList)
            {
                try
                {
                 
                    await _elasticClient.IndexAsync<DbModels.Page>(item, d => d.Index("page"));
                  
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
        [CapSubscribe("Set.UserType")]
        public async Task SetUserType(UserTypeMqDto userType)
        {
            var page = await _repository.Select.Where(s => s.id == userType.PageId).FirstAsync();
            var tags = page.Tags.Split(",").Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            if(tags.Count >0)
            {
                foreach(var tag in tags)
                {
                    await RedisHelper.cli.LPushAsync("UserType:" + userType.Uid, tag);
                }
            }
            

            if(await RedisHelper.cli.LLenAsync("UserType:" + userType.Uid) == 22)
            {
                //删除最早的两条
                await RedisHelper.cli.LTrimAsync("UserType:" + userType.Uid, 0, 2);
            }
        }
        [CapSubscribe("SuperTopic.Add")]
        public async Task AddSuperTopic(SuperTopicAddMqDto superTopic)
        {
            var page = await _baseRepository.InsertAsync(new SuperTopic
            {
                AvatarUrl = superTopic.AvatarUrl,
                Name = superTopic.Name,
                CreatorId = superTopic.CreatorId,
                Description = superTopic.Description,
                Number = 1,
                TypeId = superTopic.TypeId
            });
            await RedisHelper.JoinTopicAsync(superTopic.CreatorId, page.Id);

        }
        [CapSubscribe("comment.add")]
        public async Task AddComment(CommonAddRedisReq add)
        {
            await RedisComment.AddComment(add);


        }
        [CapSubscribe("DisLike.Page")]
        public async Task DisLikePage(LikePageMqDto disLike)
        {
            using(var uow = _repository.Orm.CreateUnitOfWork())
            {
                try
                {
                    var page = await _repository.Select.Where(s => s.id == disLike.PageId).FirstAsync();
                    page.LikeCount -= 1;
                    await _repository.UpdateAsync(page);
                    uow.Commit();
                    await RedisPage.UnlikePage(disLike.PageId, disLike.Uid);
                }
                catch(Exception ex)
                {
                    uow.Rollback();
                    throw ex;
                }
            }
        }
        [CapSubscribe("Like.Page")]
        public async Task LikePage(LikePageMqDto like)
        {
            using(var uow = _repository.Orm.CreateUnitOfWork())
            {
                try
                {
                    var page = await _repository.Select.Where(s => s.id == like.PageId).FirstAsync();
                    page.LikeCount += 1;
                    await _repository.UpdateAsync(page);
                    uow.Commit();
                    await RedisPage.LikePage(like.PageId, like.Uid);
                }
                catch(Exception ex)
                {
                    uow.Rollback();
                    throw ex;
                }
            }
        }
        [CapSubscribe("Delete.Page")]
        public async Task DeletePage(DeletePageMqDto delete)
        {
            using(var uow = _repository.Orm.CreateUnitOfWork())
            {
                try
                {
                    var page = await _repository.Select.Where(s => s.id == delete.PageId).FirstAsync();
                    if(page.AuthorId != delete.Uid)
                    {
                        throw new Exception("无权限删除");
                    }

                    var response = await _elasticClient.DeleteAsync<DbModels.Page>(page.id, d => d.Index("page"));
                    if(response.IsValid)
                    {
                        await _repository.DeleteAsync(page);
                        uow.Commit();
                    }
                    else
                    {
                        Console.WriteLine("删除失败: " + response.ServerError.Error.Reason);
                    }
                    
                    //await RedisPage.DeletePage(delete.PageId);
                }
                catch(Exception ex)
                {
                    uow.Rollback();
                    throw ex;
                }
            }
        }
        [CapSubscribe("Delete.Comment")]
        public async Task DeleteComment(DeleteCommentMqDto delete)
        {
            using(var uow = _comrepository.Orm.CreateUnitOfWork())
            {
                try
                {
                    var comment = await _comrepository.Select.Where(s => s.Id == delete.CommentId).FirstAsync();
                    var page = await _repository.Select.Where(s => s.id == comment.ArticleId).FirstAsync();
                    if(comment.Uid != delete.Uid && page.AuthorId != delete.Uid)
                    {
                        throw new Exception("无权限删除");
                    }
                    await _comrepository.DeleteAsync(comment);
                    uow.Commit();
                    await RedisComment.DeleteComment(delete.Uid, delete.CommentId, page.id);
                }
                catch(Exception ex)
                {
                    uow.Rollback();
                    throw ex;
                }
            }
        }
        [CapSubscribe("Update.Page")]
        public async Task UpdatePage(UpdatePageMqDto update)
        {
            using(var uow = _repository.Orm.CreateUnitOfWork())
            {
                try
                {
                    var page = await _repository.Select.
                        Where(s => s.id == update.Id)
                        .IncludeMany(s => s.Resources)
                        .FirstAsync();
                    if(update.AuthorId != page.AuthorId)
                    {
                        throw new Exception("无权限修改");
                    }

                    page.SuperTopics = string.Join(",", update.SuperTopics);
                    page.Content = update.Content;
                    page.Title = update.Title;
                    page.Cover = update.Cover;
                    page.CoverType = update.CoverType;
                    await _repository.UpdateAsync(page);
                    var resources = new List<Resource>();
                    foreach(var item in update.Resources)
                    {
                        var newRes = await _resourceRepository.InsertOrUpdateAsync(new Resource
                        {
                            Url = item.Url,
                            Name = item.Name,
                            Type = item.Type,
                            PageId = update.Id,
                            Sort = item.Sort,
                            Id = item.Id,
                        });
                        resources.Add(newRes);
                    }
                    var delres = page.Resources.Where(s => !update.Resources.Any(t => t.Id == s.Id)).ToList();
                    page.Resources = resources;
                    page.SuperIds = update.SuperTopics.Select(s => new Supers { Id=s.ToString()}).ToList();
                    await _elasticClient.UpdateAsync<DbModels.Page>(page.id, d => d.Index("page").Doc(page));
                    await _resourceRepository.DeleteAsync(delres);
                    uow.Commit();
                    //await RedisPage.UpdatePage(update.PageId, update.Title, update.Content);

                }
                catch(Exception ex)
                {
                    uow.Rollback();
                    throw ex;
                }
            }
        }
        [CapSubscribe("Add.KeyWord")]
        public async Task AddKeyWord(string addKey)
        {
            await RedisSearchKey.AddSearchKey(addKey);
        }
        [CapSubscribe("Upload.Resource")]
        public async Task UploadResource(ObsMqReq obs)
        {
            using var stream = new MemoryStream(obs.File);
            await ObsHelper.UploadImageFile(stream, obs.Name);
        }
    }
}
