using LifeHub.Api.Domain.Enums;

namespace LifeHub.Api.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;

    public string? Category { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}