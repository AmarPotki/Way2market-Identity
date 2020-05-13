using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RahyabIdentity.Controllers.Account
{
    public class RegisterViewModel
    {
        [Display(Name = "نام")]
        [StringLength(100, ErrorMessage = "نام حداقل باید 3 کارکتر باشد", MinimumLength = 3)]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [StringLength(100, ErrorMessage = "نام خانوادگی حداقل باید 3 کارکتر باشد", MinimumLength = 3)]
        public string LastName { get; set; }

        //[Required]
        //[EmailAddress(ErrorMessage = "پست الکترونیکی معتبر نمی باشد")]
        //[Display(Name = "پست الکترونیکی")]
        ////[RegularExpression(@"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$",
        ////    ErrorMessage = "پست الکترونیکی معتبر نمی باشد")]
        //public string Email { get; set; }

        [Required]
        [Display(Name = "شماره همراه")]
        [RegularExpression(@"(0|\+98)?([ ]|-|[()]){0,2}9[0|1|2|3|4|5|6|7|8|9]([ ]|-|[()]){0,2}(?:[0-9]([ ]|-|[()]){0,2}){8}", 
            ErrorMessage = "شماره تلفن همراه معتبر نمی باشد")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "رمز عبور حداقل باید 8 کارکتر باشد", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }


        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));
        public string ReturnUrl { get; set; }
    }
}
