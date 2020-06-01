using Microsoft.AspNetCore.Mvc;

namespace RahyabIdentity.Controllers.Account{
    public class ConfirmViewModel{
        public string Code { get; set; }
        public string UserId { get; set; }
        public string ReturnUrl { get; set; }


        public bool EnableConfirmedAccount { get; set; }

    }
}