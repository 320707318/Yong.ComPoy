using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FreeRedis;

namespace Middleware.Redis
{
    public static class RedisHelper
    {
       public static RedisClient cli = new RedisClient("127.0.0.1:6379,password=123456,defaultDatabase=1");

        #region 关注
        /// <summary>
        /// 用户A关注用户B
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="followedUserId"></param>
        /// <returns></returns>
        public static async Task FollowAsync(long userId, long followedUserId)
        {
            // 用户A关注用户B  
            await cli.SAddAsync($"user:followers:{userId}", followedUserId);
            await cli.SAddAsync($"user:followings:{followedUserId}", userId);
        }
        /// <summary>
        /// 用户A取消关注用户B
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="followedUserId"></param>
        /// <returns></returns>
        public static async Task UnfollowAsync(long userId, long followedUserId)
        {
            // 用户A取消关注用户B  
            await cli.SRemAsync($"user:followers:{userId}", followedUserId);
            await cli.SRemAsync($"user:followings:{followedUserId}", userId);
        }
        /// <summary>
        /// 检查用户A是否关注用户B
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="followedUserId"></param>
        /// <returns></returns>
        public static async Task<bool> IsFollowingAsync(long userId, long followedUserId)
        {
            // 检查用户A是否关注用户B  
            var res= await cli.SIsMemberAsync($"user:followers:{userId}", followedUserId);
            return res;
        }

        /// <summary>
        /// 检查用户A和用户B是否互相关注
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="otherUserId"></param>
        /// <returns></returns>
        public static async Task<bool> AreMutuallyFollowingAsync(long userId, long otherUserId)
        {
            // 检查用户A和用户B是否互相关注  
            return await cli.SIsMemberAsync($"user:followers:{userId}", otherUserId) &&
                   await cli.SIsMemberAsync($"user:followers:{otherUserId}", userId);
        }

        /// <summary>
        /// 获取用户A的关注者列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<List<long>> GetFollowersAsync(long userId)
        {
            return (await cli.SMembersAsync($"user:followers:{userId}")).Select(x => long.Parse(x)).ToList();
        }
        /// <summary>
        /// 获取用户A的粉丝列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<List<long>> GetFansAsync(long userId)
        {
            return (await cli.SMembersAsync($"user:followings:{userId}")).Select(x => long.Parse(x)).ToList();
        }

        /// <summary>
        /// 获取互关列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<List<long>> GetFriendsAsync(long userId)
        {
            // 获取互关列表  
            return (await cli.SInterAsync($"user:followers:{userId}", $"user:followings:{userId}")).Select(x => long.Parse(x)).ToList();
        }

        public static async Task<RedisUserDataRes> GetUserDataAsync(long userId)
        {
            RedisUserDataRes res = new RedisUserDataRes();
            res.followerCount = (await GetFollowersAsync(userId)).Count;
            res.fansCount = (await GetFansAsync(userId)).Count;
            res.friendCount = (await GetFriendsAsync(userId)).Count;
            return res;
        }
        #endregion

        #region 超话
        /// <summary>
        /// 加入超话
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public static async Task<bool> JoinTopicAsync(long userId, long topicId)
        {
            //检查用户是否已在超话中
            if (await cli.SIsMemberAsync($"user:topics:{topicId}", userId))
            {
                return false;
            }
            // 用户加入超话
             await cli.SAddAsync($"user:topics:{topicId}", userId);
            await cli.SAddAsync($"user:jointopic:{userId}", topicId);
            return true;
        }

        /// <summary>
        /// 退出超话
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public static async Task<bool> OutTopicAsync(long userId, long topicId)
        {
            //检查用户是否已在超话中
            if(await cli.SIsMemberAsync($"user:topics:{topicId}", userId))
            {
                // 用户退出超话
                await cli.SRemAsync($"user:topics:{topicId}", userId);
                await cli.SRemAsync($"user:jointopic:{userId}", topicId);
                return true;
            }
            
            return false;
        }

        public static async Task<bool> DissolutionAsync(long topicId)
        {
            //超话解散
            await cli.DelAsync($"user:topics:{topicId}");
            var uids =await RedisHelper.GetTopicMembersAsync(topicId);
            foreach (var item in uids)
            {
                await cli.SRemAsync($"user:jointopic:{item}", topicId);
            }
            return true;
        }
        /// <summary>
        /// 检查用户是否在超话中
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public static  bool IsInTopicAsync(long userId, long topicId)
        {
            // 检查用户是否在超话中
            return  cli.SIsMember($"user:topics:{topicId}", userId);
        }
        //获取超话成员数量
        public static async Task<long> GetTopicCountAsync(long topicId)
        {
            return await cli.SCardAsync($"user:topics:{topicId}");
        }
        //获取超话成员列表
        public static async Task<List<long>> GetTopicMembersAsync(long topicId)
        {
            return (await cli.SMembersAsync($"user:topics:{topicId}")).Select(x => long.Parse(x)).ToList();
        }
        //获取用户加入的超话列表
        public static async Task<List<long>> GetUserTopicsAsync(long userId)
        {
            return (await cli.SMembersAsync($"user:jointopic:{userId}")).Select(x => long.Parse(x)).ToList();
        }
        #endregion

        public static async Task<List<long>> GetPageIds(PageIdsReq idsReq)
        {
            var ids=new List<long>();
            idsReq.pageTypes.ForEach(x =>
            {
                RedisHelper.cli.LRange($"PageType:{x.Name}", x.Count * (idsReq.PageIndex-1), x.Count * (idsReq.PageIndex ))
                .ToList().
                ForEach(y => ids.Add(long.Parse(y)));
            });
            Random rng = new Random();
            return ids.OrderBy(x => rng.Next()).ToList();
        }
        public static async Task<List<PageTypeWeight>> GetPageTypeWeight(long uid,int size)
        {
            var redisData = (await RedisHelper.cli.LRangeAsync($"UserType:{uid}", 0, -1));
            var types = redisData.ToList()
                .GroupBy(x => x);
            var res=types
                .Select(s=>new PageTypeWeight
                {
                    Name=s.Key,
                    Count= (int)(size*(double)s.Count() / (double)redisData.Length)
                }).ToList();
            return res;

        }
    } 
}
