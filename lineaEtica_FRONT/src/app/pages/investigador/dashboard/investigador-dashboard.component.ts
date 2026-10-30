import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { InvestigadorService, DenunciaInvestigadorResumenDto } from '../investigador.service';
import { AuthService } from '../../../auth/auth.service';

interface DonutSegment {
  dasharray: string;
  dashoffset: number;
  color: string;
  estado: string;
  cantidad: number;
  pct: number;
}

@Component({
  selector: 'app-investigador-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './investigador-dashboard.component.html',
  styleUrl: './investigador-dashboard.component.scss'
})
export class InvestigadorDashboardComponent implements OnInit {
  private svc    = inject(InvestigadorService);
  private router = inject(Router);
  private auth   = inject(AuthService);

  readonly empresaNombre = this.auth.getCurrentUserEmpresaNombre();

  cargando = true;
  error: string | null = null;
  denuncias: DenunciaInvestigadorResumenDto[] = [];
  donutSegments: DonutSegment[] = [];

  private readonly STATE_COLORS: Record<number, string> = {
    8:  '#1a7fc1',
    10: '#f59e0b',
    9:  '#198754',
  };

  private readonly STATE_LABELS: Record<number, string> = {
    8:  'En investigación',
    10: 'Pendiente respuesta',
    9:  'Cerrado',
  };

  ngOnInit(): void {
    this.svc.getMisDenuncias().subscribe({
      next: (d) => {
        this.denuncias = d;
        this.buildDonut(d);
        this.cargando = false;
      },
      error: () => {
        this.error = 'No se pudieron cargar sus investigaciones.';
        this.cargando = false;
      }
    });
  }

  get total(): number { return this.denuncias.length; }

  get enInvestigacion(): number {
    return this.denuncias.filter(d => d.idEstado === 8).length;
  }

  get pendienteRespuesta(): number {
    return this.denuncias.filter(d => d.idEstado === 10).length;
  }

  get cerradas(): number {
    return this.denuncias.filter(d => d.idEstado === 9).length;
  }

  private buildDonut(denuncias: DenunciaInvestigadorResumenDto[]): void {
    if (!denuncias.length) { this.donutSegments = []; return; }
    const r = 54;
    const circ = 2 * Math.PI * r;
    let cumOffset = 0;

    const porEstado = new Map<number, number>();
    for (const d of denuncias) {
      porEstado.set(d.idEstado, (porEstado.get(d.idEstado) ?? 0) + 1);
    }

    this.donutSegments = Array.from(porEstado.entries()).map(([idEstado, cantidad]) => {
      const pct = cantidad / denuncias.length;
      const len = pct * circ;
      const seg: DonutSegment = {
        dasharray:  `${len} ${circ}`,
        dashoffset: -cumOffset,
        color:      this.STATE_COLORS[idEstado] ?? '#94a3b8',
        estado:     this.STATE_LABELS[idEstado] ?? 'Otro',
        cantidad,
        pct:        Math.round(pct * 100)
      };
      cumOffset += len;
      return seg;
    });
  }

  irBandeja(filtro: 'activas' | 'cerradas' | 'todas'): void {
    this.router.navigate(['/pages/investigador/bandeja'], { queryParams: { filtro } });
  }
}
