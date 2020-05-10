using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
namespace RahyabIdentity.Configuration
{
    public class Way2MarketRedirectValidator : IRedirectUriValidator
    {
        private static readonly Regex Way2Market = new Regex(@"https?://([a-z0-9]+[.])*way2market.ir['/'].*",
            RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (client.ClientId == "RahyabStore"){
                if (!Way2Market.IsMatch(requestedUri)) return Task.FromResult(false);
                return Task.FromResult(true);
            }
            var flag=client.RedirectUris.Any(x => x == requestedUri);
                return Task.FromResult(flag);

            

        }
        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (client.ClientId == "RahyabStore")
            {
                if (!Way2Market.IsMatch(requestedUri)) return Task.FromResult(false);
                return Task.FromResult(true);
            }

            var flag = client.PostLogoutRedirectUris.Any(x => x == requestedUri);
            return Task.FromResult(flag);
        }
    }
}
