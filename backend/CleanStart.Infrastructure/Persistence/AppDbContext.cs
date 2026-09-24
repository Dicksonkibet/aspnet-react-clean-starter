using CleanStart.Domain.Entities;
using CleanStart.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanStart.Infrastructure.Persistence;

/// <summary>
/// IdentityDbContext gives you Users/Roles/Claims tables for free alongside your own
/// entities. Add a DbSet&lt;T&gt; per entity as you create them — the generic
/// repository (EfRepository&lt;T&gt;) finds it via context.Set&lt;T&gt;(), no extra
/// wiring needed.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Soft-deleted rows never show up in a normal query. Add the same filter for
        // every new entity you create (EF Core doesn't inherit query filters, each
        // entity needs its own line here).
        builder.Entity<TodoItem>().HasQueryFilter(t => !t.IsDeleted);
    }
}
