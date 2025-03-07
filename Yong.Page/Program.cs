
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
            //IdentityServer��ַ
            options.Authority = "http://localhost:5001/auth";
            //��ӦIdp��ApiResource��Name
            options.Audience = "PageServices";
            //��ʹ��https
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
//�ѷ����ע�����д������
builder.Services.AddFreeRepository(null); //���û�м̳еĲִ����ڶ����������ô�
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
