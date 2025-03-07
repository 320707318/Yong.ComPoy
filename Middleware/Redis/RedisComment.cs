using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Middleware.Redis
{
    public static class RedisComment
    {
        public static async Task AddComment(CommonAddRedisReq comment)
        {
            await RedisHelper.cli.SetAsync($"comment:{comment.Id}", JsonSerializer. Serialize (comment));
            await RedisHelper.cli.ZAddAsync($"comments:likes:{comment.ArticleId}", comment.LikesCount, comment.Id.ToString());
        }

        public static CommonRedisRes GetComment(string id,long uid)
        {
            var res=JsonSerializer.Deserialize< CommonRedisRes>(RedisHelper.cli.Get($"comment:{id}"));
            res.IsLike = RedisHelper.cli.SIsMember($"user:likes:comment:{uid}", id);
            return res;
        }
        public static async Task< IEnumerable<CommonRedisRes>> GetAllCommentsByArticleId(string articleId,int index,int size,long uid)
        {
            // 获取指定文章 ID 的所有评论的 ID，并按点赞数排序  
            var ids =await RedisHelper.cli.ZRevRangeAsync($"comments:likes:{articleId}",(index-1)*size,size*index);
            return ids.Select(id => GetComment(id,uid)).ToList();
        }

        public static async Task<bool> LikeComment(long userId, string commentId)
        {
            if(! await RedisHelper.cli.SIsMemberAsync($"user:likes:comment:{userId}",commentId))
            {
                var comment = GetComment(commentId, userId);
                if(comment != null)
                {
                    string articleId = comment.ArticleId.ToString();
                    comment.LikesCount++;
                    await RedisHelper.cli.SetAsync($"comment:{commentId}",JsonSerializer.Serialize( comment));
                    await RedisHelper.cli.ZAddAsync($"comments:likes:{articleId}", comment.LikesCount, commentId);
                    await RedisHelper.cli.SAddAsync($"user:likes:comment:{userId}", commentId); // 保存用户的点赞记录  
                    return true; // 成功点赞  
                }
            }
            return false; // 用户已点赞，未成功  
        }

        public static async Task<bool> UnlikeComment(long userId, string commentId)
        {
            if(await RedisHelper.cli.SIsMemberAsync($"user:likes:comment:{userId}", commentId))
            {
                var comment = GetComment(commentId, userId);
                if(comment != null && comment.LikesCount > 0)
                {
                    string articleId = comment.ArticleId.ToString();
                    comment.LikesCount--;
                    await RedisHelper.cli.SetAsync($"comment:{commentId}", JsonSerializer.Serialize(comment));
                    await RedisHelper.cli.ZAddAsync($"comments:likes:{articleId}", comment.LikesCount, commentId);
                    await RedisHelper.cli.SRemAsync($"user:likes:comment:{userId}", commentId); // 移除用户的点赞记录  
                    return true; // 成功取消点赞  
                }
            }
            return false; // 用户未点赞，未成功取消  
        }
        public static async Task DeleteComment(long userId, long commentId,long pageid)
        {
            await RedisHelper.cli.DelAsync($"comment:{commentId}");
            await RedisHelper.cli.ZRemAsync($"comments:likes:{pageid}", commentId.ToString());
        }
        public static async Task<long> GetCommentCountByArticleId(long articleId)
        {
            return await RedisHelper.cli.ZCardAsync($"comments:likes:{articleId}");
        }
        public static async Task<long>GetCommentCount(long pageid)
        {
            return await RedisHelper.cli.ZCardAsync($"comments:likes:{pageid}");
        }
    }
}
