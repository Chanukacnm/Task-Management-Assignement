/** Task priority — matches the backend's string enum serialization. */
export type TaskPriority = 'Low' | 'Medium' | 'High';

export const TASK_PRIORITIES: TaskPriority[] = ['Low', 'Medium', 'High'];

/** A task as returned by the API. */
export interface TaskItem {
  id: number;
  title: string;
  description: string | null;
  isCompleted: boolean;
  priority: TaskPriority;
  dueDateUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

/** Payload to create a new task. */
export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  dueDateUtc?: string | null;
}

/** Payload to update an existing task. */
export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  isCompleted: boolean;
  priority: TaskPriority;
  dueDateUtc?: string | null;
}

export type TaskStatusFilter = 'all' | 'active' | 'completed';
export type TaskSortField = 'created' | 'title' | 'priority' | 'duedate' | 'status';
export type SortDirection = 'asc' | 'desc';

/** Filter and sort options sent to the list endpoint. */
export interface TaskQuery {
  status?: TaskStatusFilter;
  priority?: TaskPriority | '';
  search?: string;
  sortBy?: TaskSortField;
  sortDir?: SortDirection;
}
