import { Routes } from '@angular/router';

export const authRoutes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'RecuperarCuenta',
    loadComponent: () =>
      import('./recuperar-password/recuperar-password.component').then(
        (m) => m.RecuperarPasswordComponent
      ),
  },
  {
    path: 'cambiar-password',
    loadComponent: () =>
      import('./cambiar-password/cambiar-password.component').then(
        (m) => m.CambiarPasswordComponent
      ),
  },
  {
    path: 'registro',
    loadComponent: () =>
      import('./registro/registro.component').then((m) => m.RegistroComponent),
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
];
