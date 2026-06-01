import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateTaskRequest,
  TaskItem,
  TaskQuery,
  UpdateTaskRequest
} from '../models/task.model';

/** Talks to the Tasks REST API. */
@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly tasksUrl = `${environment.apiBaseUrl}/tasks`;

  getTasks(query: TaskQuery = {}): Observable<TaskItem[]> {
    let params = new HttpParams();

    if (query.status && query.status !== 'all') {
      params = params.set('status', query.status);
    }
    if (query.priority) {
      params = params.set('priority', query.priority);
    }
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
    }
    if (query.sortDir) {
      params = params.set('sortDir', query.sortDir);
    }

    return this.http.get<TaskItem[]>(this.tasksUrl, { params });
  }

  getTask(id: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.tasksUrl}/${id}`);
  }

  createTask(request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.tasksUrl, request);
  }

  updateTask(id: number, request: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.tasksUrl}/${id}`, request);
  }

  setCompletion(id: number, isCompleted: boolean): Observable<TaskItem> {
    return this.http.patch<TaskItem>(`${this.tasksUrl}/${id}/status`, { isCompleted });
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.tasksUrl}/${id}`);
  }
}
