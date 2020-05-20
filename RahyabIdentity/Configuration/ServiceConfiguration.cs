using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using RahyabIdentity.Models;
namespace RahyabIdentity.Configuration
{
    public class ServiceConfiguration
    {
       // private  IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();

        public static IEnumerable<IdentityResource> IdentityResources = new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
      
        };
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "Demo API")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                },

            
                // PolicyServer demo
                new ApiResource("policyserver.runtime"),
                new ApiResource("policyserver.management")
            };
        }
        public static IEnumerable<Client> GetClients(string url)
        {
            return new List<Client>
            {
                  //my test
                new Client
                {
                    ClientId = "implicit.test",
                    ClientName = "Implicit Client",
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RequireClientSecret = false,
                    RedirectUris = { "http://localhost:15536/IdentityServer4Authentication/LoginCallback?returnUrl=","http://localhost:15536/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:15536/" },
                   // FrontChannelLogoutUri = "http://localhost:15536/", // for testing identityserver on localhost
                   
                    AllowedGrantTypes = GrantTypes.Implicit,
                 //  AllowedScopes = { "openid", "profile", "email", "api" },
                   AllowedScopes = new List<string>
                   {
                       IdentityServerConstants.StandardScopes.OpenId,
                       IdentityServerConstants.StandardScopes.Profile,
                       IdentityServerConstants.StandardScopes.Email,

                   },
                },
                new Client
                {
                    ClientId = "implicit.test2",
                    ClientName = "Implicit Client",

                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RequireClientSecret = false,
                    RedirectUris = { "https://localhost:44389/", "https://localhost:44389/signin-oidc"},
                    PostLogoutRedirectUris = { "https://localhost:44389/","https://localhost:44389/signin-oidc" },
                    // FrontChannelLogoutUri = "https://localhost:44389/", // for testing identityserver on localhost
                   
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = { "openid", "profile", "email", "api" },
                    //AllowedScopes = new List<string>
                    //{
                    //    IdentityServerConstants.StandardScopes.OpenId,
                    //    IdentityServerConstants.StandardScopes.Profile
                    //},
                }
            };
        }
        public static List<ApplicationUser> GetUsers()
        {
            //return new List<TestUser>
            //{
            //    new TestUser
            //    {
            //        SubjectId = "818727",
            //        Username = "Amar",
            //        Password = "aA123456",
            //        Claims =
            //        {
            //            new Claim(JwtClaimTypes.Name, "Amar Potki"),
            //            new Claim(JwtClaimTypes.GivenName, "Amar"),
            //            new Claim(JwtClaimTypes.FamilyName, "Potki"),
            //            new Claim(JwtClaimTypes.Email, "Amar.potki@hotmail.com"),
            //            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
            //            new Claim(JwtClaimTypes.WebSite, "http://RahyabIdentity.com"),
            //            new Claim(JwtClaimTypes.Address,
            //                @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
            //                IdentityServerConstants.ClaimValueTypes.Json)
            //        }
            //    }
            //};
            var user =
                new ApplicationUser
                {
                    Id = "2483572a-d3ac-49d3-8a79-392313260be1",
                    PhoneNumber = "1234567890",
                    UserName = "Admin@BlueIrvine.com",
                    NormalizedEmail = "Admin@BlueIrvine.com",
                    NormalizedUserName = "Admin@BlueIrvine.com",
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    EmailConfirmed = true,
                    

                };
          
           // user.PasswordHash = _passwordHasher.HashPassword(user, "Blue!rvin3");

            return new List<ApplicationUser>()
            {
                user
            };
        }
    }
}
