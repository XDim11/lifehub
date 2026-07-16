using LifeHub.Api.Contracts.Tasks;
using LifeHub.Api.Domain.Entities;

namespace LifeHub.Api.Mappings;

public static class TaskItemMappings
{
    public static TaskResponse ToResponse(this TaskItem task)
    {
        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.Category,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt,
            task.CompletedAt
        );
    }
}