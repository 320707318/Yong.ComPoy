namespace Middleware.FreeIm
{
    public class ImRes
    {
        public ImResType type { get; set; }
        public ImReq content { get; set; }
        public UserRes from { get; set; }
        public long to { get; set; }
        public long CreateTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    }
    public class MetionRes
    {
        public long pageid { get; set; }
        public string pagedesc { get; set; }
        public ImResType type { get; set; }
        public ImReq content { get; set; }
        public UserRes from { get; set; }
        public long to { get; set; }
        public long CreateTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    }
    public class AiRes
    {
        public string text { get; set; }

        public ImResType type { get; set; }
    }
    public class UserRes
    {
        public long Id { get; set; }
        public string NickName { get; set; }
        public string Avatar { get; set; }
    }
    public enum ImResType
    {
        Boardcast,
        Group,
        Private,
        System,
        User,
        Other
    }

}
