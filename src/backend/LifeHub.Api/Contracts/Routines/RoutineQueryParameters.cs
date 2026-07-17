using System.ComponentModel.DataAnnotations;

namespace LifeHub.Api.Contracts.Routines;

public sealed record RoutineQueryParameters
{
    public bool? IsActive { get; init; }

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

    public DateOnly? Date { get; init; }
}