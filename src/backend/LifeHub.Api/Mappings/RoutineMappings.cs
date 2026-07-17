using LifeHub.Api.Contracts.Routines;
using LifeHub.Api.Domain.Entities;
using LifeHub.Api.Domain.Enums;

namespace LifeHub.Api.Mappings;

public static class RoutineMappings
{
    private static readonly DayOfWeek[] OrderedDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];

    public static RoutineResponse ToResponse(
        this Routine routine,
        DateOnly? evaluationDate = null
    )
    {
        bool? isScheduled = null;
        bool? isCompleted = null;

        if (evaluationDate.HasValue)
        {
            var date = evaluationDate.Value;

            isScheduled =
                routine.IsActive
                && routine.IsScheduledOn(date);

            isCompleted = routine.Completions.Any(
                completion =>
                    completion.CompletionDate == date
            );
        }

        return new RoutineResponse(
            routine.Id,
            routine.Title,
            routine.Description,
            routine.Frequency,
            routine.DaysOfWeek.ToDayOfWeekCollection(),
            routine.Category,
            routine.IsActive,
            routine.StartDate,
            routine.EndDate,
            routine.CreatedAt,
            routine.UpdatedAt,
            evaluationDate,
            isScheduled,
            isCompleted
        );
    }

    public static RoutineCompletionResponse ToResponse(
        this RoutineCompletion completion
    )
    {
        return new RoutineCompletionResponse(
            completion.Id,
            completion.RoutineId,
            completion.CompletionDate,
            completion.CompletedAt
        );
    }

    public static RoutineDays ToRoutineDays(
        this IEnumerable<DayOfWeek> days
    )
    {
        var result = RoutineDays.None;

        foreach (var day in days.Distinct())
        {
            result |= day.ToRoutineDay();
        }

        return result;
    }

    private static IReadOnlyCollection<DayOfWeek>
        ToDayOfWeekCollection(
            this RoutineDays days
        )
    {
        return OrderedDays
            .Where(
                day =>
                    days.HasFlag(day.ToRoutineDay())
            )
            .ToArray();
    }

    private static RoutineDays ToRoutineDay(
        this DayOfWeek day
    )
    {
        return day switch
        {
            DayOfWeek.Monday => RoutineDays.Monday,
            DayOfWeek.Tuesday => RoutineDays.Tuesday,
            DayOfWeek.Wednesday => RoutineDays.Wednesday,
            DayOfWeek.Thursday => RoutineDays.Thursday,
            DayOfWeek.Friday => RoutineDays.Friday,
            DayOfWeek.Saturday => RoutineDays.Saturday,
            DayOfWeek.Sunday => RoutineDays.Sunday,
            _ => RoutineDays.None
        };
    }
}