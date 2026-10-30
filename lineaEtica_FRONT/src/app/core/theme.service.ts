import { Injectable, PLATFORM_ID, effect, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export interface ColorTheme {
  id: string;
  label: string;
  accent: string;
  dark: string;
  rgb: string;
}

export const COLOR_THEMES: ColorTheme[] = [
  { id: 'slate',  label: 'Azul Gris', accent: '#486581', dark: '#334e68', rgb: '72,101,129'  },
  { id: 'blue',   label: 'Azul',      accent: '#2B6CB0', dark: '#1e4e8c', rgb: '43,108,176'  },
  { id: 'teal',   label: 'Teal',      accent: '#2C7A7B', dark: '#1a5e5e', rgb: '44,122,123'  },
  { id: 'green',  label: 'Verde',     accent: '#276749', dark: '#1a4a33', rgb: '39,103,73'   },
  { id: 'orange', label: 'Naranja',   accent: '#C05621', dark: '#9c3d15', rgb: '192,86,33'   },
  { id: 'purple', label: 'Morado',    accent: '#6B46C1', dark: '#553c9a', rgb: '107,70,193'  },
  { id: 'red',    label: 'Rojo',      accent: '#C53030', dark: '#9b2c2c', rgb: '197,48,48'   },
];

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private platformId = inject(PLATFORM_ID);

  readonly isDarkMode  = signal(false);
  readonly colorTheme  = signal<ColorTheme>(COLOR_THEMES[0]);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.isDarkMode.set(localStorage.getItem('le-theme') === 'dark');
      const savedId = localStorage.getItem('le-color');
      const saved = COLOR_THEMES.find(t => t.id === savedId);
      if (saved) this.colorTheme.set(saved);
    }

    effect(() => {
      if (!isPlatformBrowser(this.platformId)) return;
      document.body.classList.toggle('dark-mode', this.isDarkMode());
      localStorage.setItem('le-theme', this.isDarkMode() ? 'dark' : 'light');
    });

    effect(() => {
      if (!isPlatformBrowser(this.platformId)) return;
      this.applyColor(this.colorTheme());
      localStorage.setItem('le-color', this.colorTheme().id);
    });
  }

  toggleDark(): void { this.isDarkMode.update(v => !v); }

  setColor(theme: ColorTheme): void { this.colorTheme.set(theme); }

  private applyColor(t: ColorTheme): void {
    const r = document.documentElement;
    r.style.setProperty('--accent',     t.accent);
    r.style.setProperty('--accent-dark', t.dark);
    r.style.setProperty('--accent-rgb',  t.rgb);
  }
}
