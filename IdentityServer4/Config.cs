using IdentityServer4;
using IdentityServer4.Models;

namespace IdentityServer
{
    public class Config
    {
        /// <summary>
        /// 添加对OpenID Connect的支持
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(), //必须要添加，否则报无效的scope错误
                new IdentityResources.Profile()
            };
        }
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("UserServices_scope"),
                new ApiScope("PageServices_scope"),
                new ApiScope("ImServices_scope"),
                new ApiScope("AdminServices_scope"),
                 new ApiScope("MerchantsServices_scope")

            };
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>()
            {
                new ApiResource("UserServices","UserServices"){ Scopes={ "UserServices_scope" } ,ApiSecrets=new List<Secret>{new Secret("123456".Sha256()) }},
                new ApiResource("PageServices","PageServices"){ Scopes={ "PageServices_scope" } ,ApiSecrets=new List<Secret>{new Secret("123456".Sha256()) }},
                new ApiResource("ImServices","ImServices"){ Scopes={ "ImServices_scope" } ,ApiSecrets=new List<Secret>{new Secret("123456".Sha256()) }},
                new ApiResource("AdminServices","AdminServices"){ Scopes={ "AdminServices_scope" } ,ApiSecrets=new List<Secret>{new Secret("123456".Sha256()) }},
                new ApiResource("MerchantsServices","MerchantsServices"){ Scopes={ "MerchantsServices_scope" } ,ApiSecrets=new List<Secret>{new Secret("123456".Sha256()) }}
            };
        }
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
               new Client{
                   ClientId="web_client",//客户端唯一标识
                   //AllowedGrantTypes=new List<string>{ "password", "username"},
                   AllowedGrantTypes=GrantTypes.ResourceOwnerPassword,
                   ClientSecrets=new[]{new Secret("123456".Sha256()) },//客户端密码，进行了加密
                   AccessTokenLifetime=3600,
                   AllowedScopes=new List<string>//允许访问的资源
                   {
                        "PageServices_scope",
                        "UserServices_scope",
                        "ImServices_scope",
                        "AdminServices_scope",
                        "MerchantsServices_scope",
                        IdentityServerConstants.StandardScopes.OpenId, //必须要添加，否则报403 forbidden错误
                        IdentityServerConstants.StandardScopes.Profile
                   }
                  
               }
            };
        }

    }
}
