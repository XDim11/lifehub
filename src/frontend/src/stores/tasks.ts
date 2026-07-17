import axios from 'axios'
import { defineStore } from 'pinia'
import { tasksApi } from '@/services/tasksApi'
import type {
  CreateTaskRequest,
  TaskItem,
  TaskQueryParameters,
} from '@/types/task'

interface ValidationProblemDetails {
  title?: string
  errors?: Record<string, string[]>
}

interface TasksState {
  tasks: TaskItem[]
  loading: boolean
  saving: boolean
  processingTaskIds: string[]
  error: string | null
  createError: string | null
  actionError: string | null
}

function getApiErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return 'Se ha producido un error inesperado.'
  }

  const problemDetails =
    error.response?.data as ValidationProblemDetails | undefined

  const firstValidationError = problemDetails?.errors
    ? Object.values(problemDetails.errors)
        .flat()
        .find((message) => message.length > 0)
    : undefined

  return (
    firstValidationError ??
    problemDetails?.title ??
    error.message ??
    'No se ha podido completar la operación.'
  )
}

export const useTasksStore = defineStore('tasks', {
  state: (): TasksState => ({
    tasks: [],
    loading: false,
    saving: false,
    processingTaskIds: [],
    error: null,
    createError: null,
    actionError: null,
  }),

  getters: {
    totalTasks: (state) => state.tasks.length,

    pendingTasks: (state) =>
      state.tasks.filter(
        (task) => task.status === 'pending',
      ),

    completedTasks: (state) =>
      state.tasks.filter(
        (task) => task.status === 'completed',
      ),

    isTaskProcessing:
      (state) =>
      (taskId: string): boolean =>
        state.processingTaskIds.includes(taskId),
  },

  actions: {
    async fetchTasks(
      parameters: TaskQueryParameters = {},
    ): Promise<void> {
      this.loading = true
      this.error = null

      try {
        this.tasks = await tasksApi.getAll(parameters)
      } catch (error: unknown) {
        this.error = getApiErrorMessage(error)
      } finally {
        this.loading = false
      }
    },

    async createTask(
      request: CreateTaskRequest,
    ): Promise<TaskItem | null> {
      this.saving = true
      this.createError = null

      try {
        const createdTask =
          await tasksApi.create(request)

        this.tasks.unshift(createdTask)

        return createdTask
      } catch (error: unknown) {
        this.createError =
          getApiErrorMessage(error)

        return null
      } finally {
        this.saving = false
      }
    },

    async completeTask(
      taskId: string,
    ): Promise<TaskItem | null> {
      this.actionError = null
      this.startTaskProcessing(taskId)

      try {
        const completedTask =
          await tasksApi.complete(taskId)

        const taskIndex = this.tasks.findIndex(
          (task) => task.id === taskId,
        )

        if (taskIndex !== -1) {
          this.tasks[taskIndex] = completedTask
        }

        return completedTask
      } catch (error: unknown) {
        this.actionError =
          getApiErrorMessage(error)

        return null
      } finally {
        this.stopTaskProcessing(taskId)
      }
    },

    async deleteTask(
      taskId: string,
    ): Promise<boolean> {
      this.actionError = null
      this.startTaskProcessing(taskId)

      try {
        await tasksApi.delete(taskId)

        this.tasks = this.tasks.filter(
          (task) => task.id !== taskId,
        )

        return true
      } catch (error: unknown) {
        this.actionError =
          getApiErrorMessage(error)

        return false
      } finally {
        this.stopTaskProcessing(taskId)
      }
    },

    startTaskProcessing(taskId: string): void {
      if (!this.processingTaskIds.includes(taskId)) {
        this.processingTaskIds.push(taskId)
      }
    },

    stopTaskProcessing(taskId: string): void {
      this.processingTaskIds =
        this.processingTaskIds.filter(
          (id) => id !== taskId,
        )
    },

    clearCreateError(): void {
      this.createError = null
    },

    clearActionError(): void {
      this.actionError = null
    },
  },
})