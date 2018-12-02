using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using WebApp.Helpers;

namespace WebApp.Pages
{
    public class UnsecureModel : PageModel
    {
        private AzureAdB2COptions _azureAdB2COptions;

        public UnsecureModel(
            IOptions<AzureAdB2COptions> azureAdB2COptions)
        {
            _azureAdB2COptions = azureAdB2COptions.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string responseString = "";

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _azureAdB2COptions.ApiUrl + "/unsecure");

                var response =
                    await client.SendAsync(request);

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
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            ViewData["ResponsePayload"] = $"{responseString}";

            return Page();
        }
    }
}