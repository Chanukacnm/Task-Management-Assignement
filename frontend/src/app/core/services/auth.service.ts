import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthUser } from '../models/user.model';

const TOKEN_KEY = 'tm.auth.token';
const USER_KEY = 'tm.auth.user';

/**
 * Handles authentication using HTTP Basic credentials (no JWT, per requirements).
 * The Base64-encoded "username:password" token is kept in sessionStorage and
 * attached to every API request by the auth interceptor.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly authUrl = `${environment.apiBaseUrl}/auth`;

  private readonly _user = signal<AuthUser | null>(this.readStoredUser());
  private _token: string | null = sessionStorage.getItem(TOKEN_KEY);

  /** The currently authenticated user, or null. */
  readonly currentUser = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  /** The Base64 Basic token used by the interceptor. */
  get token(): string | null {
    return this._token;
  }

  /**
   * Validates the credentials against the API. On success the credentials and
   * user are persisted; on failure they are cleared.
   */
  login(username: string, password: string): Observable<AuthUser> {
    // Set the token first so the interceptor attaches it to this very request.
    this._token = encodeBasicToken(username, password);

    return this.http.post<AuthUser>(`${this.authUrl}/login`, {}).pipe(
      tap({
        next: (user) => this.persistSession(user),
        error: () => this.clearSession()
      })
    );
  }

  logout(): void {
    this.clearSession();
  }

  private persistSession(user: AuthUser): void {
    sessionStorage.setItem(TOKEN_KEY, this._token ?? '');
    sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  private clearSession(): void {
    this._token = null;
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    this._user.set(null);
  }

  private readStoredUser(): AuthUser | null {
    const raw = sessionStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }
}

/** UTF-8 safe Base64 encoding of "username:password". */
function encodeBasicToken(username: string, password: string): string {
  const bytes = new TextEncoder().encode(`${username}:${password}`);
  let binary = '';
  bytes.forEach((b) => (binary += String.fromCharCode(b)));
  return btoa(binary);
}
