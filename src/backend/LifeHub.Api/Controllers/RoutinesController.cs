using LifeHub.Api.Contracts.Routines;
using LifeHub.Api.Data;
using LifeHub.Api.Domain.Entities;
using LifeHub.Api.Domain.Enums;
using LifeHub.Api.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Api.Controllers;

[ApiController]
[Route("api/routines")]
public sealed class RoutinesController : ControllerBase
{
    private readonly LifeHubDbContext _dbContext;

    public RoutinesController(
        LifeHubDbContext dbContext
    )
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType<
        IReadOnlyCollection<RoutineResponse>
    >(StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyCollection<RoutineResponse>>
    > GetAll(
        [FromQuery] RoutineQueryParameters parameters,
        CancellationToken cancellationToken
    )
    {
        IQueryable<Routine> query =
            _dbContext.Routines.AsNoTracking();

        if (parameters.Date.HasValue)
        {
            var date = parameters.Date.Value;

            query = query.Include(
                routine => routine.Completions.Where(
                    completion =>
                        completion.CompletionDate == date
                )
            );
        }

        if (parameters.IsActive.HasValue)
        {
            query = query.Where(
                routine =>
                    routine.IsActive
                    == parameters.IsActive.Value
            );
        }

        if (
            !string.IsNullOrWhiteSpace(
                parameters.Category
            )
        )
        {
            var category = parameters.Category.Trim();

            query = query.Where(
                routine =>
                    routine.Category == category
            );
        }

        if (
            !string.IsNullOrWhiteSpace(
                parameters.Search
            )
        )
        {
            var search = parameters.Search.Trim();

            query = query.Where(
                routine =>
                    routine.Title.Contains(search)
                    || (
                        routine.Description != null
                        && routine.Description.Contains(
                            search
                        )
                    )
            );
        }

        var routines = await query
            .OrderByDescending(
                routine => routine.IsActive
            )
            .ThenBy(routine => routine.Title)
            .ToListAsync(cancellationToken);

        var response = routines
            .Select(
                routine =>
                    routine.ToResponse(parameters.Date)
            )
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RoutineResponse>(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<RoutineResponse>>
        GetById(
            Guid id,
            [FromQuery] DateOnly? date,
            CancellationToken cancellationToken
        )
    {
        IQueryable<Routine> query =
            _dbContext.Routines.AsNoTracking();

        if (date.HasValue)
        {
            var evaluationDate = date.Value;

            query = query.Include(
                routine => routine.Completions.Where(
                    completion =>
                        completion.CompletionDate
                        == evaluationDate
                )
            );
        }

        var routine = await query.FirstOrDefaultAsync(
            routine => routine.Id == id,
            cancellationToken
        );

        if (routine is null)
        {
            return NotFound();
        }

        return Ok(routine.ToResponse(date));
    }

    [HttpPost]
    [ProducesResponseType<RoutineResponse>(
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    public async Task<ActionResult<RoutineResponse>>
        Create(
            CreateRoutineRequest request,
            CancellationToken cancellationToken
        )
    {
        var routine = new Routine
        {
            Title = request.Title.Trim(),
            Description = NormalizeOptionalText(
                request.Description
            ),
            Frequency = request.Frequency,
            DaysOfWeek =
                request.Frequency
                == RoutineFrequency.Daily
                    ? RoutineDays.None
                    : request.DaysOfWeek
                        .ToRoutineDays(),
            Category = NormalizeOptionalText(
                request.Category
            ),
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _dbContext.Routines.Add(routine);

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = routine.Id },
            routine.ToResponse()
        );
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<RoutineResponse>(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<RoutineResponse>>
        Update(
            Guid id,
            UpdateRoutineRequest request,
            CancellationToken cancellationToken
        )
    {
        var routine =
            await _dbContext.Routines
                .FirstOrDefaultAsync(
                    routine => routine.Id == id,
                    cancellationToken
                );

        if (routine is null)
        {
            return NotFound();
        }

        routine.Title = request.Title.Trim();
        routine.Description = NormalizeOptionalText(
            request.Description
        );
        routine.Frequency = request.Frequency;
        routine.DaysOfWeek =
            request.Frequency
            == RoutineFrequency.Daily
                ? RoutineDays.None
                : request.DaysOfWeek
                    .ToRoutineDays();
        routine.Category = NormalizeOptionalText(
            request.Category
        );
        routine.IsActive = request.IsActive;
        routine.StartDate = request.StartDate;
        routine.EndDate = request.EndDate;

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return Ok(routine.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var routine =
            await _dbContext.Routines
                .FirstOrDefaultAsync(
                    routine => routine.Id == id,
                    cancellationToken
                );

        if (routine is null)
        {
            return NotFound();
        }

        _dbContext.Routines.Remove(routine);

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return NoContent();
    }

    [HttpGet("{id:guid}/completions/{date}")]
    [ProducesResponseType<
        RoutineCompletionResponse
    >(StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<
        ActionResult<RoutineCompletionResponse>
    > GetCompletion(
        Guid id,
        DateOnly date,
        CancellationToken cancellationToken
    )
    {
        var completion =
            await _dbContext.RoutineCompletions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    completion =>
                        completion.RoutineId == id
                        && completion.CompletionDate
                        == date,
                    cancellationToken
                );

        if (completion is null)
        {
            return NotFound();
        }

        return Ok(completion.ToResponse());
    }

    [HttpPut("{id:guid}/completions/{date}")]
    [ProducesResponseType<
        RoutineCompletionResponse
    >(StatusCodes.Status200OK)]
    [ProducesResponseType<
        RoutineCompletionResponse
    >(StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<
        ActionResult<RoutineCompletionResponse>
    > Complete(
        Guid id,
        DateOnly date,
        CancellationToken cancellationToken
    )
    {
        var routine =
            await _dbContext.Routines
                .FirstOrDefaultAsync(
                    routine => routine.Id == id,
                    cancellationToken
                );

        if (routine is null)
        {
            return NotFound();
        }

        if (!routine.IsActive)
        {
            return ValidationProblem(
    new ValidationProblemDetails(
        new Dictionary<string, string[]>
        {
            [nameof(Routine.IsActive)] =
            [
                "No se puede completar una rutina pausada."
            ]
        }
    )
);
        }

        if (!routine.IsScheduledOn(date))
        {
            return ValidationProblem(
    new ValidationProblemDetails(
        new Dictionary<string, string[]>
        {
            [nameof(date)] =
            [
                "La rutina no está programada para la fecha indicada."
            ]
        }
    )
);
        }

        var existingCompletion =
            await _dbContext.RoutineCompletions
                .FirstOrDefaultAsync(
                    completion =>
                        completion.RoutineId == id
                        && completion.CompletionDate
                        == date,
                    cancellationToken
                );

        if (existingCompletion is not null)
        {
            return Ok(
                existingCompletion.ToResponse()
            );
        }

        var completion = new RoutineCompletion
        {
            RoutineId = id,
            CompletionDate = date
        };

        _dbContext.RoutineCompletions.Add(
            completion
        );

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetCompletion),
            new
            {
                id,
                date = date.ToString("yyyy-MM-dd")
            },
            completion.ToResponse()
        );
    }

    [HttpDelete("{id:guid}/completions/{date}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Uncomplete(
        Guid id,
        DateOnly date,
        CancellationToken cancellationToken
    )
    {
        var completion =
            await _dbContext.RoutineCompletions
                .FirstOrDefaultAsync(
                    completion =>
                        completion.RoutineId == id
                        && completion.CompletionDate
                        == date,
                    cancellationToken
                );

        if (completion is null)
        {
            return NotFound();
        }

        _dbContext.RoutineCompletions.Remove(
            completion
        );

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return NoContent();
    }

    private static string? NormalizeOptionalText(
        string? value
    )
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}