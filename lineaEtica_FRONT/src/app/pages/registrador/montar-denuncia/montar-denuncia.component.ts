import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CrearDenunciaComponent } from '../../denuncia/crear-denuncia/crear-denuncia.component';
import { RegistradorService, BuscarUsuarioResponseDto } from '../registrador.service';
import { AuthService } from '../../../auth/auth.service';

export const CANALES_ORIGEN = ['Correo', 'WhatsApp', 'Presencial', 'Telefono', 'Otro'] as const;

@Component({
  selector: 'app-montar-denuncia',
  standalone: true,
  imports: [CommonModule, FormsModule, CrearDenunciaComponent],
  templateUrl: './montar-denuncia.component.html',
  styleUrl: './montar-denuncia.component.scss'
})
export class MontarDenunciaComponent {
  private svc = inject(RegistradorService);
  private auth = inject(AuthService);

  readonly idEmpresa = this.auth.getCurrentUserEmpresa();

  readonly canales = CANALES_ORIGEN;

  paso: 1 | 2 = 1;
  correo = '';
  canalOrigen = '';
  tipoDenuncia: 'anonimo' | 'no-anonimo' = 'no-anonimo';
  buscando = false;
  errorBusqueda: string | null = null;
  resultadoBusqueda: BuscarUsuarioResponseDto | null = null;

  get idFormularioSeleccionado(): number {
    return this.tipoDenuncia === 'anonimo' ? 1 : 2;
  }

  buscar(): void {
    const correo = this.correo.trim();
    if (!correo) return;

    this.buscando = true;
    this.errorBusqueda = null;
    this.resultadoBusqueda = null;

    this.svc.buscarUsuario(correo).subscribe({
      next: (res) => {
        this.resultadoBusqueda = res;
        this.buscando = false;
      },
      error: () => {
        this.errorBusqueda = 'No se pudo verificar el correo. Intente nuevamente.';
        this.buscando = false;
      }
    });
  }

  continuar(): void {
    if (!this.resultadoBusqueda || !this.canalOrigen) return;
    this.paso = 2;
  }

  volverPaso1(): void {
    this.paso = 1;
    this.resultadoBusqueda = null;
  }
}
