import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { DenunciaService, MisDenunciaDto } from '../denuncia.service';
import { PdfDenunciaService } from '../../../core/pdf-denuncia.service';

@Component({
  selector: 'app-mis-denuncias',
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe],
  templateUrl: './mis-denuncias.component.html',
  styleUrl: './mis-denuncias.component.scss'
})
export class MisDenunciasComponent implements OnInit {
  private router         = inject(Router);
  private denunciaService = inject(DenunciaService);
  private pdfService     = inject(PdfDenunciaService);

  denuncias: MisDenunciaDto[] = [];
  cargando  = true;
  error: string | null = null;
  pdfGenerando = new Set<number>();

  ngOnInit(): void {
    this.denunciaService.getMisDenuncias().subscribe({
      next: (lista) => { this.denuncias = lista; this.cargando = false; },
      error: () => { this.error = 'No se pudieron cargar las denuncias. Intente nuevamente.'; this.cargando = false; }
    });
  }

  getEstadoBadge(estado: string): string {
    const map: Record<string, string> = {
      'Creado':    'badge-creado',
      'Guardado':  'badge-guardado',
      'Enviado':   'badge-enviado',
      'Corregido': 'badge-corregido',
      'Devuelto':  'badge-devuelto',
      'Aprobado':  'badge-aprobado',
      'Rechazado': 'badge-rechazado',
      'Cerrado':   'badge-cerrado'
    };
    return map[estado] ?? 'bg-secondary';
  }

  esBorrador(d: MisDenunciaDto): boolean { return d.estado === 'Guardado' || d.estado === 'Creado'; }
  esDevuelto(d: MisDenunciaDto): boolean  { return d.estado === 'Devuelto'; }

  puedeDescargarPdf(d: MisDenunciaDto): boolean {
    return d.estado !== 'Creado' && d.estado !== 'Guardado';
  }

  continuarBorrador(d: MisDenunciaDto): void {
    const tipo = d.esAnonimo ? 'anonimo' : 'no-anonimo';
    this.router.navigate(['/pages/denuncia/crear', tipo], {
      queryParams: { borrador: d.id }
    });
  }

  descargarPdf(d: MisDenunciaDto): void {
    if (this.pdfGenerando.has(d.id)) return;
    this.pdfGenerando.add(d.id);

    this.pdfService.generarPdfReporter(d.id, {
      consecutivo:     d.consecutivo,
      nombreFormulario: d.nombreFormulario,
      estado:          d.estado,
      esAnonimo:       d.esAnonimo,
      fechaCreacion:   d.fechaCreacion
    }).subscribe({
      next:  () => { this.pdfGenerando.delete(d.id); },
      error: () => { this.pdfGenerando.delete(d.id); alert('Error al generar el PDF. Intente nuevamente.'); }
    });
  }

  volver(): void { this.router.navigate(['/pages/inicio']); }
  nueva():  void { this.router.navigate(['/pages/inicio']); }
}
