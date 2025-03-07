using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DbModels;
using DbModels.BaseModel;
using DotNetCore.CAP;
using FreeSql;
using Middleware.Cap.Mq;
using Middleware.Redis;
using Nest;
using PageUnitOfWork.Inferface;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace PageUnitOfWork.UnitOfWokes
{
    public class CommentUOW:ICommentUOW
    {
        private readonly IBaseRepository<Comment> _comrepository;
        private readonly ICapPublisher _capPublisher;

        public CommentUOW(IBaseRepository<Comment> comrepository,ICapPublisher capPublisher)
        {
            this._comrepository = comrepository;
            this._capPublisher = capPublisher;
        }

        public async Task<DefaultMesRes> AddComment(CommonAddReq addReq)
        {
            // 正则表达式匹配 <a> 标签中的 id 属性  
            addReq.Content=addReq.Content.Replace("#","@");
            addReq.Content = addReq.Content.Replace("@0984e3", "#0984e3");
            Regex regex = new Regex(@"<a [^>]*id=""([^""]*)""[^>]*>");
            MatchCollection matches = regex.Matches(addReq.Content);

            List<long> ids = new List<long>();

            // 遍历匹配结果并提取 id  
            foreach(Match match in matches)
            {
                ids.Add(long.Parse( match.Groups[1].Value));
            }
            if(ids.Count > 0)
            {
                await _capPublisher.PublishAsync("Message.Metio", new MetionMqDto
                {
                    Ids = ids,
                    Uid = addReq.Uid,
                    PageId = addReq.ArticleId
                });
            }
            using(var uow = _comrepository.Orm.CreateUnitOfWork())
            {
                try
                {
                    var comment = await _comrepository.InsertAsync(new Comment
                    {
                        Content = addReq.Content,
                        ArticleId = addReq.ArticleId,
                        LikesCount = 0,
                        Uid = addReq.Uid
                    });
                    uow.Commit();
                    await _capPublisher.PublishAsync("comment.add", new CommonAddRedisReq
                    {
                        Content = addReq.Content,
                        Uid = addReq.Uid,
                        ArticleId = addReq.ArticleId,
                        Id = comment.Id,
                        LikesCount = 0,
                        CreateTime = comment.CreateTime
                    });
                    return new DefaultMesRes
                    {
                        Code = 200,
                        Data = comment,
                        Message = "评论成功"
                    };
                }
                catch(Exception ex)
                {
                    uow.Rollback();
                    throw ex;
                }
            }
           
           
        }

        public async Task<DefaultMesRes> LikeComment(long userId, long commentId)
        {
            var comment = await RedisComment.LikeComment(userId, commentId.ToString());
            return new DefaultMesRes
            {
                Code = comment ? 200 : 400,
                Message = comment ? "点赞成功" : "点赞失败"
            };
        }
        public async Task<DefaultMesRes> UnLikeComment(long userId, long commentId)
        {
            var comment = await RedisComment.UnlikeComment(userId, commentId.ToString());
            return new DefaultMesRes
            {
                Code = comment ? 200 : 400,
                Message = comment ? "取消点赞成功" : "取消点赞失败"
            };
        }

        #region 查询
        public async Task<List<CommentRes>> GetCommentByArticleId(CommentReq req)
        {
            var redisData=await RedisComment.GetAllCommentsByArticleId(req.ArticleId.ToString(), req.PageIndex, req.PageSize, req.Uid);
            var result=new List<CommentRes>();
            foreach (var item in redisData)
            {
                result.Add(new CommentRes
                {
                    common = item,
                    user = JsonSerializer.Deserialize<CommentUserRes>
                    (await RedisHelper.cli.GetAsync("UserDb." + item.Uid.ToString())) ?? new CommentUserRes(),

                });
            }
            return result;
        }
        public async Task<long>GetCommentCount(long commentId)
        {
            return await RedisComment.GetCommentCount(commentId);
        }
        #endregion

        #region 删除
        public async Task<DefaultMesRes> DeleteComment(long userId,long commentId)
        {
            await _capPublisher.PublishAsync("Delete.Comment", new DeleteCommentMqDto
            {
                Uid = userId,
                CommentId = commentId
            });
            return new DefaultMesRes
            {
                Code = 200,
                Message = "删除成功"
            };
        }
        #endregion
    }
}
