using Microsoft.AspNetCore.Identity;

namespace CleanStart.Infrastructure.Identity;

/// <summary>ASP.NET Identity user, extended with the app-specific fields you need.
/// Add more columns here (not in a separate Profile table) unless they're genuinely
/// optional/1-to-many.</summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;
}
