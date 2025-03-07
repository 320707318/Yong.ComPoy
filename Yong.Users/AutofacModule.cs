

using Cap.EventBus;
using FreeRedis;
using Hangfire;
using Hangfire.Common;
using Hangfire.MySql;
using HangFire.User.Job.Interface;
using HangFire.User.Job.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Middleware.ES;
using Middleware.Redis;
using Nest;
using SendProvide.QQEmali;
using UserUnitOfWork.Interface;
using UserUnitOfWork.UnitOfWokes;

namespace Yong.Users
{
    public static class AutofacModule
    {

        public static IServiceCollection AddYongUsersModule(this IServiceCollection services)
        {
            
            Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
            {
                IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                    .UseConnectionString(FreeSql.DataType.MySql, @"Data Source=119.29.202.87;Port=3306;User ID=yong;Password=123456; Initial Catalog=yong; SslMode=none;Min pool size=5")
                    .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
                    .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                    .Build();
                return fsql;
            };
            var RedisClient = RedisHelper.cli;
            services.AddSingleton(fsqlFactory);
            services.AddScoped<IOauthUOW, OauthUOW>();
            services.AddScoped<IUsersUOW, UsersUOW>();
            services.AddScoped<IMessageUOW, MessageUOW>();
            services.AddSingleton<IMailService, MailService>();
            services.AddCap(x =>
            {
                x.UseMySql(opt =>
                {
                    opt.ConnectionString = @"Data Source=119.29.202.87;Port=3306;User ID=cap;Password=123456; Initial Catalog=cap;Charset=utf8; SslMode=none;Min pool size=1";
                });
                x.UseRabbitMQ(opt =>
                {
                    opt.HostName = "119.29.202.87";
                    opt.Port = 5672;
                    opt.UserName = "320707";
                    opt.Password = "320707";
                });
            });
            services.AddTransient<IUserEventBus, UserEventBus>();
            services.AddTransient<IMessageEventBus, MessageEventBus>();
            services.AddSingleton(provider =>
            {
                var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200")); // 请根据你的 ES 地址进行修改  
                //.CertificateFingerprint("28592AC8FAEC42D5059AA8F3AEB8E2FD539FA558781ECCBF0A2DE6C57E684CD9")
                //.Authentication(new BasicAuthentication("elastic", "123456"))
                var client = new ElasticClient(settings);
            var createIndexResponse1 = client.Indices.CreateAsync("user", c => c
                .Map<UserEs>(m => m
                .Properties(ps => ps
                    .Text(t => t
                        .Name(p => p.NickName)
                    )
                    .Text(t => t
                        .Name(p => p.UserName)
                    )
                    .Text(t => t
                        .Name(p => p.Id)
                    )
                 )));
                return client;
            });
            #region hangfire
            services.AddHangfire(c =>
            {
                // 使用内存数据库演示，在实际使用中，会配置对应数据库连接，要保证该数据库要存在
                c.UseStorage(new MySqlStorage("Data Source=119.29.202.87;Port=3306;User ID=hangfire;Password=123456; Initial Catalog=hangfire2;Charset=utf8; SslMode=none;Min pool size=1", new MySqlStorageOptions()));
            });

            // Hangfire全局配置
            GlobalConfiguration.Configuration
            .UseColouredConsoleLogProvider()
                .UseStorage(new MySqlStorage("Data Source=119.29.202.87;Port=3306;User ID=hangfire;Password=123456; Initial Catalog=hangfire2;Charset=utf8; SslMode=none;Min pool size=1", new MySqlStorageOptions()))
                .WithJobExpirationTimeout(TimeSpan.FromDays(7));

            // Hangfire服务器配置
            services.AddHangfireServer(options =>
            {
                options.HeartbeatInterval = TimeSpan.FromSeconds(10);
            });
            services.AddTransient<IUsersJob, UsersJob>();
            var JobService = services.BuildServiceProvider().GetService<IUsersJob>();

            // Recurring job
            //RecurringJob.AddOrUpdate("UserDbToRedis", () => JobService.UserDbToRedis(), Cron.Daily(0, 21), TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("UserRedisToDb", () => JobService.UserRedisToDb(), Cron.Daily(15, 37), TimeZoneInfo.Local);
           // RecurringJob.AddOrUpdate("UserAiPush", () => JobService. UserAiPush(), Cron.Daily(0,0), TimeZoneInfo.Local);
            //BackgroundJob.Enqueue(() => JobService.UserDbToRedis());
            //BackgroundJob.Enqueue(() => JobService.UserToEs());
            //BackgroundJob.Enqueue(() => JobService.UserAiPush());
            #endregion

            return services;
        }
    }
}
