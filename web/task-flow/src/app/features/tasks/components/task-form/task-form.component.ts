import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgClass } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import { Task } from '../../../../core/models/task.model';
import { TaskService } from '../../../../core/services/task.service';
import { ToastService } from '../../../../core/services/toast.service';

const TITLE_MAX = 200;
const DESC_MAX = 2000;

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule, NgClass],
  template: `
    <div
      class="fixed inset-0 z-40 bg-black/50 flex items-end sm:items-center justify-center p-0 sm:p-4"
      (click)="onBackdropClick($event)"
    >
      <div
        class="bg-white w-full sm:max-w-lg sm:rounded-xl shadow-xl flex flex-col"
        (click)="$event.stopPropagation()"
      >
        <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <h2 class="text-lg font-semibold text-slate-800">
            {{ task() ? 'Edit Task' : 'New Task' }}
          </h2>
          <button
            (click)="cancelled.emit()"
            class="rounded-lg p-1.5 text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors"
            aria-label="Close"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-5 w-5"
              viewBox="0 0 20 20"
              fill="currentColor"
            >
              <path
                fill-rule="evenodd"
                d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                clip-rule="evenodd"
              />
            </svg>
          </button>
        </div>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="px-6 py-5 space-y-4">
          <!-- Title -->
          <div>
            <label for="title" class="block text-sm font-medium text-slate-700 mb-1">
              Title <span class="text-red-500">*</span>
            </label>
            <input
              id="title"
              type="text"
              formControlName="title"
              [maxlength]="titleMax"
              placeholder="What needs to be done?"
              class="w-full rounded-lg border px-3 py-2 text-sm outline-none transition-colors placeholder:text-slate-400"
              [ngClass]="{
                'text-red-500': titleLength() >= titleMax,
                'text-slate-800': titleLength() < titleMax,
                'border-red-400 focus:border-red-500 focus:ring-1 focus:ring-red-500':
                  titleInvalid(),
                'border-slate-300 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500':
                  !titleInvalid(),
              }"
              autocomplete="off"
            />
            <div class="mt-1 flex items-start justify-between gap-2">
              <div class="text-xs text-red-500 min-h-[1rem]">
                @if (titleInvalid()) {
                  @if (form.controls['title'].errors?.['required']) {
                    Title is required.
                  } @else if (form.controls['title'].errors?.['minlength']) {
                    Title must be at least 3 characters.
                  } @else if (form.controls['title'].errors?.['maxlength']) {
                    Title cannot exceed {{ titleMax }} characters.
                  }
                }
              </div>
              <span
                class="shrink-0 text-xs tabular-nums"
                [ngClass]="{
                  'text-red-500 font-medium': titleLength() >= titleMax,
                  'text-slate-400': titleLength() < titleMax,
                }"
              >
                {{ titleLength() }}/{{ titleMax }}
              </span>
            </div>
          </div>

          <!-- Description -->
          <div>
            <label for="description" class="block text-sm font-medium text-slate-700 mb-1">
              Description
            </label>
            <textarea
              id="description"
              formControlName="description"
              rows="3"
              [maxlength]="descMax"
              placeholder="Add details (optional)"
              class="w-full rounded-lg border px-3 py-2 text-sm outline-none transition-colors placeholder:text-slate-400 resize-none"
              [ngClass]="{
                'text-red-500': descLength() >= descMax,
                'text-slate-800': descLength() < descMax,
                'border-red-400 focus:border-red-500 focus:ring-1 focus:ring-red-500':
                  descriptionInvalid(),
                'border-slate-300 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500':
                  !descriptionInvalid(),
              }"
            ></textarea>
            <div class="mt-1 flex items-start justify-between gap-2">
              <div class="text-xs text-red-500 min-h-[1rem]">
                @if (descriptionInvalid()) {
                  Description cannot exceed {{ descMax }} characters.
                }
              </div>
              <span
                class="shrink-0 text-xs tabular-nums"
                [ngClass]="{
                  'text-red-500 font-medium': descLength() >= descMax,
                  'text-slate-400': descLength() < descMax,
                }"
              >
                {{ descLength() }}/{{ descMax }}
              </span>
            </div>
          </div>

          @if (submitError()) {
            <div class="rounded-lg bg-red-50 border border-red-200 px-3 py-2 text-sm text-red-600">
              {{ submitError() }}
            </div>
          }

          <div class="flex gap-3 justify-end pt-1">
            <button
              type="button"
              (click)="cancelled.emit()"
              class="px-4 py-2 text-sm font-medium text-slate-700 bg-slate-100 rounded-lg hover:bg-slate-200 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              [disabled]="submitting() || form.invalid"
              class="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {{ submitting() ? 'Saving...' : task() ? 'Save Changes' : 'Create Task' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
})
export class TaskFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly toast = inject(ToastService);

  readonly task = input<Task | null>(null);
  readonly submitted = output<void>();
  readonly cancelled = output<void>();

  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly titleMax = TITLE_MAX;
  readonly descMax = DESC_MAX;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(TITLE_MAX)]],
    description: ['', [Validators.maxLength(DESC_MAX)]],
  });

  ngOnInit(): void {
    const task = this.task();
    if (task) {
      this.form.patchValue({ title: task.title, description: task.description });
    }
  }

  protected titleLength(): number {
    return this.form.controls['title'].value.length;
  }

  protected descLength(): number {
    return this.form.controls['description'].value.length;
  }

  protected titleInvalid(): boolean {
    const control = this.form.controls['title'];
    return control.invalid && control.touched;
  }

  protected descriptionInvalid(): boolean {
    const control = this.form.controls['description'];
    return control.invalid && control.touched;
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.cancelled.emit();
    }
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { title, description } = this.form.getRawValue();
    const editingTask = this.task();

    this.submitting.set(true);
    this.submitError.set(null);

    const request$ = editingTask
      ? this.taskService.updateTask(editingTask.id, { title, description })
      : this.taskService.createTask({ title, description });

    request$.subscribe({
      next: () => {
        this.toast.show(
          editingTask ? 'Task updated successfully.' : 'Task created successfully.',
          'success',
        );
        this.submitted.emit();
      },
      error: (err: HttpErrorResponse) => {
        const detail = err.error?.detail ?? null;
        this.submitError.set(detail ?? 'Failed to save the task. Please try again.');
        this.submitting.set(false);
      },
    });
  }
}
