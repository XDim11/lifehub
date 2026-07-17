using LifeHub.Api.Domain.Common;
using LifeHub.Api.Domain.Enums;

namespace LifeHub.Api.Domain.Entities;

public sealed class Routine : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public RoutineFrequency Frequency { get; set; } =
        RoutineFrequency.Daily;

    public RoutineDays DaysOfWeek { get; set; } =
        RoutineDays.None;

    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;

    public DateOnly StartDate { get; set; } =
        DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } =
        DateTime.UtcNow;

    public ICollection<RoutineCompletion> Completions
    {
        get;
        set;
    } = new List<RoutineCompletion>();
}