using System.Security.Claims;
using CleanStart.Application.Common.Interfaces;

namespace CleanStart.API.Auth;

/// <summary>API-layer implementation of ICurrentUserService — the only place that's
/// allowed to know about HttpContext. Application handlers depend on the interface,
/// never on this class.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User?.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
