export type TaskStatus =
  | 'pending'
  | 'inProgress'
  | 'completed'
  | 'cancelled'

export type TaskPriority =
  | 'low'
  | 'medium'
  | 'high'
  | 'urgent'

export type TaskSortBy =
  | 'dueDate'
  | 'createdAt'
  | 'title'

export interface TaskItem {
  id: string
  title: string
  description: string | null
  status: TaskStatus
  priority: TaskPriority
  category: string | null
  dueDate: string | null
  createdAt: string
  updatedAt: string
  completedAt: string | null
}

export interface TaskQueryParameters {
  status?: TaskStatus
  priority?: TaskPriority
  category?: string
  search?: string
  sortBy?: TaskSortBy
  descending?: boolean
}