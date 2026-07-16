using LifeHub.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeHub.Api.Data.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(task => task.Description)
            .HasMaxLength(1000);

        builder.Property(task => task.Category)
            .HasMaxLength(80);

        builder.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(task => task.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(task => task.CreatedAt)
            .IsRequired();

        builder.Property(task => task.UpdatedAt)
            .IsRequired();

        builder.HasIndex(task => task.Status);

        builder.HasIndex(task => task.Priority);

        builder.HasIndex(task => task.DueDate);
    }
}