import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, EMPTY } from 'rxjs';
import { tap, catchError, finalize, map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import {
  Task,
  RawTask,
  TaskFilter,
  CreateTaskDto,
  UpdateTaskDto,
  mapTask,
} from '../models/task.model';
import { PaginatedResponse, Pagination } from '../models/api.model';

const PAGE_SIZE = 10;

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/v1`;

  private readonly _tasks = signal<Task[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _filter = signal<TaskFilter>('all');
  private readonly _pagination = signal<Pagination>({
    page: 1,
    pageSize: PAGE_SIZE,
    totalItems: 0,
    totalPages: 0,
  });

  readonly tasks = this._tasks.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly filter = this._filter.asReadonly();
  readonly pagination = this._pagination.asReadonly();

  loadTasks(page = 1): void {
    this._loading.set(true);
    this._error.set(null);

    const filter = this._filter();
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(PAGE_SIZE));

    if (filter === 'active') {
      params = params.set('status', '0');
    } else if (filter === 'completed') {
      params = params.set('status', '1');
    } else {
      params = params.set('status', '');
    }

    this.http
      .get<PaginatedResponse<RawTask>>(`${this.apiUrl}/tasks`, { params })
      .pipe(
        tap((res) => {
          this._tasks.set(res.data.map(mapTask));
          this._pagination.set({
            page: Number(res.page),
            pageSize: Number(res.pageSize),
            totalItems: Number(res.totalItems),
            totalPages: Number(res.totalPages),
          });
        }),
        catchError(() => {
          this._error.set('Failed to load tasks. Please try again.');
          return EMPTY;
        }),
        finalize(() => this._loading.set(false)),
      )
      .subscribe();
  }

  setFilter(filter: TaskFilter): void {
    this._filter.set(filter);
    this.loadTasks(1);
  }

  createTask(dto: CreateTaskDto): Observable<Task> {
    return this.http.post<RawTask>(`${this.apiUrl}/tasks`, dto).pipe(
      map(mapTask),
      tap(() => this.loadTasks(this._pagination().page)),
    );
  }

  updateTask(id: string, dto: UpdateTaskDto): Observable<Task> {
    return this.http.put<RawTask>(`${this.apiUrl}/tasks/${id}`, dto).pipe(
      map(mapTask),
      tap(() => this.loadTasks(this._pagination().page)),
    );
  }

  deleteTask(id: string): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/tasks/${id}`)
      .pipe(tap(() => this.loadTasks(this._pagination().page)));
  }

  toggleStatus(id: string): Observable<Task> {
    return this.http.patch<RawTask>(`${this.apiUrl}/tasks/${id}/status`, null).pipe(
      map(mapTask),
      tap(() => this.loadTasks(this._pagination().page)),
    );
  }
}
