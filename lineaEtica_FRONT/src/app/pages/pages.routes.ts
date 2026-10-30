import { Routes } from '@angular/router';
import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';
import { roleGuard } from '../auth/role.guard';
import { LayoutComponent } from './layout/layout.component';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="inicio-wrapper">

      <div class="inicio-header">
        <h3 class="mb-1">Bienvenido, {{ nombreUsuario }}</h3>
        <p class="text-muted mb-0">
          {{ esAdmin ? 'Consultor · Canal Ético Corporativo' : 'Canal Ético Corporativo' }}
        </p>
      </div>

      <div class="inicio-body">
        @if (esAdmin) {
          <div class="row g-4">
            <div class="col-md-6">
              <div class="denuncia-card card h-100" (click)="irBandeja()" role="button">
                <div class="card-body text-center p-5">
                  <div class="icono-wrapper mb-3">
                    <i class="bi bi-inbox-fill"></i>
                  </div>
                  <h5 class="card-title">Bandeja de Denuncias</h5>
                  <p class="card-text text-muted">
                    Revise, valide y gestione todas las denuncias enviadas al sistema.
                  </p>
                  <span class="btn btn-primary mt-2">
                    <i class="bi bi-eye me-2"></i>Ir a la bandeja
                  </span>
                </div>
              </div>
            </div>
          </div>
        } @else {
          <h5 class="mb-4 text-secondary">¿Qué desea reportar hoy?</h5>
          <div class="row g-4">
            <div class="col-md-6">
              <div class="denuncia-card card h-100 anonima" (click)="irA('anonimo')" role="button">
                <div class="card-body text-center p-5">
                  <div class="icono-wrapper mb-3">
                    <i class="bi bi-incognito"></i>
                  </div>
                  <h5 class="card-title">Denuncia Anónima</h5>
                  <p class="card-text text-muted">
                    Su identidad no quedará registrada en el reporte. Puede
                    reportar situaciones de manera completamente confidencial.
                  </p>
                  <span class="btn btn-primary mt-2">
                    <i class="bi bi-plus-circle me-2"></i>Crear reporte anónimo
                  </span>
                </div>
              </div>
            </div>

            <div class="col-md-6">
              <div class="denuncia-card card h-100 no-anonima" (click)="irA('no-anonimo')" role="button">
                <div class="card-body text-center p-5">
                  <div class="icono-wrapper mb-3">
                    <i class="bi bi-person-lines-fill"></i>
                  </div>
                  <h5 class="card-title">Denuncia No Anónima</h5>
                  <p class="card-text text-muted">
                    Sus datos de contacto quedarán registrados. Esto permite un
                    seguimiento más efectivo de su reporte.
                  </p>
                  <span class="btn btn-outline-primary mt-2">
                    <i class="bi bi-plus-circle me-2"></i>Crear reporte identificado
                  </span>
                </div>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .inicio-wrapper {
      max-width: 900px;
      margin: 0 auto;
    }
    .inicio-header {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      margin-bottom: 2rem;
      box-shadow: 0 1px 4px rgba(0,0,0,0.08);
      border-left: 4px solid var(--accent, #486581);
    }
    .inicio-body { padding: 0 0.25rem; }
    .denuncia-card {
      border-radius: 12px;
      border: 2px solid #e2e8f0;
      cursor: pointer;
      transition: all 0.2s ease;
      background: white;
      &:hover {
        transform: translateY(-3px);
        box-shadow: 0 8px 24px rgba(var(--accent-rgb, 72 101 129) / 0.18);
        border-color: var(--accent, #486581);
      }
    }
    .icono-wrapper {
      width: 72px; height: 72px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--accent, #486581), var(--accent-dark, #334e68));
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto;
      i { font-size: 2rem; color: white; }
    }
  `]
})
class InicioComponent implements OnInit {
  private router = inject(Router);
  private auth   = inject(AuthService);
  nombreUsuario   = this.auth.getCurrentUserName();
  rol             = this.auth.getCurrentUserRole();
  esAdmin         = this.rol === 'Consultor';
  esInvestigador  = this.rol === 'Investigador';
  esDuenoProceso  = this.rol === 'Jefe del proceso';
  esRegistrador   = this.rol === 'Registrador';

  ngOnInit(): void {
    if (this.esAdmin) {
      this.router.navigate(['/pages/admin/dashboard'], { replaceUrl: true });
    } else if (this.esInvestigador) {
      this.router.navigate(['/pages/investigador/dashboard'], { replaceUrl: true });
    } else if (this.esDuenoProceso) {
      this.router.navigate(['/pages/admin/dashboard'], { replaceUrl: true });
    } else if (this.esRegistrador) {
      this.router.navigate(['/pages/registrador/dashboard'], { replaceUrl: true });
    }
  }

  irA(tipo: string): void { this.router.navigate(['/pages/denuncia/crear', tipo]); }
  irBandeja(): void        { this.router.navigate(['/pages/admin/bandeja']); }
}

export const pagesRoutes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: 'inicio', component: InicioComponent },
      {
        path: 'denuncia/crear/:tipo',
        loadComponent: () => import('./denuncia/crear-denuncia/crear-denuncia.component')
          .then(m => m.CrearDenunciaComponent),
        canActivate: [roleGuard],
        data: { roles: ['Denunciante'] }
      },
      {
        path: 'mis-denuncias',
        loadComponent: () => import('./denuncia/mis-denuncias/mis-denuncias.component')
          .then(m => m.MisDenunciasComponent),
        canActivate: [roleGuard],
        data: { roles: ['Denunciante'] }
      },
      {
        path: 'admin/dashboard',
        loadComponent: () => import('./admin/dashboard/admin-dashboard.component')
          .then(m => m.AdminDashboardComponent),
        canActivate: [roleGuard],
        data: { roles: ['Consultor', 'Jefe del proceso'] }
      },
      {
        path: 'admin/bandeja',
        loadComponent: () => import('./admin/bandeja/bandeja.component')
          .then(m => m.BandejaComponent),
        canActivate: [roleGuard],
        data: { roles: ['Consultor', 'Jefe del proceso'] }
      },
      {
        path: 'admin/revision/:id',
        loadComponent: () => import('./admin/revision/revision-denuncia.component')
          .then(m => m.RevisionDenunciaComponent),
        canActivate: [roleGuard],
        data: { roles: ['Consultor', 'Jefe del proceso'] }
      },
      {
        path: 'admin/investigaciones',
        loadComponent: () => import('./admin/investigaciones/investigaciones-admin.component')
          .then(m => m.InvestigacionesAdminComponent),
        canActivate: [roleGuard],
        data: { roles: ['Consultor', 'Jefe del proceso'] }
      },
      {
        path: 'admin/investigacion/:id',
        loadComponent: () => import('./admin/investigacion-detalle/investigacion-detalle-admin.component')
          .then(m => m.InvestigacionDetalleAdminComponent),
        canActivate: [roleGuard],
        data: { roles: ['Consultor', 'Jefe del proceso'] }
      },
      {
        path: 'investigador/dashboard',
        loadComponent: () => import('./investigador/dashboard/investigador-dashboard.component')
          .then(m => m.InvestigadorDashboardComponent),
        canActivate: [roleGuard],
        data: { roles: ['Investigador'] }
      },
      {
        path: 'investigador/bandeja',
        loadComponent: () => import('./investigador/bandeja/bandeja-investigador.component')
          .then(m => m.BandejaInvestigadorComponent),
        canActivate: [roleGuard],
        data: { roles: ['Investigador'] }
      },
      {
        path: 'investigador/seguimiento/:id',
        loadComponent: () => import('./investigador/seguimiento/seguimiento-denuncia.component')
          .then(m => m.SeguimientoDenunciaComponent),
        canActivate: [roleGuard],
        data: { roles: ['Investigador'] }
      },
      {
        path: 'admin/usuarios',
        loadComponent: () => import('./admin/usuarios/gestion-usuarios.component')
          .then(m => m.GestionUsuariosComponent),
        canActivate: [roleGuard],
        data: { roles: ['Consultor', 'Jefe del proceso'] }
      },
      {
        path: 'admin/manual',
        loadComponent: () => import('./admin/manual/manual-usuario.component')
          .then(m => m.ManualUsuarioComponent),
        canActivate: [roleGuard],
        data: { roles: ['Jefe del proceso'] }
      },
      {
        path: 'registrador/dashboard',
        loadComponent: () => import('./registrador/dashboard/registrador-dashboard.component')
          .then(m => m.RegistradorDashboardComponent),
        canActivate: [roleGuard],
        data: { roles: ['Registrador'] }
      },
      {
        path: 'registrador/montar-denuncia',
        loadComponent: () => import('./registrador/montar-denuncia/montar-denuncia.component')
          .then(m => m.MontarDenunciaComponent),
        canActivate: [roleGuard],
        data: { roles: ['Registrador'] }
      },
      { path: '', redirectTo: 'inicio', pathMatch: 'full' },
    ]
  }
];
