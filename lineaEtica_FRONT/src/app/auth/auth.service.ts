import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError, timeout, TimeoutError } from 'rxjs';
import { Router } from '@angular/router';
import { ConfigService } from '../core/config.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'auth_token';
  private http = inject(HttpClient);
  private router = inject(Router);
  private configService = inject(ConfigService);
  private get apiUrl(): string {
    return `${this.configService.apiUrl}/auth`;
  }

  login(email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, { email, password }).pipe(
      timeout(5000),
      catchError((error) => this.handleError(error))
    );
  }

  recuperacion(correoElectronico: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Recuperacion`, { correoElectronico });
  }

  registro(dto: { nombre: string; correo: string; clave: string; confirmarClave: string }): Observable<{ exito: boolean; mensaje: string }> {
    return this.http.post<{ exito: boolean; mensaje: string }>(`${this.apiUrl}/registro`, dto);
  }

  getCurrentUser(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/current`);
  }

  updatePassword(newpassword: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/passwordUpdate`, { newpassword });
  }

  mustUpdatePassword(): boolean {
    return localStorage.getItem('must_update_password') === 'true';
  }

  setMustUpdatePassword(value: boolean): void {
    if (value) {
      localStorage.setItem('must_update_password', 'true');
    } else {
      localStorage.removeItem('must_update_password');
    }
  }

  getToken(): string | null {
    const tokenString = localStorage.getItem(this.tokenKey);
    if (!tokenString) return null;
    try {
      const parsed = JSON.parse(tokenString);
      return parsed.token?.access_token ?? parsed.access_token ?? null;
    } catch {
      return null;
    }
  }

  getRefreshToken(): string | null {
    const tokenString = localStorage.getItem(this.tokenKey);
    if (!tokenString) return null;
    try {
      const parsed = JSON.parse(tokenString);
      return parsed.token?.refresh_token ?? parsed.refresh_token ?? null;
    } catch {
      return null;
    }
  }

  refreshAccessToken(refreshToken: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/refresh`, { refreshToken });
  }

  setToken(token: any): void {
    localStorage.setItem(this.tokenKey, JSON.stringify(token));
  }

  clearToken(): void {
    localStorage.removeItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    return !this.isTokenExpired(token);
  }

  logout(): void {
    this.clearToken();
    this.setMustUpdatePassword(false);
    this.router.navigate(['/auth/login']);
  }

  getCurrentUserRole(): string {
    const token = this.getToken();
    if (!token) return '';
    try {
      const payload = this.decodeJwtPayload(token);
      return (
        payload['role'] ??
        payload['roles']?.[0] ??
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
        ''
      );
    } catch {
      return '';
    }
  }

  getCurrentUserEmpresa(): number | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = this.decodeJwtPayload(token);
      const valor = payload['idEmpresa'];
      return valor != null ? Number(valor) : null;
    } catch {
      return null;
    }
  }

  getCurrentUserEmpresaNombre(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = this.decodeJwtPayload(token);
      return payload['nombreEmpresa'] ?? null;
    } catch {
      return null;
    }
  }

  getCurrentUserName(): string {
    const token = this.getToken();
    if (!token) return 'Usuario';
    try {
      const payload = this.decodeJwtPayload(token);
      return (
        payload['name'] ??
        payload['unique_name'] ??
        payload['sub'] ??
        'Usuario'
      );
    } catch {
      return 'Usuario';
    }
  }

  private decodeJwtPayload(token: string): any {
    const base64Url = token.split('.')[1];
    const base64    = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const padded    = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=');
    const bytes     = Uint8Array.from(atob(padded), c => c.charCodeAt(0));
    return JSON.parse(new TextDecoder().decode(bytes));
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.decodeJwtPayload(token);
      return payload.exp < Date.now() / 1000;
    } catch {
      return true;
    }
  }

  private handleError(error: HttpErrorResponse | TimeoutError): Observable<never> {
    let errorMessage: string;
    if (error instanceof TimeoutError) {
      errorMessage = 'El servicio no respondió dentro del tiempo esperado.';
    } else if ((error as HttpErrorResponse).error instanceof ErrorEvent) {
      errorMessage = `Error: ${(error as HttpErrorResponse).error.message}`;
    } else {
      errorMessage = `Código de error: ${(error as HttpErrorResponse).status}`;
    }
    return throwError(() => errorMessage);
  }
}
