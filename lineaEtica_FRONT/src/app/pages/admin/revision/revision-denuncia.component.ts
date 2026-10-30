import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  DenunciaService, DenunciaDetalleAdminDto, FormularioDto,
  SeccionDto, CampoDto, ParametroDto, CampoRechazadoDto, AdjuntoDto, TIPO_CAMPO
} from '../../denuncia/denuncia.service';
import { PdfDenunciaService } from '../../../core/pdf-denuncia.service';
import { InvestigadorService, InvestigadorResumenDto } from '../../investigador/investigador.service';
import { AuthService } from '../../../auth/auth.service';

type AccionAdmin = 'aprobar' | 'devolver' | 'rechazar' | null;

@Component({
  selector: 'app-revision-denuncia',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './revision-denuncia.component.html',
  styleUrl: './revision-denuncia.component.scss'
})
export class RevisionDenunciaComponent implements OnInit {
  private route   = inject(ActivatedRoute);
  private router  = inject(Router);
  private cdr     = inject(ChangeDetectorRef);
  private svc     = inject(DenunciaService);
  private pdfSvc  = inject(PdfDenunciaService);
  private invSvc  = inject(InvestigadorService);
  private auth    = inject(AuthService);

  readonly soloLectura = this.auth.getCurrentUserRole() === 'Jefe del proceso';
  readonly esConsultor = this.auth.getCurrentUserRole() === 'Consultor';

  esSeccionSoloDefiniciones(seccion: SeccionDto): boolean {
    return seccion.campos.length > 0 && seccion.campos.every(c => c.tipoCampo === TIPO_CAMPO.LABEL);
  }

  readonly TIPO = TIPO_CAMPO;

  idDenuncia     = 0;
  cargando       = true;
  pdfGenerando   = false;
  error: string | null = null;
  procesando     = false;
  accionRealizada = false;
  mensajeAccion  = '';

  denuncia: DenunciaDetalleAdminDto | null = null;
  formulario: FormularioDto | null = null;
  parametrosMap = new Map<number, ParametroDto[]>();
  adjuntos: AdjuntoDto[] = [];

  accionSeleccionada: AccionAdmin = null;
  comentarioAdmin = '';
  modoSeleccionCampos = false;
  camposParaDevolver  = new Map<string, CampoRechazadoDto>();

  investigadores: InvestigadorResumenDto[] = [];
  mostrarAsignacion = false;
  idInvestigadorSeleccionado: number | null = null;
  comentarioAsignacion = '';
  asignando = false;

  get puedeActuar(): boolean {
    return !this.soloLectura && (this.denuncia?.idEstado === 2 || this.denuncia?.idEstado === 3);
  }

  get puedeAsignarInvestigador(): boolean {
    return !this.soloLectura && this.denuncia?.idEstado === 4;
  }

  get totalCamposSeleccionados(): number {
    return this.camposParaDevolver.size;
  }

  ngOnInit(): void {
    this.idDenuncia = Number(this.route.snapshot.paramMap.get('id'));
    this.cargarDatos();
    this.invSvc.getInvestigadores().subscribe({
      next: (lista) => { this.investigadores = lista; },
      error: () => {}
    });
  }

  private cargarDatos(): void {
    this.svc.getDenunciaDetalleAdmin(this.idDenuncia).subscribe({
      next: (detalle) => {
        this.denuncia = detalle;
        this.svc.getAdjuntosAdmin(this.idDenuncia).subscribe({
          next: (adj) => { this.adjuntos = adj; },
          error: () => {}
        });
        this.svc.getFormulario(detalle.idFormulario).subscribe({
          next:  (form) => { this.formulario = form; this.cargarParametros(form); },
          error: ()     => { this.error = 'Error al cargar el formulario.'; this.cargando = false; }
        });
      },
      error: () => { this.error = 'Error al cargar la denuncia.'; this.cargando = false; }
    });
  }

  adjuntosDeCampo(idCampo: number): AdjuntoDto[] {
    return this.adjuntos.filter(a => a.idCampo === idCampo);
  }

