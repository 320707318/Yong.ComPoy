using FreeSql.DataAnnotations;
using static System.Net.WebRequestMethods;

namespace DbModels
{
    public class Users
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        public string UserName { get; set; }=string.Empty;
        public string Password { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public string Phone { get; set; }=string.Empty;
        public string NickName { get; set; }=string.Empty;
        public string Avatar { get; set; }=string.Empty;
        public int Gender { get; set; }=0;
        public int IsDeleted { get; set; }=0;
        public long CreateTime { get; set; }= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public int Role { get; set; }=0;
        public  long FansCount { get; set; }=0;
        public string Banner { get; set; } = "https://obs-bucked.obs.cn-south-1.myhuaweicloud.com/star.png";

        [Column(DbType = "text")]
       public string Profile { get; set; }=string.Empty;
    }
}
