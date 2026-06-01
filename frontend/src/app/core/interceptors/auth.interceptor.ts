import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

/**
 * Attaches the HTTP Basic Authorization header to API requests and, on a 401
 * response, signs the user out and redirects to the login page.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const toast = inject(ToastService);

  const isApiRequest = req.url.startsWith(environment.apiBaseUrl);
  const token = auth.token;

  const authReq =
    isApiRequest && token
      ? req.clone({ setHeaders: { Authorization: `Basic ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      const isLoginRequest = req.url.endsWith('/auth/login');

      if (error.status === 401 && !isLoginRequest) {
        auth.logout();
        toast.error('Your session is no longer valid. Please sign in again.');
        void router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );
};
