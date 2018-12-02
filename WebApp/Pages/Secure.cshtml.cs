using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using WebApp.Helpers;

namespace WebApp.Pages
{
    [Authorize]
    public class SecureModel : PageModel
    {
        private AzureAdB2COptions _azureAdB2COptions;

        public SecureModel(
            IOptions<AzureAdB2COptions> azureAdB2COptions)
        {
            _azureAdB2COptions = azureAdB2COptions.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string responseString = "";

            try
            {
                // Retrieve the token with the specified scopes
                var scope = _azureAdB2COptions.ApiScopes.Split(' ');

                string signedInUserID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                var userTokenCache = new MSALSessionCache(
                    signedInUserID,
                    this.HttpContext).GetMsalCacheInstance();

                var cca =
                    new ConfidentialClientApplication(
                        _azureAdB2COptions.ClientId,
                        _azureAdB2COptions.Authority,
                        _azureAdB2COptions.RedirectUri,
                        new ClientCredential(_azureAdB2COptions.ClientSecret),
                        userTokenCache,
                        null);

                var accounts =
                    await cca.GetAccountsAsync();

                var result =
                    await cca.AcquireTokenSilentAsync(scope, accounts.FirstOrDefault(), _azureAdB2COptions.Authority, false);

                var client = new HttpClient();
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _azureAdB2COptions.ApiUrl + "/secure");

                // Add token to the Authorization header and make the request
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.AccessToken);
                var response =
                    await client.SendAsync(request);

                ViewData["AuthenticationResultAccessToken"] = result.AccessToken;
                ViewData["AuthenticationResultIdToken"] = result.IdToken;
                ViewData["AuthenticationResultTenantId"] = result.TenantId;
                ViewData["AuthenticationResultScopes"] = JsonConvert.SerializeObject(result.Scopes, Formatting.Indented);
                ViewData["AuthenticationResultUser"] = JsonConvert.SerializeObject(result.Account, Formatting.Indented);
                ViewData["AzureAdB2COptionsClientId"] = _azureAdB2COptions.ClientId;
                ViewData["AzureAdB2COptionsAuthority"] = _azureAdB2COptions.Authority;
                ViewData["AzureAdB2COptionsRedirectUri"] = _azureAdB2COptions.RedirectUri;
                ViewData["AzureAdB2COptionsClientSecret"] = _azureAdB2COptions.ClientSecret;
                ViewData["AzureAdB2COptionsApiUrl"] = _azureAdB2COptions.ApiUrl;
                ViewData["AzureAdB2COptionsApiScopes"] = _azureAdB2COptions.ApiScopes;

                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responseString = await response.Content.ReadAsStringAsync();
                        break;

                    case HttpStatusCode.Unauthorized:
                        responseString = $"Please sign in again. {response.ReasonPhrase}";
                        break;

                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            ViewData["ResponsePayload"] = $"{responseString}";

            return Page();
        }
    }
}