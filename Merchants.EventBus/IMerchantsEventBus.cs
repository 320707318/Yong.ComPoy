
using Middleware.Cap.Mq;

namespace Merchants.EventBus
{
    public interface IMerchantsEventBus
    {
        public  Task SendLoginCode(MailLoginCode mbc);
        public  Task AddShop(MerchantMqDto mqDto);
    }
}
