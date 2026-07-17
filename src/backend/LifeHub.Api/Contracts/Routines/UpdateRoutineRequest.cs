using System.ComponentModel.DataAnnotations;
using LifeHub.Api.Domain.Enums;

namespace LifeHub.Api.Contracts.Routines;

public sealed record UpdateRoutineRequest : IValidatableObject
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(
        150,
        MinimumLength = 1,
        ErrorMessage = "El título debe tener entre 1 y 150 caracteres."
    )]
    public string Title { get; init; } = string.Empty;

    [StringLength(
        1000,
        ErrorMessage = "La descripción no puede superar los 1000 caracteres."
    )]
    public string? Description { get; init; }

    [EnumDataType(
        typeof(RoutineFrequency),
        ErrorMessage = "La frecuencia indicada no es válida."
    )]
    public RoutineFrequency Frequency { get; init; } =
        RoutineFrequency.Daily;

    public IReadOnlyCollection<DayOfWeek> DaysOfWeek
    {
        get;
        init;
    } = Array.Empty<DayOfWeek>();

    [StringLength(
        80,
        ErrorMessage = "La categoría no puede superar los 80 caracteres."
    )]
    public string? Category { get; init; }

    public bool IsActive { get; init; } = true;

    public DateOnly StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext
    )
    {
        if (
            EndDate.HasValue
            && EndDate.Value < StartDate
        )
        {
            yield return new ValidationResult(
                "La fecha de finalización no puede ser anterior a la fecha de inicio.",
                new[] { nameof(EndDate) }
            );
        }

        if (
            Frequency == RoutineFrequency.Weekly
            && DaysOfWeek.Count == 0
        )
        {
            yield return new ValidationResult(
                "Una rutina semanal debe tener al menos un día seleccionado.",
                new[] { nameof(DaysOfWeek) }
            );
        }
    }
}