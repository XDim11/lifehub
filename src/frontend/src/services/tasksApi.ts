import { http } from './http'
import type {
  CreateTaskRequest,
  TaskItem,
  TaskQueryParameters,
  UpdateTaskRequest,
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

  async update(
    id: string,
    request: UpdateTaskRequest,
  ): Promise<TaskItem> {
    const response = await http.put<TaskItem>(
      `/api/tasks/${id}`,
      request,
    )

    return response.data
  },

  async complete(id: string): Promise<TaskItem> {
    const response = await http.patch<TaskItem>(
      `/api/tasks/${id}/complete`,
    )

    return response.data
  },

  async delete(id: string): Promise<void> {
    await http.delete(`/api/tasks/${id}`)
  },
}