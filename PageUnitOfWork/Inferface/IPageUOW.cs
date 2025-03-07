using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels;
using DbModels.BaseModel;
using Microsoft.AspNetCore.Http;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace PageUnitOfWork.Inferface
{
    public interface IPageUOW
    {
        public Task<DefaultMesRes> AddPage(PageAddReq addReq);
        public  Task<PageRes> GetPage(long id, long userId);
        public Task<List<PageRes>> GetHotRecom(PageHotReq pageHotReq);
        public Task<DefaultMesRes> PageDisLike(long pageId, long userId);
        public Task<DefaultMesRes> PageLike(long pageId, long userId);
        public  Task<DefaultMesRes> DeletePage(long id, long userId);
        public Task<List<PageRes>> GetPageByIp(PageIpReq req);
        public Task<DefaultMesRes> UpdatePage(PageUpdateReq updateReq);
        public Task<List<string>> GetSerchKeyRank();
        public Task<List<PageRes>> GetPageByRealTime(PageSearchReq req);
        public Task<List<PageRes>> GetPageIntegrated(PageSearchReq req);
        public Task<List<PageRes>> GetPageByFollow(PageSearchReq req);
        public Task<string> UpLoadResource(IFormFile file);
        public Task<List<PageRes>> GetPageAll(PageSearchReq req);
        public Task<List<PageRes>> GetPageByFriends(PageSearchReq req);
        public Task AccessPage(long id, long userId);
        public Task<PageRes> GetPage(long id);
        public  Task<List<PageRes>> GetPageByUid(BasePagination pagination, long id, long uid);
        public  Task<List<PageRes>> GetPageByTag(PageTopicReq req);

    }
}
