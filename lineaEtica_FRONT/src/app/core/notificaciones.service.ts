import { Injectable, OnDestroy, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from './config.service';

export interface Notificacion {
  id:                  number;
  mensaje:             string;
  tipo:                'info' | 'warning' | 'success' | 'danger';
  leida:               boolean;
  fecha:               string;
  idFormularioUsuario: number | null;
}

@Injectable({ providedIn: 'root' })
export class NotificacionesService implements OnDestroy {
  private http   = inject(HttpClient);
  private config = inject(ConfigService);

  readonly items    = signal<Notificacion[]>([]);
  readonly noLeidas = computed(() => this.items().length);

  private timer: ReturnType<typeof setInterval> | null = null;
  private readonly INTERVALO_MS = 30_000;

  startPolling(): void {
    if (this.timer) return;
    this.poll();
    this.timer = setInterval(() => this.poll(), this.INTERVALO_MS);
  }

  stopPolling(): void {
    if (this.timer) { clearInterval(this.timer); this.timer = null; }
  }

  private poll(): void {
    this.http
      .get<Notificacion[]>(`${this.config.apiUrl}/notificaciones`)
      .subscribe({ next: data => this.items.set(data), error: () => {} });
  }

  marcarLeida(id: number): void {
    this.http
      .put(`${this.config.apiUrl}/notificaciones/${id}/marcar-leida`, {})
      .subscribe({
        next:  () => this.items.update(list => list.filter(n => n.id !== id)),
        error: () => {}
      });
  }

  marcarTodasLeidas(): void {
    if (this.items().length === 0) return;
    this.http
      .put(`${this.config.apiUrl}/notificaciones/marcar-leidas`, {})
      .subscribe({ next: () => this.items.set([]), error: () => {} });
  }

  ngOnDestroy(): void { this.stopPolling(); }
}
