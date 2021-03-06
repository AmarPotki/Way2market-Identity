// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RahyabIdentity.Models;
using RahyabIdentity.Services;
namespace RahyabIdentity.Controllers.Account
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly ISmsService _smsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events, ISmsService smsService, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _smsService = smsService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>Login.
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl){
            //todo: check it on production if performance is ok
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            var pageName = context.Parameters["pageName"];


            if (!string.IsNullOrEmpty(pageName)){
                if (pageName=="register"){ return RedirectToAction("Register", new {returnUrl}); }
            }
            //if user is logged in, system should redirect it
            var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync();
            if (authenticateResult.Succeeded)
                return Redirect(returnUrl);
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);
            
            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
         

            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
          
              //  _signInManager.CheckPasswordSignInAsync()
               var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
              // var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    if (!user.PhoneNumberConfirmed)
                    {
                        ModelState.AddModelError("", "کاربری شما هنوز تایید نشده، اگر اس ام اس دریافت نکرده اید دوباره تلاش کنید");

                        var vv = await BuildLoginViewModelAsync(model);
                        vv.UserId = user.Id;
                        vv.EnablerResend = true;
                        vv.DisplayConfirmAccount = true;
                        return View(vv);
                    }

                    await _signInManager.SignInWithClaimsAsync(user,model.RememberLogin, new List<Claim>
                    {
                        new Claim("FirstName", user.FirstName),
                        new Claim("LastName", user.LastName),
                        new Claim("UserName", user.UserName),

                    });
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.ClientId));

                    if (context != null)
                    {
                        if (await _clientStore.IsPkceClientAsync(context.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }


        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            // return View("LoggedOut", vm);
            return Redirect("http://way2market.ir");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();
            var registerVm = new RegisterViewModel();
            registerVm.ExternalProviders = providers;
            registerVm.ReturnUrl = returnUrl;
            ViewData["ReturnUrl"] = returnUrl;

            return View(registerVm);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.PhoneNumber,
                    //                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Errors.Any())
                {
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }

                    // If we got this far, something failed, redisplay form
                    return View(model);
                }
                else if (result.Succeeded)
                {
                    var smsCode = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
                    var r =  _smsService.SendVerifySms(model.PhoneNumber, smsCode);
                    if (string.IsNullOrEmpty(r))
                        return RedirectToAction("ConfirmPhoneNumber", "account", new { userId = user.Id,ReturnUrl=returnUrl, isDefault=false });

                    ModelState.AddModelError("", r);
                    return View(model);
                }
            }
            else { return View(model); }

            if (returnUrl != null)
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                    return Redirect(returnUrl);
                else
                    if (ModelState.IsValid)
                    return RedirectToAction("login", "account", new { returnUrl = returnUrl });
                else
                    return View(model);
            }

            return RedirectToAction("index", "home");
        }


        public async Task<IActionResult> ReSendConfirmSms(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var smsCode = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            var r =   _smsService.SendSms(user.PhoneNumber, smsCode);
            //if (r == "ok")
            return RedirectToAction("ConfirmPhoneNumber", "account", new { userId = user.Id });
            //return;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhoneNumber(string userId, string returnUrl,bool isDefault=true)
        {
            var model = new ConfirmViewModel
            {
                UserId = userId,
                ReturnUrl = returnUrl,
                EnableConfirmedAccount = true
            };
            //default means the normal way the user registers

                var user =await _userManager.FindByIdAsync(userId);
                if (user == null){
                    ModelState.AddModelError("0", "کار بر پیدا نشد");
                    model.EnableConfirmedAccount = false;
                    return View(model);
                }

                if (user.PhoneNumberConfirmed){
                    ModelState.AddModelError("1", "شماره تلفن شما قبل تایید شده");
                    model.EnableConfirmedAccount = false;

                return View(model);
                }

                if (isDefault){
                    var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                    _smsService.SendVerifySms(user.PhoneNumber, code);
                    return View(model);
                }



                return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPhoneNumber(ConfirmViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                var result = await _userManager.VerifyChangePhoneNumberTokenAsync(user,  model.Code, user.PhoneNumber);
                if (result){
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    await _signInManager.SignInWithClaimsAsync(user,true, new List<Claim>
                    {
                        new Claim("FirstName", user.FirstName),
                        new Claim("LastName", user.LastName),
                        new Claim("UserName", user.UserName),

                    });
                    if(string.IsNullOrEmpty(model.ReturnUrl))
                        return Redirect("~/");
                    return Redirect(model.ReturnUrl);
                }

                model.EnableConfirmedAccount = true;
                ModelState.AddModelError("", "کد اشتباه می باشد");
                return View(model);
            }
            return View(model);
        }

     

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePasswordRequest( string returnUrl)
        {
            var model = new ChangePasswordRequestViewModel
            {

                ReturnUrl = returnUrl

            };

            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordRequest(ChangePasswordRequestViewModel model,string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid){
                var user = await _userManager.FindByNameAsync(model.PhoneNumber);
                if (user == null)
                {
                    ModelState.AddModelError("", "کار بر پیدا نشد");
                    return View(model);
                }

                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                _smsService.SendVerifySms(user.PhoneNumber, code);
                return RedirectToAction("ChangePassword", new {ReturnUrl = returnUrl,UserId = user.Id});
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(string userId,string returnUrl)
        {
            var model = new ChangePasswordViewModel
            {
                UserId = userId,
                ReturnUrl = returnUrl

            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, string returnUrl)
        {
            if (ModelState.IsValid){
                var user = await _userManager.FindByIdAsync(model.UserId);
                var result = await _userManager.VerifyChangePhoneNumberTokenAsync(user,model.Code,user.PhoneNumber);
                if (result){
                    user.PhoneNumberConfirmed = true;

                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, model.NewPassword);
                    await _userManager.UpdateAsync(user);

                    await _signInManager.SignInWithClaimsAsync(user, true, new List<Claim>
                    {
                        new Claim("FirstName", user.FirstName),
                        new Claim("LastName", user.LastName),
                        new Claim("UserName", user.UserName),

                    });
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                        return Redirect("~/");
                    return Redirect(model.ReturnUrl);
                }

                ModelState.AddModelError("", "کد اشتباه می باشد");
                return View(model);
            }
            return View(model);
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
       
    }
}