  descargarAdjunto(idAdjunto: number, nombreOriginal: string): void {
    this.svc.descargarAdjuntoAdmin(idAdjunto).subscribe({
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

  private cargarParametros(form: FormularioDto): void {
    const tipos = new Set<number>();
    for (const s of form.secciones)
      for (const c of s.campos)
        if (c.tipoParametro) tipos.add(c.tipoParametro);

    if (tipos.size === 0) { this.cargando = false; return; }

    const obs: { [k: number]: any } = {};
    tipos.forEach(t => { obs[t] = this.svc.getParametros(t); });

    forkJoin(obs).subscribe({
      next:  (res: any) => {
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
  getNombreParametrosMulti(idTipo: number, ids: number[]): string {
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
  esCampoRechazado(idCampo: number, idSeccion: number): boolean {
    return this.denuncia?.camposRechazados.some(
      c => c.idCampo === idCampo && c.idSeccion === idSeccion
    ) ?? false;
  }

  toggleCampo(idCampo: number, idSeccion: number, idFila: number | null = null): void {
    const key = idFila != null ? `${idSeccion}_${idCampo}_fila${idFila}` : `${idSeccion}_${idCampo}`;
    if (this.camposParaDevolver.has(key)) this.camposParaDevolver.delete(key);
    else this.camposParaDevolver.set(key, { idCampo, idSeccion, idFila });
    this.cdr.detectChanges();
  }

  isCampoSeleccionado(idCampo: number, idSeccion: number, idFila: number | null = null): boolean {
    const key = idFila != null ? `${idSeccion}_${idCampo}_fila${idFila}` : `${idSeccion}_${idCampo}`;
    return this.camposParaDevolver.has(key);
  }

  getCamposSeleccionables(seccion: SeccionDto): CampoDto[] {
    return seccion.campos.filter(c => c.tipoCampo !== this.TIPO.LABEL);
  }

  isSeccionSeleccionada(seccion: SeccionDto): boolean {
    const campos = this.getCamposSeleccionables(seccion);
    return campos.length > 0 && campos.every(c => this.isCampoSeleccionado(c.id, seccion.id));
  }

  isSeccionIndeterminada(seccion: SeccionDto): boolean {
    const campos = this.getCamposSeleccionables(seccion);
    const n = campos.filter(c => this.isCampoSeleccionado(c.id, seccion.id)).length;
    return n > 0 && n < campos.length;
  }

  toggleSeccion(seccion: SeccionDto): void {
    const campos = this.getCamposSeleccionables(seccion);
    const todosSeleccionados = this.isSeccionSeleccionada(seccion);
    campos.forEach(c => {
      const key = `${seccion.id}_${c.id}`;
      if (todosSeleccionados) this.camposParaDevolver.delete(key);
      else this.camposParaDevolver.set(key, { idCampo: c.id, idSeccion: seccion.id, idFila: null });
    });
    this.cdr.detectChanges();
  }

  iniciarAccion(accion: AccionAdmin): void {
    this.accionSeleccionada  = accion;
    this.comentarioAdmin     = '';
    this.modoSeleccionCampos = accion === 'devolver';
    this.camposParaDevolver.clear();
  }

  cancelarAccion(): void {
    this.accionSeleccionada  = null;
    this.modoSeleccionCampos = false;
    this.camposParaDevolver.clear();
  }

  confirmarAccion(): void {
    if (this.procesando) return;
    if (this.accionSeleccionada !== 'aprobar' && !this.comentarioAdmin.trim()) {
      alert('Debe ingresar un comentario.');
      return;
    }

    this.procesando = true;
    const dto = {
      comentario: this.comentarioAdmin.trim() || null,
      camposRechazados: Array.from(this.camposParaDevolver.values())
    };

    const obs = this.accionSeleccionada === 'aprobar'
      ? this.svc.aprobarDenuncia(this.idDenuncia, dto)
      : this.accionSeleccionada === 'devolver'
        ? this.svc.devolverDenuncia(this.idDenuncia, dto)
        : this.svc.rechazarDenuncia(this.idDenuncia, dto);

    obs.subscribe({
      next: (r) => {
        this.procesando       = false;
        this.accionRealizada  = true;
        this.mensajeAccion    = r.mensaje;
        this.accionSeleccionada = null;
        setTimeout(() => this.router.navigate(['/pages/admin/bandeja']), 2000);
      },
      error: () => {
        this.procesando = false;
        alert('Error al procesar la acción. Intente nuevamente.');
      }
    });
  }

  descargarPdf(): void {
    if (!this.denuncia || this.pdfGenerando) return;
    this.pdfGenerando = true;
    this.pdfSvc.generarPdfAdmin(this.idDenuncia, {
      consecutivo:       this.denuncia.consecutivo,
      nombreFormulario:  this.formulario?.nombre ?? '',
      estado:            this.denuncia.estado,
      esAnonimo:         this.denuncia.esAnonimo,
      usuarioReportante: this.denuncia.usuarioReportante,
      fechaCreacion:     this.denuncia.fechaCreacion
    }).subscribe({
      next:  () => { this.pdfGenerando = false; },
      error: () => { this.pdfGenerando = false; alert('Error al generar el PDF.'); }
    });
  }

  iniciarAsignacion(): void { this.mostrarAsignacion = true; }
  cancelarAsignacion(): void {
    this.mostrarAsignacion = false;
    this.idInvestigadorSeleccionado = null;
    this.comentarioAsignacion = '';
  }

  confirmarAsignacion(): void {
    if (!this.idInvestigadorSeleccionado || this.asignando) return;
    this.asignando = true;
    this.invSvc.asignarInvestigador(
      this.idDenuncia,
      this.idInvestigadorSeleccionado,
      this.comentarioAsignacion || undefined
    ).subscribe({
      next: (r) => {
        this.asignando     = false;
        this.accionRealizada = true;
        this.mensajeAccion = r.mensaje;
        setTimeout(() => this.router.navigate(['/pages/admin/bandeja']), 2000);
      },
      error: () => { this.asignando = false; alert('Error al asignar el investigador.'); }
    });
  }

  volver(): void { this.router.navigate(['/pages/admin/bandeja']); }

  getEstadoBadge(estado: string): string {
    const map: { [k: string]: string } = {
      'Enviado':   'badge-enviado',
      'Corregido': 'badge-corregido',
      'Devuelto':  'badge-devuelto',
      'Aprobado':  'badge-aprobado',
      'Rechazado': 'badge-rechazado'
    };
    return map[estado] ?? 'bg-secondary';
  }
}
