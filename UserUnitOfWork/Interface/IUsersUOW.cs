using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModels.BaseModel;
using Microsoft.AspNetCore.Http;
using Middleware.Redis;
using SendProvide.QQEmali.Models;
using UserUnitOfWork.Request;
using UserUnitOfWork.Response;

namespace UserUnitOfWork.Interface
{
    public interface IUsersUOW
    {
        public Task<UserRes> GetUserById(int id,long uid);
        public Task<List<UserRes>> GetSuperTopicMembers(long id);
        public Task<DefaultMesRes> EditUser(UserEditReq editRes);
        public Task SendEmailCode(MailRequest mail);
        public Task<EmailBindRes> BindEmail(EmaliBindReq emaliBind);
        public Task SendForgetPasswordCode(MailRequest mail);
        public Task<DefaultMesRes> EditPassWord(UserChangePasswordReq req);
        public  Task<List<UserRes>> GetFans(long uid);
        public  Task<List<UserRes>> GetFollows(long uid);
        public  Task<RedisUserDataRes> GetUserData(long uid);
        public  Task<List<UserRes>> GetFriends(long uid);
        public  Task<List<UserSimpleRes>> UserSimples(UserSearchReq req);
        public Task<string> UpLoadAvatar(IFormFile file);
        public Task<List<UserRes>> SearchUser(BasePagination page, string keyWord, long uid);
        public  Task<List<UserSimpleRes>> SearchSimplesUser(long uid);
        public  Task<string> UplodSound(IFormFile file);
        public Task<string> UpLoadImage(IFormFile file);
        public  Task<string> UplodBg(IFormFile file);
        public  Task<string> UploadResource(IFormFile file);

        public Task ScanLogin(long uid, string qrcode);
        public  Task ScanQrCode(string qrcode);

    }
}
