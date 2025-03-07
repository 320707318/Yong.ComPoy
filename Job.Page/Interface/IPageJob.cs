using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job.Page.Interface
{
    public interface IPageJob
    {
        public  Task PageDbToEs();
        public Task ClearSearchKey();
        public Task CommentDbToRedis();
        public Task InitPageType();


    }
}
