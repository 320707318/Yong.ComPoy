using IdentityModel;
using IdentityServer4.Models;
using System.Security.Claims;
using IdentityServer4.Validation;

namespace IdentityServer
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // 根据用户名和密码查询用户  
            

            if( string.IsNullOrEmpty(context.UserName ) || string.IsNullOrEmpty(context.Password) )
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid credential");
                return ;
            }

            // 用户验证成功，将用户ID添加到Claims中  
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Role, context.Password), // 用户ID作为Subject Claim
                new Claim(JwtClaimTypes.Id, context.UserName), // 用户ID作为Subject Claim  
                // 可以添加其他用户信息作为Claims  
               
                // ...  
            };

            context.Result = new GrantValidationResult(
                subject: context.UserName,
                authenticationMethod: "custom",
                claims: claims);
        }
    }

}
