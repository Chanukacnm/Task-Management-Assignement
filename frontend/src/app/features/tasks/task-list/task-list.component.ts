import { DatePipe, LowerCasePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TaskItem } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [DatePipe, LowerCasePipe],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent {
  @Input() tasks: TaskItem[] = [];
  @Input() loading = false;
  @Input() selectedId: number | null = null;

  @Output() readonly toggleComplete = new EventEmitter<TaskItem>();
  @Output() readonly edit = new EventEmitter<TaskItem>();
  @Output() readonly remove = new EventEmitter<TaskItem>();

  trackById(_index: number, task: TaskItem): number {
    return task.id;
  }

  isOverdue(task: TaskItem): boolean {
    if (task.isCompleted || !task.dueDateUtc) {
      return false;
    }
    // Compare calendar dates in UTC (consistent with how the due date is shown),
    // so a task due "today" is not flagged overdue regardless of the local timezone.
    const dueDate = task.dueDateUtc.substring(0, 10);
    const todayUtc = new Date().toISOString().substring(0, 10);
    return dueDate < todayUtc;
  }
}
