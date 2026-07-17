namespace LifeHub.Api.Contracts.Routines;

public sealed record RoutineCompletionResponse(
    Guid Id,
    Guid RoutineId,
    DateOnly CompletionDate,
    DateTime CompletedAt
);