import { Component, input, output } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';

import { Task, TaskStatus } from '../../../../core/models/task.model';

@Component({
  selector: 'app-task-card',
  imports: [DatePipe, NgClass],
  template: `
    <div
      class="group rounded-xl border border-l-4 bg-white p-4 shadow-sm transition-all hover:shadow-md"
      [ngClass]="{
        'border-slate-100 border-l-indigo-400': task().status === taskStatus.Active,
        'border-green-100 border-l-green-400 bg-green-50/50':
          task().status === taskStatus.Completed,
      }"
    >
      <div class="flex items-start gap-3">
        <button
          class="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded border-2 transition-all duration-150"
          [ngClass]="{
            'border-slate-300 hover:border-indigo-400': task().status !== taskStatus.Completed,
            'border-green-500 bg-green-500': task().status === taskStatus.Completed,
          }"
          [attr.aria-label]="
            task().status === taskStatus.Completed ? 'Mark as active' : 'Mark as completed'
          "
          [disabled]="toggling()"
          (click)="toggleStatus.emit(task())"
        >
          @if (task().status === taskStatus.Completed) {
            <svg class="h-3 w-3 text-white" viewBox="0 0 12 12" fill="currentColor">
              <path
                d="M10.28 1.28a1 1 0 0 1 0 1.44l-5.5 5.5a1 1 0 0 1-1.44 0l-2.5-2.5a1 1 0 1 1 1.44-1.44L4.5 6.06l4.78-4.78a1 1 0 0 1 1.44 0Z"
              />
            </svg>
          }
        </button>

        <div class="min-w-0 flex-1">
          <div class="flex items-start justify-between gap-2">
            <h3
              class="font-medium leading-snug transition-all duration-150"
              [ngClass]="{
                'text-slate-800': task().status === taskStatus.Active,
                'line-through text-slate-400': task().status === taskStatus.Completed,
              }"
            >
              {{ task().title }}
            </h3>
            <span
              class="shrink-0 rounded-full px-2 py-0.5 text-xs font-medium"
              [ngClass]="{
                'bg-indigo-100 text-indigo-700': task().status === taskStatus.Active,
                'bg-green-100 text-green-700': task().status === taskStatus.Completed,
              }"
            >
              {{ task().status === taskStatus.Active ? 'Active' : 'Completed' }}
            </span>
          </div>

          @if (task().description) {
            <p
              class="mt-1 text-sm line-clamp-2 transition-colors duration-150"
              [ngClass]="{
                'text-slate-500': task().status === taskStatus.Active,
                'text-slate-400': task().status === taskStatus.Completed,
              }"
            >
              {{ task().description }}
            </p>
          }

          <div class="mt-3 flex items-center justify-between">
            <div class="text-xs text-slate-400 space-x-2">
              <span>Created {{ task().createdAtUtc | date: 'MMM d, y' }}</span>
              @if (task().updatedAtUtc) {
                <span>· Updated {{ task().updatedAtUtc | date: 'MMM d, y' }}</span>
              }
            </div>

            <div class="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
              <button
                (click)="edit.emit(task())"
                class="rounded-lg px-2.5 py-1 text-xs font-medium text-slate-600 hover:bg-slate-100 transition-colors"
                aria-label="Edit task"
              >
                Edit
              </button>
              <button
                (click)="delete.emit(task())"
                class="rounded-lg px-2.5 py-1 text-xs font-medium text-red-500 hover:bg-red-50 transition-colors"
                aria-label="Delete task"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class TaskCardComponent {
  readonly task = input.required<Task>();
  readonly toggling = input(false);

  readonly edit = output<Task>();
  readonly delete = output<Task>();
  readonly toggleStatus = output<Task>();

  protected readonly taskStatus = TaskStatus;
}
