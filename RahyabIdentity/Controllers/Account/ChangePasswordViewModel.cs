namespace RahyabIdentity.Controllers.Account{
    public class ChangePasswordViewModel
    {
        public string UserId { get; set; }
        public string ReturnUrl { get; set; }
        public string NewPassword { get; set; }
        public string Code { get; set; }

    }
}