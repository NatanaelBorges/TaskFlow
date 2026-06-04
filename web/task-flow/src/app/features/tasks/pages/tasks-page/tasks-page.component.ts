import { Component, OnInit, inject, signal } from '@angular/core';

import { TaskService } from '../../../../core/services/task.service';
import { Task, TaskFilter } from '../../../../core/models/task.model';
import { TaskListComponent } from '../../components/task-list/task-list.component';
import { TaskFiltersComponent } from '../../components/task-filters/task-filters.component';
import { TaskFormComponent } from '../../components/task-form/task-form.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-tasks-page',
  imports: [TaskListComponent, TaskFiltersComponent, TaskFormComponent, ConfirmDialogComponent],
  template: `
    <div class="mx-auto max-w-2xl px-4 py-8">
      <div class="mb-6 flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-slate-800">My Tasks</h1>
          <p class="text-sm text-slate-500">
            {{ taskService.pagination().totalItems }} task{{
              taskService.pagination().totalItems !== 1 ? 's' : ''
            }}
          </p>
        </div>
        <button
          (click)="openCreate()"
          class="flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 transition-colors shadow-sm"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="h-4 w-4"
            viewBox="0 0 20 20"
            fill="currentColor"
          >
            <path
              fill-rule="evenodd"
              d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z"
              clip-rule="evenodd"
            />
          </svg>
          New Task
        </button>
      </div>

      <div class="mb-5">
        <app-task-filters
          [activeFilter]="taskService.filter()"
          (filterChanged)="onFilterChange($event)"
        />
      </div>

      <app-task-list (edit)="openEdit($event)" />
    </div>

    @if (showForm()) {
      <app-task-form [task]="editingTask()" (submitted)="closeForm()" (cancelled)="closeForm()" />
    }

    @if (pendingDelete()) {
      <app-confirm-dialog
        title="Delete Task"
        [message]="'Delete &quot;' + pendingDelete()!.title + '&quot;? This cannot be undone.'"
        confirmLabel="Delete"
        (confirmed)="confirmDelete()"
        (cancelled)="pendingDelete.set(null)"
      />
    }
  `,
})
export class TasksPageComponent implements OnInit {
  readonly taskService = inject(TaskService);
  private readonly toast = inject(ToastService);

  readonly showForm = signal(false);
  readonly editingTask = signal<Task | null>(null);
  readonly pendingDelete = signal<Task | null>(null);

  ngOnInit(): void {
    this.taskService.loadTasks();
  }

  openCreate(): void {
    this.editingTask.set(null);
    this.showForm.set(true);
  }

  openEdit(task: Task): void {
    this.editingTask.set(task);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingTask.set(null);
  }

  onFilterChange(filter: TaskFilter): void {
    this.taskService.setFilter(filter);
  }

  confirmDelete(): void {
    const task = this.pendingDelete();
    if (!task) return;

    this.taskService.deleteTask(task.id).subscribe({
      next: () => this.toast.show('Task deleted.', 'success'),
      error: () => this.toast.show('Could not delete the task.', 'error'),
    });
    this.pendingDelete.set(null);
  }
}
