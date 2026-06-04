import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  template: `
    <div
      class="fixed inset-0 z-40 bg-black/50 flex items-center justify-center p-4"
      (click)="cancelled.emit()"
    >
      <div
        class="bg-white rounded-xl shadow-xl w-full max-w-sm p-6"
        (click)="$event.stopPropagation()"
      >
        <h2 class="text-lg font-semibold text-slate-800">{{ title() }}</h2>
        <p class="mt-2 text-sm text-slate-500">{{ message() }}</p>
        <div class="mt-6 flex gap-3 justify-end">
          <button
            (click)="cancelled.emit()"
            class="px-4 py-2 text-sm font-medium text-slate-700 bg-slate-100 rounded-lg hover:bg-slate-200 transition-colors"
          >
            Cancel
          </button>
          <button
            (click)="confirmed.emit()"
            class="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
          >
            {{ confirmLabel() }}
          </button>
        </div>
      </div>
    </div>
  `,
})
export class ConfirmDialogComponent {
  readonly title = input('Confirm');
  readonly message = input('Are you sure?');
  readonly confirmLabel = input('Confirm');

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}
