using LifeHub.Api.Domain.Common;
using LifeHub.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Api.Data;

public sealed class LifeHubDbContext : DbContext
{
    public LifeHubDbContext(
        DbContextOptions<LifeHubDbContext> options
    )
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<Routine> Routines => Set<Routine>();

    public DbSet<RoutineCompletion> RoutineCompletions =>
        Set<RoutineCompletion>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder
    )
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LifeHubDbContext).Assembly
        );
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default
    )
    {
        UpdateAuditFields();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var utcNow = DateTime.UtcNow;

        foreach (
            var entry in ChangeTracker
                .Entries<IAuditableEntity>()
        )
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}