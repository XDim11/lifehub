using System.ComponentModel.DataAnnotations;
using LifeHub.Api.Domain.Enums;

namespace LifeHub.Api.Contracts.Tasks;

public sealed record UpdateTaskRequest : IValidatableObject
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
        typeof(TaskItemStatus),
        ErrorMessage = "El estado indicado no es válido."
    )]
    public TaskItemStatus Status { get; init; } = TaskItemStatus.Pending;

    [EnumDataType(
        typeof(TaskItemPriority),
        ErrorMessage = "La prioridad indicada no es válida."
    )]
    public TaskItemPriority Priority { get; init; } = TaskItemPriority.Medium;

    [StringLength(
        80,
        ErrorMessage = "La categoría no puede superar los 80 caracteres."
    )]
    public string? Category { get; init; }

    public DateTime? DueDate { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext
    )
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult(
                "El título no puede estar vacío.",
                new[] { nameof(Title) }
            );
        }
    }
}