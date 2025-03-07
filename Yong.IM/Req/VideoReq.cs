namespace Yong.IM.Req
{
    public class VideoReq
    {
        public long senderId { get; set; }
        public long receiverId { get; set; }

        /// <summary>
        /// 100发起通话，200同意通话，300拒绝通话，400结束通话
        /// </summary>
        public int code { get; set; }

        public string message { get; set; }
    }
}
