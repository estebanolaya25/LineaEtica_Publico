import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { DenunciaService, DenunciaAdminResumenDto } from '../../denuncia/denuncia.service';
import { PdfDenunciaService } from '../../../core/pdf-denuncia.service';
import { AuthService } from '../../../auth/auth.service';

type Seccion = 'pendientes' | 'historial' | 'all';

@Component({
  selector: 'app-bandeja',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './bandeja.component.html',
  styleUrl: './bandeja.component.scss'
})
export class BandejaComponent implements OnInit, OnDestroy {
  private router   = inject(Router);
  private route    = inject(ActivatedRoute);
  private svc      = inject(DenunciaService);
  private pdfSvc   = inject(PdfDenunciaService);
  private auth     = inject(AuthService);

  readonly esDuenoProceso = this.auth.getCurrentUserRole() === 'Jefe del proceso';

  denuncias: DenunciaAdminResumenDto[] = [];
  cargando  = true;
  error: string | null = null;
  pdfGenerando = new Set<number>();

  seccion = signal<Seccion>('all');
  private sub?: Subscription;

  get pendientes(): DenunciaAdminResumenDto[] {
    return this.denuncias.filter(d => d.idEstado === 2 || d.idEstado === 3 || d.idEstado === 10);
  }

  get historial(): DenunciaAdminResumenDto[] {
    return this.denuncias.filter(d => d.idEstado !== 2 && d.idEstado !== 3 && d.idEstado !== 10);
  }

  get showPendientes(): boolean { return this.seccion() === 'all' || this.seccion() === 'pendientes'; }
  get showHistorial():  boolean { return this.seccion() === 'all' || this.seccion() === 'historial'; }

  ngOnInit(): void {
    this.sub = this.route.queryParams.subscribe(params => {
      const s = params['seccion'];
      this.seccion.set(s === 'pendientes' ? 'pendientes' : s === 'historial' ? 'historial' : 'all');
    });

    this.svc.getTodasDenuncias().subscribe({
      next:  (lista) => { this.denuncias = lista; this.cargando = false; },
      error: ()      => { this.error = 'Error al cargar las denuncias.'; this.cargando = false; }
    });
  }

  ngOnDestroy(): void { this.sub?.unsubscribe(); }

  revisar(d: DenunciaAdminResumenDto): void {
    if (d.idEstado === 10)
      this.router.navigate(['/pages/admin/investigacion', d.id]);
    else
      this.router.navigate(['/pages/admin/revision', d.id]);
  }

  puedeDescargarPdf(d: DenunciaAdminResumenDto): boolean {
    return d.idEstado >= 2;
  }

  descargarPdf(d: DenunciaAdminResumenDto): void {
    if (this.pdfGenerando.has(d.id)) return;
    this.pdfGenerando.add(d.id);

    this.pdfSvc.generarPdfAdmin(d.id, {
      consecutivo:      d.consecutivo,
      nombreFormulario: d.nombreFormulario,
      estado:           d.estado,
      esAnonimo:        d.esAnonimo,
      usuarioReportante: d.usuarioReportante,
      fechaCreacion:    d.fechaCreacion
    }).subscribe({
      next:  () => { this.pdfGenerando.delete(d.id); },
      error: () => { this.pdfGenerando.delete(d.id); alert('Error al generar el PDF. Intente nuevamente.'); }
    });
  }

  getEstadoBadge(estado: string): string {
    const map: Record<string, string> = {
      'Enviado':            'badge-enviado',
      'Corregido':          'badge-corregido',
      'Devuelto':           'badge-devuelto',
      'Aprobado':           'badge-aprobado',
      'Rechazado':          'badge-rechazado',
      'Cerrado':            'badge-cerrado',
      'Pendiente analista': 'badge-warning text-dark',
    };
    return map[estado] ?? 'bg-secondary';
  }
}
