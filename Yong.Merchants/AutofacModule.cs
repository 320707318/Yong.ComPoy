namespace Yong.Admin
{
    public static class AutofacModule
    {

        public static IServiceCollection AddYongMerchantsModule(this IServiceCollection services)
        {

            Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
            {
                IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                    .UseConnectionString(FreeSql.DataType.MySql, @"Data Source=119.29.202.87;Port=3306;User ID=Merchants;Password=123456; Initial Catalog=Merchants;Charset=utf8; SslMode=none;Min pool size=5")
                    .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
                    .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                    .Build();
                return fsql;
            };
            services.AddSingleton(fsqlFactory);
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
            #region gRPC Client注册
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //services.AddGrpcClient<PageApiClient>(options =>
            //{
            //    options.Address = new Uri("http://localhost:5012");
            //}).ConfigureChannel(grpcOptions =>
            //{
            //    //可以完成各种配置，比如token
            //});
            #endregion

            return services;
        }
    }
}
