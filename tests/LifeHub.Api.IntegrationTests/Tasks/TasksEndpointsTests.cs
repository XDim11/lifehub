using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LifeHub.Api.Contracts.Tasks;
using LifeHub.Api.Domain.Enums;
using LifeHub.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LifeHub.Api.IntegrationTests.Tasks;

public sealed class TasksEndpointsTests :
    IClassFixture<LifeHubApiFactory>,
    IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions =
        CreateJsonOptions();

    private readonly LifeHubApiFactory _factory;
    private readonly HttpClient _client;

    public TasksEndpointsTests(LifeHubApiFactory factory)
    {
        _factory = factory;

        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            }
        );
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAll_WhenDatabaseIsEmpty_ReturnsEmptyCollection()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tasks = await response.Content
            .ReadFromJsonAsync<List<TaskResponse>>(JsonOptions);

        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Ir al gimnasio",
            Description = "Entrenamiento de fuerza",
            Priority = TaskItemPriority.High,
            Category = "Salud",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/tasks",
            request,
            JsonOptions
        );

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var task = await response.Content
            .ReadFromJsonAsync<TaskResponse>(JsonOptions);

        Assert.NotNull(task);
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Ir al gimnasio", task.Title);
        Assert.Equal("Entrenamiento de fuerza", task.Description);
        Assert.Equal(TaskItemStatus.Pending, task.Status);
        Assert.Equal(TaskItemPriority.High, task.Priority);
        Assert.Equal("Salud", task.Category);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public async Task Create_WithWhitespaceTitle_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "   ",
            Priority = TaskItemPriority.Medium
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/tasks",
            request,
            JsonOptions
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);

        var titleErrors = problemDetails.Errors
            .Where(error =>
                string.Equals(
                    error.Key,
                    nameof(CreateTaskRequest.Title),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .SelectMany(error => error.Value)
            .ToArray();

        Assert.Contains(
            "El título es obligatorio.",
            titleErrors
        );
    }

    [Fact]
    public async Task GetById_WhenTaskExists_ReturnsTask()
    {
        // Arrange
        var createdTask = await CreateTaskAsync(
            "Comprar comida",
            TaskItemPriority.Medium,
            "Casa"
        );

        // Act
        var response = await _client.GetAsync(
            $"/api/tasks/{createdTask.Id}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var task = await response.Content
            .ReadFromJsonAsync<TaskResponse>(JsonOptions);

        Assert.NotNull(task);
        Assert.Equal(createdTask.Id, task.Id);
        Assert.Equal("Comprar comida", task.Title);
        Assert.Equal("Casa", task.Category);
    }

    [Fact]
    public async Task GetById_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync(
            $"/api/tasks/{Guid.NewGuid()}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenTaskExists_UpdatesTask()
    {
        // Arrange
        var createdTask = await CreateTaskAsync(
            "Estudiar C#",
            TaskItemPriority.Medium,
            "Estudio"
        );

        var request = new UpdateTaskRequest
        {
            Title = "Estudiar ASP.NET Core",
            Description = "Completar el CRUD de tareas",
            Status = TaskItemStatus.InProgress,
            Priority = TaskItemPriority.High,
            Category = "Programación",
            DueDate = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/tasks/{createdTask.Id}",
            request,
            JsonOptions
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedTask = await response.Content
            .ReadFromJsonAsync<TaskResponse>(JsonOptions);

        Assert.NotNull(updatedTask);
        Assert.Equal(createdTask.Id, updatedTask.Id);
        Assert.Equal("Estudiar ASP.NET Core", updatedTask.Title);
        Assert.Equal(
            "Completar el CRUD de tareas",
            updatedTask.Description
        );
        Assert.Equal(
            TaskItemStatus.InProgress,
            updatedTask.Status
        );
        Assert.Equal(
            TaskItemPriority.High,
            updatedTask.Priority
        );
        Assert.Equal("Programación", updatedTask.Category);
        Assert.Null(updatedTask.CompletedAt);
    }

    [Fact]
    public async Task Complete_WhenTaskExists_MarksTaskAsCompleted()
    {
        // Arrange
        var createdTask = await CreateTaskAsync(
            "Terminar los tests",
            TaskItemPriority.High,
            "Programación"
        );

        // Act
        var response = await _client.PatchAsync(
            $"/api/tasks/{createdTask.Id}/complete",
            content: null
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var completedTask = await response.Content
            .ReadFromJsonAsync<TaskResponse>(JsonOptions);

        Assert.NotNull(completedTask);
        Assert.Equal(
            TaskItemStatus.Completed,
            completedTask.Status
        );
        Assert.NotNull(completedTask.CompletedAt);
    }

    [Fact]
    public async Task Delete_WhenTaskExists_RemovesTask()
    {
        // Arrange
        var createdTask = await CreateTaskAsync(
            "Tarea para eliminar",
            TaskItemPriority.Low
        );

        // Act
        var deleteResponse = await _client.DeleteAsync(
            $"/api/tasks/{createdTask.Id}"
        );

        var getResponse = await _client.GetAsync(
            $"/api/tasks/{createdTask.Id}"
        );

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode
        );
    }

    [Fact]
    public async Task GetAll_WithFilters_ReturnsOnlyMatchingTasks()
    {
        // Arrange
        var expectedTask = await CreateTaskAsync(
            "Tarea urgente pendiente",
            TaskItemPriority.High
        );

        await CreateTaskAsync(
            "Tarea de prioridad media",
            TaskItemPriority.Medium
        );

        var completedTask = await CreateTaskAsync(
            "Tarea urgente completada",
            TaskItemPriority.High
        );

        var completeResponse = await _client.PatchAsync(
            $"/api/tasks/{completedTask.Id}/complete",
            content: null
        );

        completeResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync(
            "/api/tasks?status=pending&priority=high"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tasks = await response.Content
            .ReadFromJsonAsync<List<TaskResponse>>(JsonOptions);

        Assert.NotNull(tasks);

        var task = Assert.Single(tasks);

        Assert.Equal(expectedTask.Id, task.Id);
        Assert.Equal(TaskItemStatus.Pending, task.Status);
        Assert.Equal(TaskItemPriority.High, task.Priority);
    }

    private async Task<TaskResponse> CreateTaskAsync(
        string title,
        TaskItemPriority priority,
        string? category = null
    )
    {
        var request = new CreateTaskRequest
        {
            Title = title,
            Priority = priority,
            Category = category
        };

        var response = await _client.PostAsJsonAsync(
            "/api/tasks",
            request,
            JsonOptions
        );

        response.EnsureSuccessStatusCode();

        var task = await response.Content
            .ReadFromJsonAsync<TaskResponse>(JsonOptions);

        Assert.NotNull(task);

        return task;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(
            JsonSerializerDefaults.Web
        );

        options.Converters.Add(
            new JsonStringEnumConverter(
                JsonNamingPolicy.CamelCase,
                allowIntegerValues: false
            )
        );

        return options;
    }
}