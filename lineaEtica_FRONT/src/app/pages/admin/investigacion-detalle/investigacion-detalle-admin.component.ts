import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  InvestigadorService, DenunciaInvestigadorDetalleDto, SeguimientoDto
} from '../../investigador/investigador.service';
import { DenunciaService, FormularioDto, ParametroDto, TIPO_CAMPO, DecisionAdminRequestDto } from '../../denuncia/denuncia.service';
import { AuthService } from '../../../auth/auth.service';

@Component({
  selector: 'app-investigacion-detalle-admin',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule],
  templateUrl: './investigacion-detalle-admin.component.html',
  styleUrl: './investigacion-detalle-admin.component.scss'
})
export class InvestigacionDetalleAdminComponent implements OnInit {
  private route   = inject(ActivatedRoute);
  private router  = inject(Router);
  private svc     = inject(InvestigadorService);
  private denSvc  = inject(DenunciaService);
  private auth    = inject(AuthService);

  readonly TIPO = TIPO_CAMPO;
  readonly esDuenoProceso = this.auth.getCurrentUserRole() === 'Jefe del proceso';

  mostrarComentario     = false;
  textoComentario       = '';
  archivosComentario: File[] = [];
  enviandoComentario    = false;

  idDenuncia = 0;
  cargando   = true;
  error: string | null = null;
  mensajeAccion: string | null = null;

  mostrarDevolver      = false;
  comentarioDevolver   = '';
  enviandoDevolver     = false;

  get investigacionActiva(): boolean { return this.denuncia?.idEstado === 8 || this.denuncia?.idEstado === 10; }

  mostrarResponder          = false;
  comentarioRespuesta       = '';
  archivosRespuesta: File[] = [];
  enviandoRespuesta         = false;

  get pendienteAnalista(): boolean { return this.denuncia?.idEstado === 10; }

  denuncia: DenunciaInvestigadorDetalleDto | null = null;
  formulario: FormularioDto | null = null;
  parametrosMap = new Map<number, ParametroDto[]>();

  ngOnInit(): void {
    this.idDenuncia = Number(this.route.snapshot.paramMap.get('id'));
    this.cargarDatos();
  }

  private cargarDatos(): void {
    this.svc.getDetalleAdmin(this.idDenuncia).subscribe({
      next: (d) => {
        this.denuncia = d;
        this.denSvc.getFormulario(d.idFormulario).subscribe({
          next:  (f) => { this.formulario = f; this.cargarParametros(f); },
          error: ()  => { this.cargando = false; }
        });
      },
      error: () => { this.error = 'Investigación no encontrada.'; this.cargando = false; }
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

  confirmarDevolver(): void {
    if (!this.comentarioDevolver.trim() || this.enviandoDevolver) return;
    this.enviandoDevolver = true;
    const dto: DecisionAdminRequestDto = { comentario: this.comentarioDevolver.trim(), camposRechazados: [] };
    this.denSvc.devolverDenuncia(this.idDenuncia, dto).subscribe({
      next: () => {
        this.enviandoDevolver = false;
        this.mostrarDevolver  = false;
        this.mensajeAccion    = 'Denuncia devuelta al reportante para que aporte más información.';
        if (this.denuncia) this.denuncia.idEstado = 7;
      },
      error: () => { this.enviandoDevolver = false; alert('Error al devolver la denuncia.'); }
    });
  }

  onArchivosRespuestaChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    this.archivosRespuesta = [...this.archivosRespuesta, ...Array.from(input.files)];
    input.value = '';
  }

  quitarArchivoRespuesta(idx: number): void {
    this.archivosRespuesta = this.archivosRespuesta.filter((_, i) => i !== idx);
  }

  confirmarRespuesta(): void {
    if (!this.comentarioRespuesta.trim() || this.enviandoRespuesta) return;
    this.enviandoRespuesta = true;
    this.svc.responderInvestigador(this.idDenuncia, this.comentarioRespuesta.trim(), this.archivosRespuesta).subscribe({
      next: (seg) => {
        this.denuncia?.seguimientos.push(seg);
        if (this.denuncia) this.denuncia.idEstado = 8;
        this.comentarioRespuesta = '';
        this.archivosRespuesta   = [];
        this.mostrarResponder    = false;
        this.enviandoRespuesta   = false;
        this.mensajeAccion       = 'Respuesta enviada. La investigación ha vuelto a estado activo.';
      },
      error: () => { this.enviandoRespuesta = false; alert('Error al enviar la respuesta.'); }
    });
  }

  onArchivosComentarioChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    this.archivosComentario = [...this.archivosComentario, ...Array.from(input.files)];
    input.value = '';
  }

  quitarArchivoComentario(idx: number): void {
    this.archivosComentario = this.archivosComentario.filter((_, i) => i !== idx);
  }

  confirmarComentario(): void {
    if (!this.textoComentario.trim() || this.enviandoComentario) return;
    this.enviandoComentario = true;
    this.svc.agregarComentarioAdmin(this.idDenuncia, this.textoComentario.trim(), this.archivosComentario).subscribe({
      next: (seg) => {
        this.denuncia?.seguimientos.push(seg);
        this.textoComentario    = '';
        this.archivosComentario = [];
        this.mostrarComentario  = false;
        this.enviandoComentario = false;
        this.mensajeAccion      = 'Comentario agregado.';
      },
      error: () => { this.enviandoComentario = false; alert('Error al agregar el comentario.'); }
    });
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

  getEstadoBadge(estado: string): string {
    const map: { [k: string]: string } = {
      'En Investigacion': 'badge-investigacion',
      'Cerrado':          'badge-cerrado',
    };
    return map[estado] ?? 'bg-secondary';
  }

  volver(): void { this.router.navigate(['/pages/admin/investigaciones']); }
}
