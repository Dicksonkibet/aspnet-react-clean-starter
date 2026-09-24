namespace CleanStart.Application.Common.Interfaces;

/// <summary>Reads the authenticated user's id/roles out of the current HTTP request's
/// claims. Implemented in the API layer (it's the only layer that knows about
/// HttpContext) and injected into Application handlers that need it.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
