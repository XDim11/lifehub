import axios from 'axios'
import { defineStore } from 'pinia'
import { tasksApi } from '@/services/tasksApi'
import type {
  TaskItem,
  TaskQueryParameters,
} from '@/types/task'

interface TasksState {
  tasks: TaskItem[]
  loading: boolean
  error: string | null
}

export const useTasksStore = defineStore('tasks', {
  state: (): TasksState => ({
    tasks: [],
    loading: false,
    error: null,
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
        if (axios.isAxiosError(error)) {
          this.error =
            error.response?.data?.title ??
            error.message
        } else {
          this.error =
            'Se ha producido un error inesperado.'
        }
      } finally {
        this.loading = false
      }
    },
  },
})