import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TASK_PRIORITIES, TaskItem, TaskPriority } from '../../../core/models/task.model';

/** Values emitted by the form when the user saves. */
export interface TaskFormResult {
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDateUtc: string | null;
  isCompleted: boolean;
}

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent {
  private readonly fb = inject(FormBuilder);

  protected readonly priorities = TASK_PRIORITIES;

  private _task: TaskItem | null = null;

  /** The task being edited, or null when creating a new task. */
  @Input()
  set task(value: TaskItem | null) {
    this._task = value;
    this.patchForm(value);
  }
  get task(): TaskItem | null {
    return this._task;
  }

  @Input() saving = false;

  @Output() readonly save = new EventEmitter<TaskFormResult>();
  @Output() readonly cancelEdit = new EventEmitter<void>();

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    priority: ['Medium' as TaskPriority, [Validators.required]],
    dueDate: [''],
    isCompleted: [false]
  });

  get isEditing(): boolean {
    return this._task !== null;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const description = value.description.trim();

    this.save.emit({
      title: value.title.trim(),
      description: description.length > 0 ? description : null,
      priority: value.priority,
      dueDateUtc: value.dueDate ? new Date(`${value.dueDate}T00:00:00Z`).toISOString() : null,
      isCompleted: value.isCompleted
    });
  }

  reset(): void {
    if (this.isEditing) {
      this.cancelEdit.emit();
    } else {
      this.patchForm(null);
    }
  }

  /** Clears the form back to its empty "create" state. Called by the parent after a successful create. */
  clear(): void {
    this.patchForm(null);
  }

  private patchForm(task: TaskItem | null): void {
    this.form.reset({
      title: task?.title ?? '',
      description: task?.description ?? '',
      priority: task?.priority ?? 'Medium',
      dueDate: task?.dueDateUtc ? task.dueDateUtc.substring(0, 10) : '',
      isCompleted: task?.isCompleted ?? false
    });
  }
}
