using LifeHub.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeHub.Api.Data.Configurations;

public sealed class RoutineConfiguration :
    IEntityTypeConfiguration<Routine>
{
    public void Configure(
        EntityTypeBuilder<Routine> builder
    )
    {
        builder.ToTable("Routines");

        builder.HasKey(routine => routine.Id);

        builder.Property(routine => routine.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(routine => routine.Description)
            .HasMaxLength(1000);

        builder.Property(routine => routine.Category)
            .HasMaxLength(80);

        builder.Property(routine => routine.Frequency)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(routine => routine.DaysOfWeek)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(routine => routine.IsActive)
            .IsRequired();

        builder.Property(routine => routine.StartDate)
            .IsRequired();

        builder.Property(routine => routine.CreatedAt)
            .IsRequired();

        builder.Property(routine => routine.UpdatedAt)
            .IsRequired();

        builder.HasIndex(routine => routine.IsActive);

        builder.HasIndex(
            routine => new
            {
                routine.Frequency,
                routine.IsActive
            }
        );
    }
}