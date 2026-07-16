import { http } from './http'
import type {
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
}