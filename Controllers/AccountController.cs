using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/account/login")]
    [AllowAnonymous]
    public IActionResult Login([FromQuery] string returnUrl = "/")
    {
        var payload = new
        {
            path = HttpContext.Request.Path.ToString(),
            query = HttpContext.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()),
            headers = HttpContext.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString()),
            returnUrl
        };
        _logger.LogInformation("[AUTH STUB] Login called: {@payload}", payload);
        return Ok(new { message = "Auth stub: login", payload });
    }

    [HttpPost("/account/logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        var payload = new
        {
            path = HttpContext.Request.Path.ToString(),
            query = HttpContext.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()),
            headers = HttpContext.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString())
        };
        _logger.LogInformation("[AUTH STUB] Logout called: {@payload}", payload);
        return Ok(new { message = "Auth stub: logout", payload });
    }
}