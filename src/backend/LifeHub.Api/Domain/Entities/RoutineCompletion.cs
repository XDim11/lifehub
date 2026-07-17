namespace LifeHub.Api.Domain.Entities;

public sealed class RoutineCompletion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RoutineId { get; set; }

    public Routine Routine { get; set; } = null!;

    public DateOnly CompletionDate { get; set; }

    public DateTime CompletedAt { get; set; } =
        DateTime.UtcNow;
}