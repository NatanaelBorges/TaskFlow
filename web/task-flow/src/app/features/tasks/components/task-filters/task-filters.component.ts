import { Component, input, output } from '@angular/core';
import { NgClass } from '@angular/common';

import { TaskFilter } from '../../../../core/models/task.model';

interface FilterOption {
  value: TaskFilter;
  label: string;
}

@Component({
  selector: 'app-task-filters',
  imports: [NgClass],
  template: `
    <div class="flex gap-1 rounded-lg bg-slate-100 p-1" role="tablist" aria-label="Filter tasks">
      @for (option of options; track option.value) {
        <button
          role="tab"
          [attr.aria-selected]="activeFilter() === option.value"
          [ngClass]="{
            'bg-white text-slate-800 shadow-sm': activeFilter() === option.value,
            'text-slate-500 hover:text-slate-700': activeFilter() !== option.value,
          }"
          class="rounded-md px-4 py-1.5 text-sm font-medium transition-all"
          (click)="filterChanged.emit(option.value)"
        >
          {{ option.label }}
        </button>
      }
    </div>
  `,
})
export class TaskFiltersComponent {
  readonly activeFilter = input.required<TaskFilter>();
  readonly filterChanged = output<TaskFilter>();

  protected readonly options: FilterOption[] = [
    { value: 'all', label: 'All' },
    { value: 'active', label: 'Active' },
    { value: 'completed', label: 'Completed' },
  ];
}
