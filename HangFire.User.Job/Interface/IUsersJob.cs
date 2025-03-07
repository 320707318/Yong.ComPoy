using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFire.User.Job.Interface
{
    public interface IUsersJob
    {
        public  Task UserDbToRedis();
        public Task UserRedisToDb();
        public Task UserToEs();
        public Task UserAiPush();
    }
}
