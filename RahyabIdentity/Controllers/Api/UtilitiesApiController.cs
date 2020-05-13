using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RahyabIdentity.Models;
using RahyabIdentity.Services;
namespace RahyabIdentity.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilitiesController : ControllerBase{
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISmsService _smsService;
        public UtilitiesController(UserManager<ApplicationUser> userManager, ISmsService smsService){
            _userManager = userManager;
            _smsService = smsService;
        }
        [Route("Resend")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Resend(string userId){
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return  BadRequest("کاربر یافت نشد");
            var code =await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            var r= _smsService.SendVerifySms(user.PhoneNumber, code);
            if (!string.IsNullOrEmpty(r))
                BadRequest(r);
            return Ok("کد برای شما ارسال شد");
        }
    }
}