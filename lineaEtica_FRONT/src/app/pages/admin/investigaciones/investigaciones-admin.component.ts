import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InvestigadorService, InvestigacionAdminResumenDto } from '../../investigador/investigador.service';
import { AuthService } from '../../../auth/auth.service';

@Component({
  selector: 'app-investigaciones-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './investigaciones-admin.component.html',
  styleUrl: './investigaciones-admin.component.scss'
})
export class InvestigacionesAdminComponent implements OnInit {
  private router = inject(Router);
  private svc    = inject(InvestigadorService);
  private auth   = inject(AuthService);

  readonly esDuenoProceso = this.auth.getCurrentUserRole() === 'Jefe del proceso';

  cargando = true;
  error: string | null = null;
  lista: InvestigacionAdminResumenDto[] = [];
  filtro: 'todas' | 'activas' | 'cerradas' = 'todas';

  get listaFiltrada(): InvestigacionAdminResumenDto[] {
    if (this.filtro === 'activas')  return this.lista.filter(d => d.idEstado === 8);
    if (this.filtro === 'cerradas') return this.lista.filter(d => d.idEstado === 9);
    return this.lista;
  }

  get countActivas():  number { return this.lista.filter(d => d.idEstado === 8).length; }
  get countCerradas(): number { return this.lista.filter(d => d.idEstado === 9).length; }

  ngOnInit(): void {
    this.svc.getInvestigacionesAdmin().subscribe({
      next:  (l) => { this.lista = l; this.cargando = false; },
      error: ()  => { this.error = 'Error al cargar investigaciones.'; this.cargando = false; }
    });
  }

  verDetalle(id: number): void {
    this.router.navigate(['/pages/admin/investigacion', id]);
  }

  getEstadoBadge(idEstado: number): string {
    return idEstado === 8 ? 'badge-investigacion' : 'badge-cerrado';
  }
}
