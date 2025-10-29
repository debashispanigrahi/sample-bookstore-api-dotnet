import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { User } from '../models/user.model';

interface LoginRequest {
  username: string;
  password: string;
}
interface LoginResult {
  token: string;
  userId?: number;
  role?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private base = environment.apiBaseUrl;
  private tokenKey = 'bookstore_token';
  private userSubject = new BehaviorSubject<User | null>(null);
  public user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {
    const token = this.getToken();
    if (token) {
      // try to load profile
      this.loadProfile().subscribe({
        next: (u) => {},
        error: () => this.logout(),
      });
    }
  }

  login(
    username: string,
    password: string
  ): Observable<ApiResponse<LoginResult>> {
    return this.http
      .post<ApiResponse<LoginResult>>(`${this.base}/api/auth/login`, {
        username,
        password,
      })
      .pipe(
        tap((r) => {
          if (r && (r as any).data && (r as any).data.token) {
            this.setToken((r as any).data.token);
            // load profile
            this.loadProfile().subscribe();
          }
        })
      );
  }

  register(username: string, email: string, password: string, role?: string) {
    const body: any = { username, email, password, role };
    return this.http
      .post<ApiResponse<any>>(`${this.base}/api/auth/register`, body)
      .pipe(
        tap((r) => {
          // registration does not automatically log in — caller decides
        })
      );
  }

  private loadProfile() {
    return this.http
      .get<ApiResponse<User>>(`${this.base}/api/auth/profile`)
      .pipe(
        tap((r) => {
          const user = (r as any).data as User | undefined;
          if (user) this.userSubject.next(user);
        })
      );
  }

  logout() {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.removeItem(this.tokenKey);
    }
    this.userSubject.next(null);
  }

  private setToken(token: string) {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  getToken(): string | null {
    try {
      if (typeof window === 'undefined' || !window.localStorage) return null;
      return localStorage.getItem(this.tokenKey);
    } catch {
      return null;
    }
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    const u = this.userSubject.value;
    return !!u && u.role?.toLowerCase() === role.toLowerCase();
  }
}
