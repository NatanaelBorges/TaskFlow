import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 0) {
        toast.show('Cannot reach the server. Check your connection.', 'error');
      } else if (err.status >= 500) {
        toast.show('An unexpected server error occurred. Please try again.', 'error');
      }
      // 4xx errors are handled by the individual callers for contextual messages
      return throwError(() => err);
    }),
  );
};
