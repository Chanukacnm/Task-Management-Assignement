using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagement.Application.Authentication.Common;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Validates the supplied Basic credentials. Reaching this action means the
    /// credentials are valid, so the current user is returned. The Angular client
    /// calls this to "log in" and then stores the credentials for later requests.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public ActionResult<UserDto> Login() => Ok(CurrentUser());

    /// <summary>Returns the currently authenticated user.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserDto> Me() => Ok(CurrentUser());

    private UserDto CurrentUser() => new()
    {
        Id = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0,
        Username = User.Identity?.Name ?? string.Empty,
        DisplayName = User.FindFirst("displayName")?.Value ?? User.Identity?.Name ?? string.Empty
    };
}
