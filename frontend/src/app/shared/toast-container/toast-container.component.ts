import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toast-container" aria-live="polite" aria-atomic="true">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast toast--{{ toast.type }}" role="status">
          <span class="toast__message">{{ toast.message }}</span>
          <button
            type="button"
            class="toast__close"
            aria-label="Dismiss"
            (click)="toastService.dismiss(toast.id)"
          >
            &times;
          </button>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .toast-container {
        position: fixed;
        top: 1rem;
        right: 1rem;
        z-index: 1000;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        max-width: min(360px, calc(100vw - 2rem));
      }

      .toast {
        display: flex;
        align-items: flex-start;
        gap: 0.75rem;
        padding: 0.75rem 1rem;
        border-radius: 8px;
        color: #fff;
        box-shadow: 0 6px 20px rgba(0, 0, 0, 0.18);
        animation: slide-in 0.18s ease-out;
      }

      .toast__message {
        flex: 1;
        font-size: 0.9rem;
        line-height: 1.3;
      }

      .toast__close {
        background: transparent;
        border: 0;
        color: inherit;
        font-size: 1.1rem;
        line-height: 1;
        cursor: pointer;
        opacity: 0.85;
      }

      .toast__close:hover {
        opacity: 1;
      }

      .toast--success {
        background: #16a34a;
      }
      .toast--error {
        background: #dc2626;
      }
      .toast--info {
        background: #2563eb;
      }

      @keyframes slide-in {
        from {
          transform: translateX(12px);
          opacity: 0;
        }
        to {
          transform: translateX(0);
          opacity: 1;
        }
      }
    `
  ]
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);
}
