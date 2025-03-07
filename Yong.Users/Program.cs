
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

//��ȡ��ǰ��Ŀ�Ķ˿ں�; 


builder.Services.AddNacosAspNet(builder.Configuration, "nacos");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            //IdentityServer��ַ
            options.Authority = "http://localhost:5001/auth";
            //��ӦIdp��ApiResource��Name
            options.Audience = "UserServices";
            //��ʹ��https
            options.RequireHttpsMetadata = false;
        });
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
#region autofac
builder.Services.AddFreeRepository(null); //���û�м̳еĲִ����ڶ����������ô�
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
    options.MultipartBodyLengthLimit = int.MaxValue; //ÿ���ಿ������ĳ������ƣ�Ĭ��ֵԼΪ128MB ��ǰΪ2G
    options.ValueCountLimit = int.MaxValue; //Ҫ����ı���Ŀ������,Ĭ��Ϊ 1024�� ��ǰΪ2147483647��
    options.ValueLengthLimit = int.MaxValue; //��������ֵ�ĳ������� ��ԼΪ 4MB ��ǰΪ2G
});

//Ϊ ASP.NET Core Kestrel Web ����������ѡ��
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // Ĭ�ϴ�ԼΪ 28.6MB ��ǰΪ2G
});

//Ϊ IIS �������ṩ����
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue; // Ĭ�ϴ�ԼΪ 28.6MB ��ǰΪ2G
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
    Servers = new[] { "127.0.0.1:9002" }, //��Ⱥ����
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
