import { http } from './http'
import type {
  CreateTaskRequest,
  TaskItem,
  TaskQueryParameters,
} from '@/types/task'

export const tasksApi = {
  async getAll(
    parameters: TaskQueryParameters = {},
  ): Promise<TaskItem[]> {
    const response = await http.get<TaskItem[]>(
      '/api/tasks',
      {
        params: parameters,
      },
    )

    return response.data
  },

  async create(
    request: CreateTaskRequest,
  ): Promise<TaskItem> {
    const response = await http.post<TaskItem>(
      '/api/tasks',
      request,
    )

    return response.data
  },
}