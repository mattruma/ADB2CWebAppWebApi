using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        private AzureAdB2COptions _azureAdB2COptions;

        public AccountController(
            IOptions<AzureAdB2COptions> azureAdB2COptions)
        {
            _azureAdB2COptions = azureAdB2COptions.Value;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            var redirectUrl = Url.Action("Index", "Home");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var redirectUrl = Url.Action("Index", "Home");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = _azureAdB2COptions.ResetPasswordPolicyId;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}