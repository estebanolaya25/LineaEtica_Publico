import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LayoutStateService {
  readonly sidebarOpen = signal(localStorage.getItem('le-sidebar') !== 'closed');

  toggle(): void {
    this.sidebarOpen.update(v => !v);
    localStorage.setItem('le-sidebar', this.sidebarOpen() ? 'open' : 'closed');
  }

  close(): void {
    this.sidebarOpen.set(false);
    localStorage.setItem('le-sidebar', 'closed');
  }

  open(): void {
    this.sidebarOpen.set(true);
    localStorage.setItem('le-sidebar', 'open');
  }
}
