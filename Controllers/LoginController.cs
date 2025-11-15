using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NEXUSDataLayerScaffold.Controllers
{
    [ApiController]
    [Route("api/v1/login")]
    public class LoginController : ControllerBase
    {
        // This endpoint is intentionally anonymous and simply redirects to '/'
        // so external middleware can handle auth and bring the user back to the root.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // Use LocalRedirect to avoid open redirect vulnerabilities.
            return LocalRedirect("/");
        }
    }
}
