using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.IdentityModel.Logging;
using Nacos.AspNetCore.V2;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddNacosAspNet(builder.Configuration, "nacos");
//builder.Configuration.AddNacosV2Configuration(builder.Configuration.GetSection("nacos"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            //IdentityServer地址
            options.Authority = "http://localhost:5001/auth";
            //对应Idp中ApiResource的Name
            options.Audience = "ImServices";
            //不使用https
            options.RequireHttpsMetadata = false;
        });
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(120);
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseFreeImServer(new ImServerOptions
{
    Redis = new FreeRedis.RedisClient("119.29.202.87:6379,password=123456,defaultDatabase=5"),
    Servers = new[] { "127.0.0.1:9002" }, //集群配置
    Server = "127.0.0.1:9002"
});
ImHelper.Initialization(new ImClientOptions
{
    Redis = new FreeRedis.RedisClient("119.29.202.87:6379,password=123456,defaultDatabase=5"),
    Servers = new[] { "127.0.0.1:9002" }, //集群配置
});


app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.Run();

