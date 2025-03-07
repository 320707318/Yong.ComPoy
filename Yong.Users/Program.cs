
using DotNetCore.CAP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Middleware.Snowflake;
using Nacos.AspNetCore.V2;
using Yong.Users;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//获取当前项目的端口号; 


builder.Services.AddNacosAspNet(builder.Configuration, "nacos");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            //IdentityServer地址
            options.Authority = "http://localhost:5001/auth";
            //对应Idp中ApiResource的Name
            options.Audience = "UserServices";
            //不使用https
            options.RequireHttpsMetadata = false;
        });
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
#region autofac
builder.Services.AddFreeRepository(null); //如果没有继承的仓储，第二个参数不用传
builder.Services.AddYongUsersModule();
builder.Services.AddSnowflakeModule();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});



builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue; //每个多部分主体的长度限制，默认值约为128MB 当前为2G
    options.ValueCountLimit = int.MaxValue; //要允许的表单条目数限制,默认为 1024个 当前为2147483647个
    options.ValueLengthLimit = int.MaxValue; //单个窗体值的长度限制 大约为 4MB 当前为2G
});

//为 ASP.NET Core Kestrel Web 服务器配置选项
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // 默认大约为 28.6MB 当前为2G
});

//为 IIS 进程内提供配置
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue; // 默认大约为 28.6MB 当前为2G
});
#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var redis = new FreeRedis.RedisClient("119.29.202.87:6379,password=123456,defaultDatabase=5");
ImHelper.Initialization(new ImClientOptions
{
    Redis = redis,
    Servers = new[] { "127.0.0.1:9002" }, //集群配置
});
var capBus = app.Services.GetRequiredService<ICapPublisher>();
ImHelper.EventBus(
    t => capBus.Publish("Message.SendToBe", t.clientId),
    t => redis.HDel("im_v2_wsOnline", t.clientId.ToString())) ;
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
