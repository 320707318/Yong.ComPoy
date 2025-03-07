using DbModels;

using Hangfire;
using Hangfire.MySql;
using Job.Page.Interface;
using Job.Page.Jobs;
using Nest;
using Page.EventBus;
using PageUnitOfWork.Inferface;
using PageUnitOfWork.UnitOfWokes;

namespace Yong.Page
{
    public static class AutofacModule
    {

        public static IServiceCollection AddYongPageModule(this IServiceCollection services)
        {
            Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
            {
                IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                    .UseConnectionString(FreeSql.DataType.MySql, @"Data Source=119.29.202.87;Port=3306;User ID=yong;Password=123456; Initial Catalog=yong;SslMode=none;Min pool size=1")
                    .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
                    .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                    .Build();
                return fsql;
            };
            services.AddSingleton(fsqlFactory);
            services.AddScoped<IPageUOW, PageUOW>();
            services.AddScoped<ISuperTopicUOW, SuperTopicUOW>();
            services.AddScoped<ICommentUOW, CommentUOW>();
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
            services.AddTransient<IPageEventBus, PageEventBus>();

            services.AddSingleton(provider =>
            {
                var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200")) // 请根据你的 ES 地址进行修改  
                //.CertificateFingerprint("28592AC8FAEC42D5059AA8F3AEB8E2FD539FA558781ECCBF0A2DE6C57E684CD9")
                //.Authentication(new BasicAuthentication("elastic", "123456"))
                .DefaultIndex("page"); // 设置默认索引  
                var client = new ElasticClient(settings);
                var createIndexResponse = client.Indices.CreateAsync("page", c => c
                        .Map<DbModels.Page>(m => m
                        .Properties(ps => ps
                        .Keyword(t => t
                            .Name(p => p.id)
                        )
                        .Nested<Resource>(n => n  // 使用泛型 Nested 方法  
                            .Name(p => p.Resources)
                            .Properties(nps => nps
                                .Text(t => t
                                    .Name(c => c.Url)
                                )
                                .Text(t => t
                                    .Name(c => c.Name)
                                )
                                .Text(t => t
                                    .Name(c => c.Sort))
                                .Keyword(k => k
                                    .Name(c => c.Id))
                                .Keyword(k => k
                                    .Name(c => c.PageId)))
                        )
                        .Nested<Supers>(n=>n
                            .Name(p=>p.SuperIds)
                            .Properties(nps=>nps
                                .Keyword(k=>k
                                    .Name(c=>c.Id)))
                        )
                        .Text(t=>t.Name(p=>p.SuperTopics))
                        .Keyword(t => t
                            .Name(p => p.Ip)
                        )
                        .Text(t => t
                            .Name(p => p.Content)
                        )
                      
                        .Text(t => t
                            .Name(p => p.Topics)
                        )
                        .Text(t => t
                            .Name(p => p.Title)
                        )
                        .Text(t => t
                            .Name(p => p.CollectCount))
                        .Text(t => t
                            .Name(p => p.CommentCount))
                        .Number(t => t
                            .Name(p => p.CreateTime)
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
            .UseStorage(new MySqlStorage("Data Source = 119.29.202.87; Port = 3306; User ID = hangfire; Password = 123456; Initial Catalog = hangfire2;  SslMode = none; Min pool size = 1", new MySqlStorageOptions()))
                .WithJobExpirationTimeout(TimeSpan.FromDays(7));

            // Hangfire服务器配置
            services.AddHangfireServer(options =>
            {
                options.HeartbeatInterval = TimeSpan.FromSeconds(10);
            });
            services.AddTransient<IPageJob, PageJob>();
            var JobService = services.BuildServiceProvider().GetService<IPageJob>();

            // Recurring job
            //RecurringJob.AddOrUpdate("PageDbToEs", () => JobService.PageDbToEs(), Cron.Daily(20,49), TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("ClearSearchKey", () => JobService.ClearSearchKey(), Cron.Hourly(), TimeZoneInfo.Local);
           //BackgroundJob.Enqueue(()=>JobService.PageDbToEs());
            //BackgroundJob.Enqueue(()=>JobService.CommentDbToRedis());
            //BackgroundJob.Enqueue(() => JobService.InitPageType());

            #endregion

            return services;
        }
    }
}
