using LifeHub.Api.Contracts.Tasks;
using LifeHub.Api.Data;
using LifeHub.Api.Domain.Entities;
using LifeHub.Api.Domain.Enums;
using LifeHub.Api.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly LifeHubDbContext _dbContext;

    public TasksController(LifeHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<TaskResponse>>(
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<IReadOnlyCollection<TaskResponse>>> GetAll(
        [FromQuery] TaskQueryParameters parameters,
        CancellationToken cancellationToken
    )
    {
        IQueryable<TaskItem> query = _dbContext.Tasks.AsNoTracking();

        if (parameters.Status.HasValue)
        {
            query = query.Where(
                task => task.Status == parameters.Status.Value
            );
        }

        if (parameters.Priority.HasValue)
        {
            query = query.Where(
                task => task.Priority == parameters.Priority.Value
            );
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            var category = parameters.Category.Trim();

            query = query.Where(task => task.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();

            query = query.Where(task =>
                task.Title.Contains(search)
                || (
                    task.Description != null
                    && task.Description.Contains(search)
                )
            );
        }

        query = ApplySorting(query, parameters);

        var tasks = await query.ToListAsync(cancellationToken);

        var response = tasks
            .Select(task => task.ToResponse())
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var task = await _dbContext.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(
                task => task.Id == id,
                cancellationToken
            );

        if (task is null)
        {
            return NotFound();
        }

        return Ok(task.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken cancellationToken
    )
    {
        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = NormalizeOptionalText(request.Description),
            Priority = request.Priority,
            Category = NormalizeOptionalText(request.Category),
            DueDate = request.DueDate
        };

        _dbContext.Tasks.Add(task);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = task.Id },
            task.ToResponse()
        );
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken
    )
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(
            task => task.Id == id,
            cancellationToken
        );

        if (task is null)
        {
            return NotFound();
        }

        task.Title = request.Title.Trim();
        task.Description = NormalizeOptionalText(request.Description);
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.Category = NormalizeOptionalText(request.Category);
        task.DueDate = request.DueDate;

        task.CompletedAt = request.Status == TaskItemStatus.Completed
            ? task.CompletedAt ?? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(task.ToResponse());
    }

    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Complete(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(
            task => task.Id == id,
            cancellationToken
        );

        if (task is null)
        {
            return NotFound();
        }

        if (task.Status != TaskItemStatus.Completed)
        {
            task.Status = TaskItemStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(task.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(
            task => task.Id == id,
            cancellationToken
        );

        if (task is null)
        {
            return NotFound();
        }

        _dbContext.Tasks.Remove(task);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static IQueryable<TaskItem> ApplySorting(
        IQueryable<TaskItem> query,
        TaskQueryParameters parameters
    )
    {
        return parameters.SortBy switch
        {
            TaskSortBy.CreatedAt when parameters.Descending =>
                query.OrderByDescending(task => task.CreatedAt),

            TaskSortBy.CreatedAt =>
                query.OrderBy(task => task.CreatedAt),

            TaskSortBy.Title when parameters.Descending =>
                query.OrderByDescending(task => task.Title),

            TaskSortBy.Title =>
                query.OrderBy(task => task.Title),

            TaskSortBy.DueDate when parameters.Descending =>
                query
                    .OrderBy(task => task.DueDate == null)
                    .ThenByDescending(task => task.DueDate)
                    .ThenByDescending(task => task.CreatedAt),

            _ =>
                query
                    .OrderBy(task => task.DueDate == null)
                    .ThenBy(task => task.DueDate)
                    .ThenByDescending(task => task.CreatedAt)
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}