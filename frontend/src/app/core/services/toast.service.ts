import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

/**
 * Lightweight toast/notification service. Components read the `toasts` signal to
 * render messages; messages auto-dismiss after a timeout.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  private nextId = 0;

  success(message: string): void {
    this.show(message, 'success', 4000);
  }

  error(message: string): void {
    this.show(message, 'error', 6000);
  }

  info(message: string): void {
    this.show(message, 'info', 4000);
  }

  show(message: string, type: ToastType = 'info', timeoutMs = 4000): void {
    const id = ++this.nextId;
    this._toasts.update((toasts) => [...toasts, { id, type, message }]);

    if (timeoutMs > 0) {
      setTimeout(() => this.dismiss(id), timeoutMs);
    }
  }

  dismiss(id: number): void {
    this._toasts.update((toasts) => toasts.filter((toast) => toast.id !== id));
  }
}
