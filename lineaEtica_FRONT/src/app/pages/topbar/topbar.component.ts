import { Component, HostListener, OnInit, OnDestroy, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { LayoutStateService } from '../layout/layout-state.service';
import { ThemeService, COLOR_THEMES, ColorTheme } from '../../core/theme.service';
import { FontService, AVAILABLE_FONTS, FontOption } from '../../core/font.service';
import { NotificacionesService, Notificacion } from '../../core/notificaciones.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent implements OnInit, OnDestroy {
  readonly layoutState = inject(LayoutStateService);
  readonly theme       = inject(ThemeService);
  readonly fonts       = inject(FontService);
  readonly notif       = inject(NotificacionesService);
  private auth         = inject(AuthService);
  private router       = inject(Router);

  readonly colorThemes = COLOR_THEMES;
  readonly fontOptions = AVAILABLE_FONTS;

  colorPickerOpen = false;
  fontPickerOpen  = false;
  notifOpen       = false;

  ngOnInit():    void { this.notif.startPolling(); }
  ngOnDestroy(): void { this.notif.stopPolling(); }

  @HostListener('document:click')
  closeAll(): void {
    this.colorPickerOpen = false;
    this.fontPickerOpen  = false;
    this.notifOpen       = false;
  }

  stopProp(e: Event): void { e.stopPropagation(); }

  openColor(e: Event): void {
    e.stopPropagation();
    this.colorPickerOpen = !this.colorPickerOpen;
    this.fontPickerOpen  = false;
    this.notifOpen       = false;
  }

  openFont(e: Event): void {
    e.stopPropagation();
    this.fontPickerOpen  = !this.fontPickerOpen;
    this.colorPickerOpen = false;
    this.notifOpen       = false;
  }

  toggleNotif(e: Event): void {
    e.stopPropagation();
    this.notifOpen       = !this.notifOpen;
    this.colorPickerOpen = false;
    this.fontPickerOpen  = false;
  }

  marcarTodas(e: Event): void {
    e.stopPropagation();
    this.notif.marcarTodasLeidas();
  }

  pickColor(t: ColorTheme): void { this.theme.setColor(t); this.colorPickerOpen = false; }
  pickFont(f: FontOption): void  { this.fonts.setFont(f);  this.fontPickerOpen  = false; }

  abrirDenuncia(e: Event, n: Notificacion): void {
    e.stopPropagation();
    if (!n.idFormularioUsuario) {
      this.notif.marcarLeida(n.id);
      return;
    }

    this.notif.marcarLeida(n.id);
    this.notifOpen = false;

    const rol = this.auth.getCurrentUserRole();
    if (rol === 'Consultor' || rol === 'Jefe del proceso')
      this.router.navigate(['/pages/admin/revision', n.idFormularioUsuario]);
    else if (rol === 'Investigador')
      this.router.navigate(['/pages/investigador/seguimiento', n.idFormularioUsuario]);
    else if (rol === 'Registrador')
      this.router.navigate(['/pages/registrador/dashboard']);
    else
      this.router.navigate(['/pages/mis-denuncias']);
  }

  cambiarPassword(): void { this.router.navigate(['/auth/cambiar-password']); }

  logout(): void { this.auth.logout(); }
}
