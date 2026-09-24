using FluentValidation;
using MediatR;

namespace CleanStart.Application.Auth;

// Auth is handled mostly in the API layer (it owns ASP.NET Identity + JWT signing —
// see CleanStart.API/Controllers/AuthController.cs and Auth/JwtTokenGenerator.cs).
// These records are the shared request/response contracts so both layers agree on shape.

public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email, string FullName);

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
