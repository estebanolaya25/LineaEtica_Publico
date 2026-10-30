import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DenunciaService, EstadisticasDto } from '../../denuncia/denuncia.service';
import { AuthService } from '../../../auth/auth.service';
import { EmpresaService, EmpresaDto } from '../../../core/empresa.service';

interface DonutSegment {
  dasharray: string;
  dashoffset: number;
  color: string;
  estado: string;
  cantidad: number;
  pct: number;
}

interface BarData {
  mes: string;
  label: string;
  cantidad: number;
  heightPct: number;
}

interface EmpresaRow {
  nombreEmpresa: string;
  cantidad: number;
  widthPct: number;
}

interface CanalRow {
  canal: string;
  cantidad: number;
  widthPct: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl:    './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  private denunciaService = inject(DenunciaService);
  private empresaService  = inject(EmpresaService);
  private router          = inject(Router);
  private auth            = inject(AuthService);

  readonly esDuenoProceso = this.auth.getCurrentUserRole() === 'Jefe del proceso';
  readonly empresaPropia  = this.auth.getCurrentUserEmpresaNombre();

  cargando = true;
  error: string | null = null;
  stats: EstadisticasDto | null = null;

  empresas: EmpresaDto[] = [];
  empresaSeleccionada: number | null = null;

  donutSegments: DonutSegment[] = [];
  barData: BarData[] = [];
  empresaRows: EmpresaRow[] = [];
  canalRows: CanalRow[] = [];

  private readonly STATE_COLORS: Record<number, string> = {
    2: '#3b82f6',
    3: '#8b5cf6',
    4: '#10b981',
    5: '#ef4444',
    7: '#f59e0b',
    8: '#1a7fc1',
    9: '#198754',
  };

  private readonly MONTHS_ES = ['Ene','Feb','Mar','Abr','May','Jun',
                                 'Jul','Ago','Sep','Oct','Nov','Dic'];

  ngOnInit(): void {
    if (this.esDuenoProceso) {
      this.empresaService.listar().subscribe({
        next: (lista) => this.empresas = lista,
        error: () => {}
      });
    }
    this.cargarEstadisticas();
  }

  private cargarEstadisticas(): void {
    this.cargando = true;
    this.denunciaService.getEstadisticas(this.empresaSeleccionada).subscribe({
      next: (data: EstadisticasDto) => {
        this.stats = data;
        this.buildDonut(data);
        this.buildBars(data);
        this.buildCanalRows(data);
        if (this.esDuenoProceso && this.empresaSeleccionada == null) this.buildEmpresaRows(data);
        this.cargando = false;
      },
      error: () => {
        this.error = 'No se pudieron cargar las estadísticas.';
        this.cargando = false;
      }
    });
  }

  onEmpresaChange(): void {
    this.cargarEstadisticas();
  }

  get nombreEmpresaMostrado(): string | null {
    if (!this.esDuenoProceso) return this.empresaPropia;
    if (this.empresaSeleccionada == null) return null;
    return this.empresas.find(e => e.id === this.empresaSeleccionada)?.nombre ?? null;
  }

  private buildDonut(data: EstadisticasDto): void {
    if (!data.total) { this.donutSegments = []; return; }
    const r = 54;
    const circ = 2 * Math.PI * r;
    let cumOffset = 0;

    this.donutSegments = data.porEstado.map(e => {
      const pct = e.cantidad / data.total;
      const len = pct * circ;
      const seg: DonutSegment = {
        dasharray:  `${len} ${circ}`,
        dashoffset: -cumOffset,
        color:      this.STATE_COLORS[e.idEstado] ?? '#94a3b8',
        estado:     e.estado,
        cantidad:   e.cantidad,
        pct:        Math.round(pct * 100)
      };
      cumOffset += len;
      return seg;
    });
  }

  private buildBars(data: EstadisticasDto): void {
    const now = new Date();
    const meses: string[] = [];
    for (let i = 5; i >= 0; i--) {
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      meses.push(key);
    }

    const maxVal = Math.max(...data.porMes.map(m => m.cantidad), 1);

    this.barData = meses.map(mes => {
      const found = data.porMes.find(m => m.mes === mes);
      const cantidad = found?.cantidad ?? 0;
      const [y, m] = mes.split('-');
      return {
        mes,
        label: this.MONTHS_ES[parseInt(m) - 1],
        cantidad,
        heightPct: (cantidad / maxVal) * 100
      };
    });
  }

  private buildEmpresaRows(data: EstadisticasDto): void {
    const maxVal = Math.max(...data.porEmpresa.map(e => e.cantidad), 1);
    this.empresaRows = data.porEmpresa.map(e => ({
      nombreEmpresa: e.nombreEmpresa,
      cantidad:      e.cantidad,
      widthPct:      (e.cantidad / maxVal) * 100
    }));
  }

  private buildCanalRows(data: EstadisticasDto): void {
    const maxVal = Math.max(...data.porCanal.map(c => c.cantidad), 1);
    this.canalRows = data.porCanal.map(c => ({
      canal:    c.canal,
      cantidad: c.cantidad,
      widthPct: (c.cantidad / maxVal) * 100
    }));
  }

  getColor(idEstado: number): string {
    return this.STATE_COLORS[idEstado] ?? '#94a3b8';
  }

  irBandeja(seccion: 'pendientes' | 'historial'): void {
    this.router.navigate(['/pages/admin/bandeja'], { queryParams: { seccion } });
  }

  irInvestigaciones(): void {
    this.router.navigate(['/pages/admin/investigaciones']);
  }

  get anonimaPct(): number {
    if (!this.stats?.total) return 0;
    return Math.round((this.stats.anonimas / this.stats.total) * 100);
  }

  get noAnonimaPct(): number {
    if (!this.stats?.total) return 0;
    return Math.round((this.stats.noAnonimas / this.stats.total) * 100);
  }
}
