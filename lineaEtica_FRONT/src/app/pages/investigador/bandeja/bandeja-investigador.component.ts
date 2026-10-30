import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { InvestigadorService, DenunciaInvestigadorResumenDto } from '../investigador.service';

@Component({
  selector: 'app-bandeja-investigador',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './bandeja-investigador.component.html',
  styleUrl: './bandeja-investigador.component.scss'
})
export class BandejaInvestigadorComponent implements OnInit {
  private router = inject(Router);
  private route  = inject(ActivatedRoute);
  private svc    = inject(InvestigadorService);

  denuncias: DenunciaInvestigadorResumenDto[] = [];
  cargando  = true;
  error: string | null = null;
  filtro: 'todas' | 'activas' | 'cerradas' = 'activas';

  get activas(): DenunciaInvestigadorResumenDto[] {
    return this.denuncias.filter(d => d.idEstado === 8);
  }

  get cerradas(): DenunciaInvestigadorResumenDto[] {
    return this.denuncias.filter(d => d.idEstado === 9);
  }

  get lista(): DenunciaInvestigadorResumenDto[] {
    if (this.filtro === 'activas')  return this.activas;
    if (this.filtro === 'cerradas') return this.cerradas;
    return this.denuncias;
  }

  ngOnInit(): void {
    const f = this.route.snapshot.queryParams['filtro'];
    if (f === 'activas' || f === 'cerradas' || f === 'todas') this.filtro = f;

    this.svc.getMisDenuncias().subscribe({
      next:  (d) => { this.denuncias = d; this.cargando = false; },
      error: ()  => { this.error = 'Error al cargar las denuncias.'; this.cargando = false; }
    });
  }

  verDetalle(d: DenunciaInvestigadorResumenDto): void {
    this.router.navigate(['/pages/investigador/seguimiento', d.id]);
  }

  getEstadoBadge(idEstado: number): string {
    return idEstado === 9 ? 'badge-cerrado' : 'badge-investigacion';
  }

  setFiltro(f: 'todas' | 'activas' | 'cerradas'): void {
    this.filtro = f;
  }
}
