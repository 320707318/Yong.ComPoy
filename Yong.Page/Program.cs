
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Nacos.AspNetCore.V2;
using PageUnitOfWork.gRpcServices;
using Yong.Page;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddGrpc();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddNacosAspNet(builder.Configuration, "nacos");
var authenticationProviderKey = "auth2";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            //IdentityServer地址
            options.Authority = "http://localhost:5001/auth";
            //对应Idp中ApiResource的Name
            options.Audience = "PageServices";
            //不使用https
            options.RequireHttpsMetadata = false;
        });
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(6002, o => o.Protocols = HttpProtocols.Http2);
    options.Listen(IPAddress.Any,5002, o => o.Protocols = HttpProtocols.Http1);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder => policyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}); ;
#region autofac
//把服务的注入规则写在这里
builder.Services.AddFreeRepository(null); //如果没有继承的仓储，第二个参数不用传
builder.Services.AddYongPageModule();

#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CorsPolicy");
app.MapControllers();
app.MapGrpcService<PageService>();

app.Run();
