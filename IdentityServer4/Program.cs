using System.Net;
using IdentityServer;
using IdentityServer4.Services;
using Nacos.AspNetCore.V2;
ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddNacosAspNet(builder.Configuration, "nacos");
builder.Services.AddIdentityServer()
    .AddDeveloperSigningCredential()
    .AddInMemoryClients(Config.GetClients())//Client模式
    .AddInMemoryApiScopes(Config.ApiScopes)//作用域
    .AddInMemoryIdentityResources(Config.GetIdentityResources())//身份资源
    .AddInMemoryApiResources(Config.GetApiResources())//资源
    .AddProfileService<ProfileService>()
    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder => policyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}); ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseIdentityServer();//使用Id4
app.UseCors("CorsPolicy");
app.MapControllers();

app.Run();
