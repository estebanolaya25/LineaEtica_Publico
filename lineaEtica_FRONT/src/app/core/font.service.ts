import { Injectable, PLATFORM_ID, effect, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export interface FontOption { id: string; label: string; stack: string; google?: string; }

export const AVAILABLE_FONTS: FontOption[] = [
  { id: 'system',    label: 'Sistema',    stack: "system-ui, -apple-system, sans-serif" },
  { id: 'inter',     label: 'Inter',      stack: "'Inter', sans-serif",      google: 'Inter:wght@400;500;600;700' },
  { id: 'roboto',    label: 'Roboto',     stack: "'Roboto', sans-serif",     google: 'Roboto:wght@400;500;700' },
  { id: 'open-sans', label: 'Open Sans',  stack: "'Open Sans', sans-serif",  google: 'Open+Sans:wght@400;500;600;700' },
  { id: 'poppins',   label: 'Poppins',    stack: "'Poppins', sans-serif",    google: 'Poppins:wght@400;500;600;700' },
];

@Injectable({ providedIn: 'root' })
export class FontService {
  private platformId = inject(PLATFORM_ID);
  readonly currentFont = signal<FontOption>(AVAILABLE_FONTS[0]);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem('le-font');
      const found = AVAILABLE_FONTS.find(f => f.id === saved);
      if (found) this.currentFont.set(found);
    }

    effect(() => {
      if (!isPlatformBrowser(this.platformId)) return;
      const font = this.currentFont();
      document.documentElement.style.setProperty('--app-font', font.stack);
      localStorage.setItem('le-font', font.id);
      if (font.google) this.loadGoogleFont(font);
    });
  }

  setFont(font: FontOption): void { this.currentFont.set(font); }

  private loadGoogleFont(font: FontOption): void {
    const id = `gf-${font.id}`;
    if (document.getElementById(id)) return;
    const link = document.createElement('link');
    link.id = id; link.rel = 'stylesheet';
    link.href = `https://fonts.googleapis.com/css2?family=${font.google}&display=swap`;
    document.head.appendChild(link);
  }
}
