import { Component, inject } from '@angular/core';
import { NgClass } from '@angular/common';

import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  imports: [NgClass],
  template: `
    <div class="fixed bottom-4 right-4 z-50 flex flex-col gap-2" aria-live="polite">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          [ngClass]="{
            'bg-green-600': toast.type === 'success',
            'bg-red-600': toast.type === 'error',
            'bg-slate-700': toast.type === 'info',
          }"
          class="flex items-center justify-between gap-4 rounded-lg px-4 py-3 text-white shadow-lg min-w-64 max-w-sm"
        >
          <span class="text-sm">{{ toast.message }}</span>
          <button
            (click)="toastService.dismiss(toast.id)"
            class="shrink-0 text-white/70 hover:text-white transition-colors"
            aria-label="Dismiss notification"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-4 w-4"
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
      }
    </div>
  `,
})
export class ToastComponent {
  readonly toastService = inject(ToastService);
}
