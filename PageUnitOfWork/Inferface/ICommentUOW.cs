using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;
using PageUnitOfWork.Request;
using PageUnitOfWork.Response;

namespace PageUnitOfWork.Inferface
{
    public interface ICommentUOW
    {
        public Task<DefaultMesRes> AddComment(CommonAddReq addReq);
        public Task<List<CommentRes>> GetCommentByArticleId(CommentReq req);
        public Task<DefaultMesRes> LikeComment(long userId, long commentId);
        public Task<DefaultMesRes> UnLikeComment(long userId, long commentId);
        public Task<DefaultMesRes> DeleteComment(long userId, long commentId);
        public Task<long> GetCommentCount(long commentId);
    }
}
