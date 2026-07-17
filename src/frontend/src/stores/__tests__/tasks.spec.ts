import {
    beforeEach,
    describe,
    expect,
    it,
    vi,
} from 'vitest'
import {
    createPinia,
    setActivePinia,
} from 'pinia'

import { tasksApi } from '@/services/tasksApi'
import { useTasksStore } from '@/stores/tasks'
import type {
    CreateTaskRequest,
    TaskItem,
    UpdateTaskRequest,
} from '@/types/task'

vi.mock('@/services/tasksApi', () => ({
    tasksApi: {
        getAll: vi.fn(),
        create: vi.fn(),
        update: vi.fn(),
        complete: vi.fn(),
        delete: vi.fn(),
    },
}))

function createTaskFixture(
    overrides: Partial<TaskItem> = {},
): TaskItem {
    return {
        id: 'task-1',
        title: 'Tarea de prueba',
        description: null,
        status: 'pending',
        priority: 'medium',
        category: null,
        dueDate: null,
        createdAt: '2026-07-17T08:00:00.000Z',
        updatedAt: '2026-07-17T08:00:00.000Z',
        completedAt: null,
        ...overrides,
    }
}

describe('useTasksStore', () => {
    beforeEach(() => {
        setActivePinia(createPinia())
        vi.resetAllMocks()
    })

    it('carga las tareas desde la API', async () => {
        // Arrange
        const tasks = [
            createTaskFixture(),
            createTaskFixture({
                id: 'task-2',
                title: 'Segunda tarea',
                priority: 'high',
            }),
        ]

        vi.mocked(tasksApi.getAll)
            .mockResolvedValue(tasks)

        const store = useTasksStore()

        // Act
        await store.fetchTasks({
            status: 'pending',
        })

        // Assert
        expect(tasksApi.getAll).toHaveBeenCalledExactlyOnceWith({
            status: 'pending',
        })

        expect(store.tasks).toEqual(tasks)
        expect(store.totalTasks).toBe(2)
        expect(store.loading).toBe(false)
        expect(store.error).toBeNull()
    })

    it('guarda un error cuando falla la carga', async () => {
        // Arrange
        vi.mocked(tasksApi.getAll)
            .mockRejectedValue(
                new Error('Error de conexión'),
            )

        const store = useTasksStore()

        // Act
        await store.fetchTasks()

        // Assert
        expect(store.tasks).toEqual([])
        expect(store.loading).toBe(false)

        expect(store.error).toBe(
            'Se ha producido un error inesperado.',
        )
    })

    it('crea una tarea y la añade al inicio', async () => {
        // Arrange
        const existingTask = createTaskFixture({
            id: 'task-existing',
            title: 'Tarea existente',
        })

        const createdTask = createTaskFixture({
            id: 'task-created',
            title: 'Nueva tarea',
            priority: 'high',
        })

        const request: CreateTaskRequest = {
            title: 'Nueva tarea',
            description: null,
            priority: 'high',
            category: null,
            dueDate: null,
        }

        vi.mocked(tasksApi.create)
            .mockResolvedValue(createdTask)

        const store = useTasksStore()
        store.tasks = [existingTask]

        // Act
        const result = await store.createTask(request)

        // Assert
        expect(tasksApi.create).toHaveBeenCalledWith(
            request,
        )

        expect(result).toEqual(createdTask)
        expect(store.tasks).toHaveLength(2)
        expect(store.tasks[0]).toEqual(createdTask)
        expect(store.tasks[1]).toEqual(existingTask)
        expect(store.saving).toBe(false)
        expect(store.createError).toBeNull()
    })

    it('actualiza una tarea existente', async () => {
        // Arrange
        const originalTask = createTaskFixture()

        const updatedTask = createTaskFixture({
            title: 'Tarea modificada',
            status: 'inProgress',
            priority: 'urgent',
            category: 'Programación',
        })

        const request: UpdateTaskRequest = {
            title: 'Tarea modificada',
            description: null,
            status: 'inProgress',
            priority: 'urgent',
            category: 'Programación',
            dueDate: null,
        }

        vi.mocked(tasksApi.update)
            .mockResolvedValue(updatedTask)

        const store = useTasksStore()
        store.tasks = [originalTask]

        // Act
        const result = await store.updateTask(
            originalTask.id,
            request,
        )

        // Assert
        expect(tasksApi.update).toHaveBeenCalledWith(
            originalTask.id,
            request,
        )

        expect(result).toEqual(updatedTask)
        expect(store.tasks).toEqual([updatedTask])

        expect(
            store.isTaskProcessing(originalTask.id),
        ).toBe(false)

        expect(store.editError).toBeNull()
    })

    it('marca una tarea como completada', async () => {
        // Arrange
        const pendingTask = createTaskFixture()

        const completedTask = createTaskFixture({
            status: 'completed',
            completedAt:
                '2026-07-17T10:00:00.000Z',
        })

        vi.mocked(tasksApi.complete)
            .mockResolvedValue(completedTask)

        const store = useTasksStore()
        store.tasks = [pendingTask]

        // Act
        const result = await store.completeTask(
            pendingTask.id,
        )

        // Assert
        expect(tasksApi.complete).toHaveBeenCalledWith(
            pendingTask.id,
        )

        expect(result).toEqual(completedTask)
        expect(store.tasks).toEqual([completedTask])

        expect(
            store.isTaskProcessing(pendingTask.id),
        ).toBe(false)

        expect(store.actionError).toBeNull()
    })

    it('elimina una tarea del estado', async () => {
        // Arrange
        const taskToDelete = createTaskFixture({
            id: 'task-delete',
        })

        const remainingTask = createTaskFixture({
            id: 'task-remaining',
            title: 'Tarea restante',
        })

        vi.mocked(tasksApi.delete)
            .mockResolvedValue(undefined)

        const store = useTasksStore()

        store.tasks = [
            taskToDelete,
            remainingTask,
        ]

        // Act
        const result = await store.deleteTask(
            taskToDelete.id,
        )

        // Assert
        expect(tasksApi.delete).toHaveBeenCalledWith(
            taskToDelete.id,
        )

        expect(result).toBe(true)
        expect(store.tasks).toEqual([remainingTask])

        expect(
            store.isTaskProcessing(taskToDelete.id),
        ).toBe(false)

        expect(store.actionError).toBeNull()
    })

    it('mantiene la tarea cuando falla la eliminación', async () => {
        // Arrange
        const task = createTaskFixture()

        vi.mocked(tasksApi.delete)
            .mockRejectedValue(
                new Error('Error al eliminar'),
            )

        const store = useTasksStore()
        store.tasks = [task]

        // Act
        const result = await store.deleteTask(task.id)

        // Assert
        expect(result).toBe(false)
        expect(store.tasks).toEqual([task])

        expect(store.actionError).toBe(
            'Se ha producido un error inesperado.',
        )

        expect(
            store.isTaskProcessing(task.id),
        ).toBe(false)
    })
})