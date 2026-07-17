using LifeHub.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeHub.Api.Data.Configurations;

public sealed class RoutineCompletionConfiguration :
    IEntityTypeConfiguration<RoutineCompletion>
{
    public void Configure(
        EntityTypeBuilder<RoutineCompletion> builder
    )
    {
        builder.ToTable("RoutineCompletions");

        builder.HasKey(completion => completion.Id);

        builder.Property(completion => completion.CompletionDate)
            .IsRequired();

        builder.Property(completion => completion.CompletedAt)
            .IsRequired();

        builder
            .HasOne(completion => completion.Routine)
            .WithMany(routine => routine.Completions)
            .HasForeignKey(completion => completion.RoutineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(completion => new
            {
                completion.RoutineId,
                completion.CompletionDate
            })
            .IsUnique();
    }
}