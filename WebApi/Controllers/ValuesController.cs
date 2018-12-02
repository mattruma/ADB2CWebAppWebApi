using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [Authorize]
        [HttpGet("secure")]
        public IActionResult GetSecured()
        {
            return Ok("You called a SECURE action.");
        }

        [HttpGet("unsecure")]
        public ActionResult<IDictionary<string, string>> GetUnsecured()
        {
            return Ok("You called an UNSECURE action.");
        }
    }
}