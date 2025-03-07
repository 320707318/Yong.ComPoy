using System.Net;
using System.Text;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Nacos;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var authenticationProviderKey = "auth1";
builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(authenticationProviderKey, options =>
                {
                    options.ApiSecret = "123456";
                    options.Authority = "http://localhost:5001/auth";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "UserServices";
                    options.SupportedTokens = SupportedTokens.Both;
                    
                });
authenticationProviderKey = "auth2";
builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(authenticationProviderKey, options =>
                {
                    options.ApiSecret = "123456";
                    options.Authority = "http://localhost:5001/auth";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "PageServices";
                    options.SupportedTokens = SupportedTokens.Both;

                });
authenticationProviderKey = "auth3";
builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(authenticationProviderKey, options =>
                {
                    options.ApiSecret = "123456";
                    options.Authority = "http://localhost:5001/auth";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "ImServices";
                    options.SupportedTokens = SupportedTokens.Both;

                });
authenticationProviderKey = "auth_admin";
builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(authenticationProviderKey, options =>
                {
                    options.ApiSecret = "123456";
                    options.Authority = "http://localhost:5001/auth";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "AdminServices";
                    options.SupportedTokens = SupportedTokens.Both;

                });
builder.Services.AddOcelot()
    .AddPolly()
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    })
    .AddNacosDiscovery("nacos");
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

app.UseCors("CorsPolicy");
app.MapControllers();

app.UseOcelot().Wait();
app.Run();
