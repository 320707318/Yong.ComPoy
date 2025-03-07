using Microsoft.AspNetCore.Authentication.JwtBearer;
using Nacos.AspNetCore.V2;
using Yong.Admin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNacosAspNet(builder.Configuration, "nacos");
//builder.Configuration.AddNacosV2Configuration(builder.Configuration.GetSection("nacos"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            //IdentityServer地址
            options.Authority = "http://127.0.0.1:5001/auth";
            //对应Idp中ApiResource的Name
            options.Audience = "AdminServices";
            //不使用https
            options.RequireHttpsMetadata = false;
        });
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddFreeRepository(null); //如果没有继承的仓储，第二个参数不用传
builder.Services.AddYongAdminModule();
var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
