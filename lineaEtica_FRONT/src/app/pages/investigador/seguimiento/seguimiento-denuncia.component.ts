import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  InvestigadorService, DenunciaInvestigadorDetalleDto,
  SeguimientoDto, AdjuntoInvestigadorDto, InvestigadorResumenDto
} from '../investigador.service';
import { DenunciaService, FormularioDto, ParametroDto, TIPO_CAMPO } from '../../denuncia/denuncia.service';

@Component({
  selector: 'app-seguimiento-denuncia',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './seguimiento-denuncia.component.html',
  styleUrl: './seguimiento-denuncia.component.scss'
})
export class SeguimientoDenunciaComponent implements OnInit {
  private route   = inject(ActivatedRoute);
  private router  = inject(Router);
  private cdr     = inject(ChangeDetectorRef);
  private svc     = inject(InvestigadorService);
  private denSvc  = inject(DenunciaService);

  readonly TIPO = TIPO_CAMPO;

  idDenuncia   = 0;
  cargando     = true;
  error: string | null = null;
  procesando   = false;
  accionHecha  = false;
  mensajeAccion = '';

  denuncia: DenunciaInvestigadorDetalleDto | null = null;
  formulario: FormularioDto | null = null;
  parametrosMap = new Map<number, ParametroDto[]>();

  nuevoComentario  = '';
  archivosNuevos: File[] = [];
  enviandoComentario = false;

  mostrarCierre    = false;
  comentarioCierre = '';

  mostrarSolicitar          = false;
  comentarioSolicitar       = '';
  archivosSolicitar: File[] = [];
  enviandoSolicitud         = false;

  mostrarReasignacion      = false;
  investigadoresDisponibles: InvestigadorResumenDto[] = [];
  idInvestigadorSeleccionado: number | null = null;
  comentarioReasignacion   = '';
  cargandoInvestigadores   = false;

  get estaAbierta(): boolean    { return this.denuncia?.idEstado === 8; }
  get pendienteConsultor(): boolean { return this.denuncia?.idEstado === 10; }

  ngOnInit(): void {
    this.idDenuncia = Number(this.route.snapshot.paramMap.get('id'));
    this.cargarDatos();
  }

  private cargarDatos(): void {
    this.svc.getDetalle(this.idDenuncia).subscribe({
      next: (d) => {
        this.denuncia = d;
        this.denSvc.getFormulario(d.idFormulario).subscribe({
          next:  (f) => { this.formulario = f; this.cargarParametros(f); },
          error: ()  => { this.cargando = false; }
        });
      },
      error: () => { this.error = 'Error al cargar la denuncia.'; this.cargando = false; }
    });
  }

  private cargarParametros(form: FormularioDto): void {
    const tipos = new Set<number>();
    for (const s of form.secciones)
      for (const c of s.campos)
        if (c.tipoParametro) tipos.add(c.tipoParametro);

    if (tipos.size === 0) { this.cargando = false; return; }

    const obs: { [k: number]: any } = {};
    tipos.forEach(t => { obs[t] = this.denSvc.getParametros(t); });

    forkJoin(obs).subscribe({
      next: (res: any) => {
        for (const [k, v] of Object.entries(res))
          this.parametrosMap.set(Number(k), v as ParametroDto[]);
        this.cargando = false;
      },
      error: () => { this.cargando = false; }
    });
  }

  getValorCampo(idCampo: number): string | null {
    return this.denuncia?.respuestas.find(r => r.idCampo === idCampo)?.textoRespuesta ?? null;
  }
  getOpcionCampo(idCampo: number): number | null {
    return this.denuncia?.respuestas.find(r => r.idCampo === idCampo)?.idOpcion ?? null;
  }
  getNombreParametro(idTipo: number, idOpcion: number | null): string {
    if (!idOpcion) return '—';
    return this.parametrosMap.get(idTipo)?.find(p => p.id === idOpcion)?.nombre ?? '—';
  }
  getValoresMulti(idCampo: number): number[] {
    return this.denuncia?.respuestas
      .filter(r => r.idCampo === idCampo && r.idOpcion != null)
      .map(r => r.idOpcion!) ?? [];
  }
  getNombresMulti(idTipo: number, ids: number[]): string {
    const lista = this.parametrosMap.get(idTipo) ?? [];
    return ids.map(id => lista.find(p => p.id === id)?.nombre ?? id.toString()).join(', ');
  }
  getFilasTabla(idCampoTabla: number): number[] {
    const grupos = new Set<number>();
    this.denuncia?.respuestasTabla
      .filter(r => r.idCampoTabla === idCampoTabla)
      .forEach(r => grupos.add(r.idGrupo));
    return Array.from(grupos).sort((a, b) => a - b);
  }
  getValorFilaTabla(idCampoTabla: number, idGrupo: number, idCampoHijo: number): string | null {
    return this.denuncia?.respuestasTabla
      .find(r => r.idCampoTabla === idCampoTabla && r.idGrupo === idGrupo && r.idCampoHijo === idCampoHijo)
      ?.textoRespuesta ?? null;
  }

  onArchivosChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    const nuevos = Array.from(input.files);
    this.archivosNuevos = [...this.archivosNuevos, ...nuevos];
    input.value = '';
  }

  quitarArchivo(idx: number): void {
    this.archivosNuevos = this.archivosNuevos.filter((_, i) => i !== idx);
  }

  enviarComentario(): void {
    if (!this.nuevoComentario.trim() || this.enviandoComentario) return;
    this.enviandoComentario = true;
    this.svc.crearSeguimiento(this.idDenuncia, this.nuevoComentario.trim(), this.archivosNuevos).subscribe({
      next: (seg) => {
        this.denuncia?.seguimientos.push(seg);
        this.nuevoComentario   = '';
        this.archivosNuevos    = [];
        this.enviandoComentario = false;
        this.cdr.detectChanges();
      },
      error: () => { this.enviandoComentario = false; alert('Error al guardar el comentario.'); }
    });
  }

  descargarAdjunto(idAdjunto: number, nombreOriginal: string): void {
    this.svc.descargarAdjunto(idAdjunto).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a   = document.createElement('a');
        a.href     = url;
        a.download = nombreOriginal;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => alert('Error al descargar el archivo.')
    });
  }

  formatBytes(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  onArchivosSolicitudChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    this.archivosSolicitar = [...this.archivosSolicitar, ...Array.from(input.files)];
    input.value = '';
  }

  quitarArchivoSolicitud(idx: number): void {
    this.archivosSolicitar = this.archivosSolicitar.filter((_, i) => i !== idx);
  }

  confirmarSolicitud(): void {
    if (!this.comentarioSolicitar.trim() || this.enviandoSolicitud) return;
    this.enviandoSolicitud = true;
    this.svc.solicitarInformacion(this.idDenuncia, this.comentarioSolicitar.trim(), this.archivosSolicitar).subscribe({
      next: (seg) => {
        this.denuncia?.seguimientos.push(seg);
        if (this.denuncia) this.denuncia.idEstado = 10;
        this.comentarioSolicitar   = '';
        this.archivosSolicitar     = [];
        this.mostrarSolicitar      = false;
        this.enviandoSolicitud     = false;
      },
      error: () => { this.enviandoSolicitud = false; alert('Error al enviar la solicitud.'); }
    });
  }

  abrirReasignacion(): void {
    this.mostrarReasignacion = true;
    if (this.investigadoresDisponibles.length === 0) {
      this.cargandoInvestigadores = true;
      this.svc.getInvestigadoresDisponibles().subscribe({
        next: (lista) => {
          this.investigadoresDisponibles = lista;
          this.cargandoInvestigadores    = false;
        },
        error: () => { this.cargandoInvestigadores = false; }
      });
    }
  }

  confirmarReasignacion(): void {
    if (!this.idInvestigadorSeleccionado || !this.comentarioReasignacion.trim() || this.procesando) return;
    this.procesando = true;
    this.svc.reasignarInvestigacion(
      this.idDenuncia,
      this.idInvestigadorSeleccionado,
      this.comentarioReasignacion.trim()
    ).subscribe({
      next: (r) => {
        this.procesando            = false;
        this.accionHecha           = true;
        this.mensajeAccion         = r.mensaje;
        this.mostrarReasignacion   = false;
        setTimeout(() => this.router.navigate(['/pages/investigador/bandeja']), 2000);
      },
      error: (err) => {
        this.procesando = false;
        alert(err?.error ?? 'Error al reasignar la investigación.');
      }
    });
  }

  confirmarCierre(): void {
    if (!this.comentarioCierre.trim() || this.procesando) return;
    this.procesando = true;
    this.svc.cerrarInvestigacion(this.idDenuncia, this.comentarioCierre.trim()).subscribe({
      next: (r) => {
        this.procesando    = false;
        this.accionHecha   = true;
        this.mensajeAccion = r.mensaje;
        this.mostrarCierre = false;
        setTimeout(() => this.router.navigate(['/pages/investigador/bandeja']), 2000);
      },
      error: () => { this.procesando = false; alert('Error al cerrar la investigación.'); }
    });
  }

  getEstadoBadge(estado: string): string {
    const map: { [k: string]: string } = {
      'En Investigacion':  'badge-investigacion',
      'Cerrado':           'badge-cerrado',
      'Aprobado':          'badge-aprobado',
      'Pendiente analista':'badge-warning text-dark',
    };
    return map[estado] ?? 'bg-secondary';
  }

  volver(): void { this.router.navigate(['/pages/investigador/bandeja']); }
}
