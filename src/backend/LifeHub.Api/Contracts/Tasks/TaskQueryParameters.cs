using System.ComponentModel.DataAnnotations;
using LifeHub.Api.Domain.Enums;

namespace LifeHub.Api.Contracts.Tasks;

public sealed record TaskQueryParameters
{
    public TaskItemStatus? Status { get; init; }

    public TaskItemPriority? Priority { get; init; }

    [StringLength(
        80,
        ErrorMessage = "La categoría no puede superar los 80 caracteres."
    )]
    public string? Category { get; init; }

    [StringLength(
        150,
        ErrorMessage = "La búsqueda no puede superar los 150 caracteres."
    )]
    public string? Search { get; init; }

    public TaskSortBy SortBy { get; init; } = TaskSortBy.DueDate;

    public bool Descending { get; init; }
}