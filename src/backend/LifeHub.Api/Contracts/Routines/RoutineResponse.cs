using LifeHub.Api.Domain.Enums;

namespace LifeHub.Api.Contracts.Routines;

public sealed record RoutineResponse(
    Guid Id,
    string Title,
    string? Description,
    RoutineFrequency Frequency,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    string? Category,
    bool IsActive,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateOnly? EvaluationDate,
    bool? IsScheduledForDate,
    bool? IsCompletedForDate
);