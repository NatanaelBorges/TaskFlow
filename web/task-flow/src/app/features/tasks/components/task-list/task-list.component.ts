import { Component, inject, output } from '@angular/core';

import { TaskService } from '../../../../core/services/task.service';
import { Task, TaskStatus } from '../../../../core/models/task.model';
import { TaskCardComponent } from '../task-card/task-card.component';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-task-list',
  imports: [TaskCardComponent],
  template: `
    @if (taskService.loading()) {
      <div class="flex flex-col gap-3">
        @for (_ of skeletons; track $index) {
          <div class="h-24 animate-pulse rounded-xl bg-slate-100"></div>
        }
      </div>
    } @else if (taskService.error()) {
      <div class="rounded-xl border border-red-200 bg-red-50 px-5 py-10 text-center">
        <p class="text-sm font-medium text-red-600">{{ taskService.error() }}</p>
        <button
          (click)="taskService.loadTasks()"
          class="mt-3 text-sm font-medium text-red-700 underline hover:no-underline"
        >
          Try again
        </button>
      </div>
    } @else if (taskService.tasks().length === 0) {
      <div class="rounded-xl border-2 border-dashed border-slate-200 px-5 py-16 text-center">
        <svg
          class="mx-auto h-10 w-10 text-slate-300"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="1.5"
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
          />
        </svg>
        <p class="mt-3 text-sm text-slate-500">No tasks here yet.</p>
        <p class="text-xs text-slate-400">Create your first task to get started.</p>
      </div>
    } @else {
      <div class="flex flex-col gap-3">
        @for (task of taskService.tasks(); track task.id) {
          <app-task-card
            [task]="task"
            (edit)="edit.emit($event)"
            (delete)="deleteTask($event)"
            (toggleStatus)="toggleStatus($event)"
          />
        }
      </div>

      @if (taskService.pagination().totalPages > 1) {
        <div class="mt-6 flex items-center justify-center gap-2">
          <button
            [disabled]="taskService.pagination().page === 1"
            (click)="taskService.loadTasks(taskService.pagination().page - 1)"
            class="rounded-lg px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            Previous
          </button>
          <span class="text-sm text-slate-500">
            Page {{ taskService.pagination().page }} of {{ taskService.pagination().totalPages }}
          </span>
          <button
            [disabled]="taskService.pagination().page === taskService.pagination().totalPages"
            (click)="taskService.loadTasks(taskService.pagination().page + 1)"
            class="rounded-lg px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            Next
          </button>
        </div>
      }
    }
  `,
})
export class TaskListComponent {
  readonly taskService = inject(TaskService);
  private readonly toast = inject(ToastService);

  readonly edit = output<Task>();

  protected readonly skeletons = Array(4);

  protected deleteTask(task: Task): void {
    this.taskService.deleteTask(task.id).subscribe({
      next: () => this.toast.show('Task deleted.', 'success'),
      error: () => this.toast.show('Could not delete the task.', 'error'),
    });
  }

  protected toggleStatus(task: Task): void {
    const willComplete = task.status === TaskStatus.Active;
    this.taskService.toggleStatus(task.id).subscribe({
      next: () => {
        this.toast.show(
          willComplete ? 'Task marked as completed.' : 'Task marked as active.',
          'success',
        );
      },
      error: () => this.toast.show('Could not update task status.', 'error'),
    });
  }
}
