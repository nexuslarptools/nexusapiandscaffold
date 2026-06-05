using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers
{
    // Exclude this class from MVC controller discovery to avoid exposing OIDC flows.
    // External middleware (e.g., ForwardAuth/OIDC proxy) is responsible for authentication.
    // Using [NonController] prevents ASP.NET Core from treating public methods as actions,
    // which avoids the runtime error about attribute routing on [ApiController] controllers.
    [NonController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        // Removed OIDC endpoints (no longer exposed). Authentication is handled by ForwardAuth.
    }
}
