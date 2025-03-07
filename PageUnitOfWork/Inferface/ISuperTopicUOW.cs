using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;
using Middleware.Cap.Mq;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace PageUnitOfWork.Inferface
{
    public interface ISuperTopicUOW
    {
        public  Task<SuperTopicRes> GetSuperTopic(long id, long uid);
        public Task<DefaultMesRes> AddSuperTopic(SuperTopicAddMqDto superTopic);
        public Task<List<TypeRes>> GetSuperTopicType();
        public Task<List<SuperTopicRes>> GetSuperTopicSearch(SuperTopicSearchReq super, long uid);
        public Task<DefaultMesRes> AddSuperTopicType(SuperTopicTypeAddMqDto superTopicType);
        public  Task<DefaultMesRes> JoinSuperTopic(long pageId, long userId);
        public Task<DefaultMesRes> QuitSuperTopic(long pageId, long userId);
        public Task<DefaultMesRes> DeleteSuperTopic(long pageId, long userId);
        public  Task<List<SuperTopicRes>> GetSuperTopByCreater(long uid);
        public  Task<List<SuperTopicRes>> GetSuperTopByJoin(long uid);
        public Task<List<SuperTopicSimpleRes>> GetSuperTopicByIds(SuperTopicIdsReq req);
    }
}
