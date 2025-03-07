using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Yitter.IdGenerator;

namespace Middleware.Snowflake
{
    public static class SnowUtils
    {
        public static IServiceCollection AddSnowflakeModule(this IServiceCollection services)
        {
            var idGeneratorOptions = new IdGeneratorOptions(1) { WorkerIdBitLength = 6,WorkerId= (ushort)GetWorkerId() };
            YitIdHelper.SetIdGenerator(idGeneratorOptions);
            return services;
        }
        private static int GetWorkerId()
        {
            var macAddr = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up)
                ?.GetPhysicalAddress();
            return macAddr != null ? macAddr.GetHashCode() % 32 : 1; // 假设 WorkerId 最大为 31  
        }
    }
}
