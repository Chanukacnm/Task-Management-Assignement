using MediatR;
using TaskManagement.Application.Authentication.Common;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Authentication.Queries.ValidateCredentials;

/// <summary>
/// Validates a username/password pair against the stored, hashed credentials.
/// Returns the matching <see cref="UserDto"/> or <c>null</c> when authentication fails.
/// Used by the HTTP Basic authentication handler in the API layer.
/// </summary>
public record ValidateCredentialsQuery : IRequest<UserDto?>
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public class ValidateCredentialsQueryHandler : IRequestHandler<ValidateCredentialsQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ValidateCredentialsQueryHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto?> Handle(ValidateCredentialsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return null;
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null)
        {
            // Perform a comparable PBKDF2 derivation even when the user does not exist
            // so that response time does not reveal whether a username is valid
            // (mitigates username enumeration via timing).
            _passwordHasher.Hash(request.Password);
            return null;
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName
        };
    }
}
