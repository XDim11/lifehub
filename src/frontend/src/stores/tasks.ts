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
  error: string | null
  mutationError: string | null
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
    error: null,
    mutationError: null,
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
      this.mutationError = null

      try {
        const createdTask =
          await tasksApi.create(request)

        this.tasks.unshift(createdTask)

        return createdTask
      } catch (error: unknown) {
        this.mutationError =
          getApiErrorMessage(error)

        return null
      } finally {
        this.saving = false
      }
    },

    clearMutationError(): void {
      this.mutationError = null
    },
  },
})