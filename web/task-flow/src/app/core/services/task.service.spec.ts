import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';

import { TaskService } from './task.service';
import { RawTask, Task, TaskFilter, TaskStatus } from '../models/task.model';
import { PaginatedResponse } from '../models/api.model';

const rawActiveTask: RawTask = {
  id: '123e4567-e89b-12d3-a456-426614174000',
  title: 'Test Task',
  description: 'A description',
  status: 0,
  createdAtUtc: '2026-06-04T00:00:00Z',
  updatedAtUtc: null,
};

const pagedOf = (tasks: RawTask[]): PaginatedResponse<RawTask> => ({
  data: tasks,
  page: '1',
  pageSize: '10',
  totalItems: String(tasks.length),
  totalPages: '1',
});

describe('TaskService', () => {
  let service: TaskService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TaskService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('loadTasks', () => {
    it('should populate tasks signal on success', () => {
      service.loadTasks();

      httpMock.expectOne((r) => r.url.includes('/tasks')).flush(pagedOf([rawActiveTask]));

      expect(service.tasks()).toHaveLength(1);
      expect(service.tasks()[0].id).toBe(rawActiveTask.id);
    });

    it('should coerce status string "0" → TaskStatus.Active', () => {
      service.loadTasks();

      httpMock
        .expectOne((r) => r.url.includes('/tasks'))
        .flush(pagedOf([{ ...rawActiveTask, status: '0' }]));

      expect(service.tasks()[0].status).toBe(TaskStatus.Active);
    });

    it('should coerce status string "1" → TaskStatus.Completed', () => {
      service.loadTasks();

      httpMock
        .expectOne((r) => r.url.includes('/tasks'))
        .flush(pagedOf([{ ...rawActiveTask, status: '1' }]));

      expect(service.tasks()[0].status).toBe(TaskStatus.Completed);
    });

    it('should coerce status number 0 → TaskStatus.Active (no-op case)', () => {
      service.loadTasks();

      httpMock
        .expectOne((r) => r.url.includes('/tasks'))
        .flush(pagedOf([{ ...rawActiveTask, status: 0 }]));

      expect(service.tasks()[0].status).toBe(TaskStatus.Active);
    });

    it('should set loading to false after response', () => {
      service.loadTasks();
      expect(service.loading()).toBe(true);

      httpMock.expectOne((r) => r.url.includes('/tasks')).flush(pagedOf([]));

      expect(service.loading()).toBe(false);
    });

    it('should set error signal on HTTP failure', () => {
      service.loadTasks();

      httpMock.expectOne((r) => r.url.includes('/tasks')).error(new ProgressEvent('error'));

      expect(service.error()).not.toBeNull();
      expect(service.loading()).toBe(false);
    });

    it('should send status=0 param when filter is active', () => {
      service.setFilter('active');

      const req = httpMock.expectOne((r) => r.url.includes('/tasks'));
      expect(req.request.params.get('status')).toBe('0');
      req.flush(pagedOf([]));
    });

    it('should send status=1 param when filter is completed', () => {
      service.setFilter('completed');

      const req = httpMock.expectOne((r) => r.url.includes('/tasks'));
      expect(req.request.params.get('status')).toBe('1');
      req.flush(pagedOf([]));
    });

    it('should send status="" param when filter is all', () => {
      service.setFilter('all');

      const req = httpMock.expectOne((r) => r.url.includes('/tasks'));
      expect(req.request.params.get('status')).toBe('');
      req.flush(pagedOf([]));
    });
  });

  describe('setFilter', () => {
    it('should update the filter signal', () => {
      const filters: TaskFilter[] = ['active', 'completed', 'all'];

      for (const filter of filters) {
        service.setFilter(filter);
        expect(service.filter()).toBe(filter);
        httpMock.expectOne((r) => r.url.includes('/tasks')).flush(pagedOf([]));
      }
    });
  });

  describe('createTask', () => {
    it('should POST and map the response status to enum', () => {
      let result: Task | undefined;
      service.createTask({ title: 'New', description: '' }).subscribe((t) => (result = t));

      const postReq = httpMock.expectOne((r) => r.method === 'POST');
      expect(postReq.request.body).toEqual({ title: 'New', description: '' });
      postReq.flush({ ...rawActiveTask, status: '0' });

      httpMock.expectOne((r) => r.method === 'GET').flush(pagedOf([]));

      expect(result?.status).toBe(TaskStatus.Active);
    });
  });

  describe('updateTask', () => {
    it('should PUT to the correct URL and reload tasks', () => {
      service.updateTask(rawActiveTask.id, { title: 'Updated', description: '' }).subscribe();

      const putReq = httpMock.expectOne((r) => r.method === 'PUT');
      expect(putReq.request.url).toContain(rawActiveTask.id);
      putReq.flush({ ...rawActiveTask, title: 'Updated' });

      httpMock.expectOne((r) => r.method === 'GET').flush(pagedOf([]));
    });
  });

  describe('deleteTask', () => {
    it('should DELETE the correct resource and reload tasks', () => {
      service.deleteTask(rawActiveTask.id).subscribe();

      const delReq = httpMock.expectOne((r) => r.method === 'DELETE');
      expect(delReq.request.url).toContain(rawActiveTask.id);
      delReq.flush(null, { status: 204, statusText: 'No Content' });

      httpMock.expectOne((r) => r.method === 'GET').flush(pagedOf([]));
    });
  });

  describe('toggleStatus', () => {
    it('should PATCH the status endpoint and map the response', () => {
      let result: Task | undefined;
      service.toggleStatus(rawActiveTask.id).subscribe((t) => (result = t));

      const patchReq = httpMock.expectOne((r) => r.method === 'PATCH');
      expect(patchReq.request.url).toContain('/status');
      patchReq.flush({ ...rawActiveTask, status: '1' });

      httpMock.expectOne((r) => r.method === 'GET').flush(pagedOf([]));

      expect(result?.status).toBe(TaskStatus.Completed);
    });
  });
});
