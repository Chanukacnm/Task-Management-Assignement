using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TaskManagement.Application.Authentication.Queries.ValidateCredentials;

namespace TaskManagement.Api.Authentication;

/// <summary>
/// Authenticates requests using the HTTP Basic scheme (RFC 7617). Credentials are
/// validated against the database via the <see cref="ValidateCredentialsQuery"/>.
/// No JWT or server-side session is used — every request carries its credentials.
/// </summary>
public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ISender _mediator;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISender mediator)
        : base(options, logger, encoder)
    {
        _mediator = mediator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            // No credentials supplied — let the pipeline decide (challenge => 401).
            return AuthenticateResult.NoResult();
        }

        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var headerValue)
            || !BasicAuthenticationDefaults.AuthenticationScheme.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return AuthenticateResult.Fail("Invalid Authorization header.");
        }

        string username;
        string password;
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue.Parameter));
            var separatorIndex = decoded.IndexOf(':');
            if (separatorIndex < 0)
            {
                return AuthenticateResult.Fail("Invalid Basic credentials format.");
            }

            username = decoded[..separatorIndex];
            password = decoded[(separatorIndex + 1)..];
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Authorization header is not valid Base64.");
        }

        var user = await _mediator.Send(new ValidateCredentialsQuery
        {
            Username = username,
            Password = password
        });

        if (user is null)
        {
            return AuthenticateResult.Fail("Invalid username or password.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("displayName", user.DisplayName)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // Return a plain 401 without the standard "WWW-Authenticate: Basic" header so
        // the browser does not show its native credential dialog for our Angular SPA.
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}
