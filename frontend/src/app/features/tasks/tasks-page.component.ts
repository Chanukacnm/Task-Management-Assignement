import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  DestroyRef,
  OnDestroy,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { EMPTY, Subject, catchError, switchMap } from 'rxjs';
import {
  SortDirection,
  TaskItem,
  TaskPriority,
  TaskSortField,
  TaskStatusFilter
} from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';
import { ToastService } from '../../core/services/toast.service';
import { TaskFormComponent, TaskFormResult } from './task-form/task-form.component';
import { TaskListComponent } from './task-list/task-list.component';

@Component({
  selector: 'app-tasks-page',
  standalone: true,
  imports: [FormsModule, TaskListComponent, TaskFormComponent],
  templateUrl: './tasks-page.component.html',
  styleUrl: './tasks-page.component.scss'
})
export class TasksPageComponent implements OnInit, OnDestroy {
  private readonly taskService = inject(TaskService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild(TaskFormComponent) private taskForm?: TaskFormComponent;

  protected readonly tasks = signal<TaskItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly selectedTask = signal<TaskItem | null>(null);

  protected readonly totalCount = computed(() => this.tasks().length);
  protected readonly completedCount = computed(
    () => this.tasks().filter((task) => task.isCompleted).length
  );

  // Filter / sort state (bound with ngModel in the toolbar).
  protected search = '';
  protected status: TaskStatusFilter = 'all';
  protected priority: TaskPriority | '' = '';
  protected sortBy: TaskSortField = 'created';
  protected sortDir: SortDirection = 'desc';

  // A reload request stream; switchMap cancels any in-flight request so a slower
  // earlier response can never overwrite a newer one.
  private readonly reload$ = new Subject<void>();
  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.reload$
      .pipe(
        switchMap(() => {
          this.loading.set(true);
          return this.taskService
            .getTasks({
              status: this.status,
              priority: this.priority,
              search: this.search,
              sortBy: this.sortBy,
              sortDir: this.sortDir
            })
            .pipe(
              catchError(() => {
                this.loading.set(false);
                this.toast.error('Failed to load tasks. Is the API running?');
                return EMPTY;
              })
            );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((tasks) => {
        this.tasks.set(tasks);
        this.loading.set(false);
      });

    this.loadTasks();
  }

  ngOnDestroy(): void {
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
  }

  loadTasks(): void {
    this.reload$.next();
  }

  onSearchChange(value: string): void {
    this.search = value;
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    this.searchTimer = setTimeout(() => this.loadTasks(), 300);
  }

  onFilterChange(): void {
    this.loadTasks();
  }

  toggleSortDir(): void {
    this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    this.loadTasks();
  }

  startCreate(): void {
    this.selectedTask.set(null);
    this.taskForm?.clear();
  }

  startEdit(task: TaskItem): void {
    this.selectedTask.set(task);
  }

  cancelEdit(): void {
    this.selectedTask.set(null);
  }

  onSave(result: TaskFormResult): void {
    const editing = this.selectedTask();
    this.saving.set(true);

    if (editing) {
      this.taskService
        .updateTask(editing.id, {
          title: result.title,
          description: result.description,
          isCompleted: result.isCompleted,
          priority: result.priority,
          dueDateUtc: result.dueDateUtc
        })
        .subscribe({
          next: () => {
            this.saving.set(false);
            this.selectedTask.set(null);
            this.toast.success('Task updated.');
            this.loadTasks();
          },
          error: (error: HttpErrorResponse) => this.handleSaveError(error)
        });
    } else {
      this.taskService
        .createTask({
          title: result.title,
          description: result.description,
          priority: result.priority,
          dueDateUtc: result.dueDateUtc
        })
        .subscribe({
          next: () => {
            this.saving.set(false);
            this.taskForm?.clear();
            this.toast.success('Task created.');
            this.loadTasks();
          },
          error: (error: HttpErrorResponse) => this.handleSaveError(error)
        });
    }
  }

  toggleComplete(task: TaskItem): void {
    this.taskService.setCompletion(task.id, !task.isCompleted).subscribe({
      next: (updated) => {
        this.toast.success(updated.isCompleted ? 'Task completed.' : 'Task reopened.');
        if (this.selectedTask()?.id === updated.id) {
          this.selectedTask.set(updated);
        }
        this.loadTasks();
      },
      error: () => this.toast.error('Failed to update task status.')
    });
  }

  deleteTask(task: TaskItem): void {
    const confirmed = confirm(`Delete task "${task.title}"? This cannot be undone.`);
    if (!confirmed) {
      return;
    }

    this.taskService.deleteTask(task.id).subscribe({
      next: () => {
        if (this.selectedTask()?.id === task.id) {
          this.selectedTask.set(null);
        }
        this.toast.success('Task deleted.');
        this.loadTasks();
      },
      error: () => this.toast.error('Failed to delete task.')
    });
  }

  private handleSaveError(error: HttpErrorResponse): void {
    this.saving.set(false);

    const validationErrors = error.status === 400 ? error.error?.errors : null;
    if (validationErrors) {
      const messages = Object.values(validationErrors).flat() as string[];
      this.toast.error(messages[0] ?? 'Validation failed.');
    } else {
      this.toast.error('Failed to save the task.');
    }
  }
}
