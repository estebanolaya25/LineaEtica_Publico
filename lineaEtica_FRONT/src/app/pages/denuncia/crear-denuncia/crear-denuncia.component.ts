import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { forkJoin, firstValueFrom } from 'rxjs';
import {
  DenunciaService,
  CampoDto,
  SeccionDto,
  FormularioDto,
  CondicionDto,
  ParametroDto,
  RespuestaCampoDto,
  RespuestaTablaDto,
  CampoRechazadoDto,
  AdjuntoDto,
  TIPO_CAMPO
} from '../denuncia.service';
import { RegistradorService } from '../../registrador/registrador.service';
import { EmpresaService, EmpresaDto } from '../../../core/empresa.service';

interface FilaTabla {
  [idCampoHijo: number]: string | null;
}

@Component({
  selector: 'app-crear-denuncia',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './crear-denuncia.component.html',
  styleUrl: './crear-denuncia.component.scss'
})
export class CrearDenunciaComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private denunciaService = inject(DenunciaService);
  private registradorService = inject(RegistradorService);
  private empresaService = inject(EmpresaService);

  @Input() modoRegistrador = false;
  @Input() correoDestino: string | null = null;
  @Input() canalOrigen: string | null = null;
  @Input() idFormularioRegistrador = 2;
  @Input() idEmpresaRegistrador: number | null = null;

  empresas: EmpresaDto[] = [];
  idEmpresa: number | null = null;

  readonly TIPO = TIPO_CAMPO;

  formulario: FormularioDto | null = null;
  cargando = true;
  error: string | null = null;
  enviando = false;
  guardando = false;
  guardadoOk = false;
  exito = false;
  mensajeExito = '';
  esAnonimo = false;
  idFormulario = 1;

  idDenuncia: number | null = null;

  estaDevuelto = false;
  camposRechazados: CampoRechazadoDto[] = [];
  comentarioAdmin: string | null = null;

  currentStep = 0;

  valores: { [idCampo: number]: any } = {};
  valoresMulti: { [idCampo: number]: number[] } = {};
  filasTabla: { [idCampoTabla: number]: FilaTabla[] } = {};
  parametros: { [idTipo: number]: ParametroDto[] } = {};

  archivosSeleccionados = new Map<number, File[]>();
  adjuntosSubidos = new Map<number, AdjuntoDto[]>();
  subiendoAdjuntos = false;
  archivoArrastrandoSobre: number | null = null;

  erroresValidacion: string[] = [];

  private seccionesOcultas = new Set<number>();
  private camposOcultos = new Set<number>();

  get seccionesVisibles(): SeccionDto[] {
    return this.formulario?.secciones.filter(s => !this.seccionesOcultas.has(s.id)) ?? [];
  }

  get seccionActual(): SeccionDto | null {
    return this.seccionesVisibles[this.currentStep] ?? null;
  }

  get totalSteps(): number { return this.seccionesVisibles.length; }

  get progreso(): number {
    if (this.totalSteps <= 1) return 100;
    return Math.round((this.currentStep / (this.totalSteps - 1)) * 100);
  }

  get isFirstStep(): boolean { return this.currentStep === 0; }
  get isLastStep(): boolean { return this.currentStep === this.totalSteps - 1; }

  evaluarCondiciones(): void {
    if (!this.formulario?.condiciones?.length) return;
    this.seccionesOcultas.clear();
    this.camposOcultos.clear();
    for (const c of this.formulario.condiciones) {
      this.aplicarAccion(c, this.evaluarCondicion(c));
    }
    if (this.currentStep >= this.seccionesVisibles.length) {
      this.currentStep = Math.max(0, this.seccionesVisibles.length - 1);
    }
  }

  private evaluarCondicion(c: CondicionDto): boolean {
    if (c.idCampoOrigen == null) return true;
    const valor = String(this.valores[c.idCampoOrigen] ?? '');
    switch (c.condicion) {
      case 'igual':       return valor === c.valorCondicion;
      case 'diferente':   return valor !== c.valorCondicion;
      case 'vacio':       return valor.trim() === '';
      case 'no_vacio':    return valor.trim() !== '';
      case 'contiene':    return valor.toLowerCase().includes((c.valorCondicion ?? '').toLowerCase());
      case 'no_contiene': return !valor.toLowerCase().includes((c.valorCondicion ?? '').toLowerCase());
      default:            return false;
    }
  }

  private aplicarAccion(c: CondicionDto, cumplida: boolean): void {
    switch (c.accion) {
      case 'visible':
        if (!cumplida) {
          if (c.idCampoDestino != null) this.camposOcultos.add(c.idCampoDestino);
          else this.seccionesOcultas.add(c.idSeccionDestino);
        }
        break;
      case 'oculto':
        if (cumplida) {
          if (c.idCampoDestino != null) this.camposOcultos.add(c.idCampoDestino);
          else this.seccionesOcultas.add(c.idSeccionDestino);
        }
        break;
    }
  }

  isCampoVisible(campoId: number): boolean { return !this.camposOcultos.has(campoId); }
  isSeccionVisible(seccionId: number): boolean { return !this.seccionesOcultas.has(seccionId); }
  onCampoChange(): void { this.evaluarCondiciones(); this.erroresValidacion = []; }

  esCampoRechazado(idCampo: number, idSeccion: number): boolean {
    return this.camposRechazados.some(c => c.idCampo === idCampo && c.idSeccion === idSeccion);
  }

  esCampoEditable(idCampo: number, idSeccion: number): boolean {
    if (!this.estaDevuelto) return true;
    if (this.camposRechazados.length === 0) return true;
    return this.esCampoRechazado(idCampo, idSeccion);
  }

  ngOnInit(): void {
    if (this.modoRegistrador) {
      this.idFormulario = this.idFormularioRegistrador;
      this.esAnonimo = this.idFormulario === 1;
      this.idEmpresa = this.idEmpresaRegistrador;
    } else {
      const tipo = this.route.snapshot.paramMap.get('tipo');
      this.esAnonimo = tipo === 'anonimo';
      this.idFormulario = this.esAnonimo ? 1 : 2;

      const borradorParam = this.route.snapshot.queryParamMap.get('borrador');
      if (borradorParam) {
        this.idDenuncia = parseInt(borradorParam, 10);
      }

      this.empresaService.listar().subscribe({
        next: (lista) => { this.empresas = lista; },
        error: () => {  }
      });
    }

    this.denunciaService.getFormulario(this.idFormulario).subscribe({
      next: (form) => {
        this.formulario = form;
        this.inicializarFormulario(form);
      },
      error: () => {
        this.error = 'No se pudo cargar el formulario. Intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  private inicializarFormulario(form: FormularioDto): void {
    const tiposUnicos = new Set<number>();

    form.secciones.forEach(sec => {
      sec.campos.forEach(campo => {
        if (campo.tipoCampo !== this.TIPO.TABLA && campo.tipoCampo !== this.TIPO.LABEL) {
          this.valores[campo.id] = null;
        }
        if (campo.tipoCampo === this.TIPO.TABLA) {
          this.filasTabla[campo.id] = [this.nuevaFila(campo)];
          campo.camposHijo?.forEach(hijo => {
            if (hijo.tipoCampo === this.TIPO.SELECT_UNICO && hijo.tipoParametro > 0) {
              tiposUnicos.add(hijo.tipoParametro);
            }
          });
        }
        if (campo.tipoCampo === this.TIPO.SELECT_MULTIPLE) {
          this.valoresMulti[campo.id] = [];
        }
        if ((campo.tipoCampo === this.TIPO.SELECT_UNICO || campo.tipoCampo === this.TIPO.SELECT_MULTIPLE) && campo.tipoParametro > 0) {
          tiposUnicos.add(campo.tipoParametro);
        }
      });
    });

    const finalizarCarga = () => {
      this.evaluarCondiciones();
      if (this.idDenuncia) {
        this.cargarBorrador(this.idDenuncia);
      } else {
        this.cargando = false;
      }
    };

    if (tiposUnicos.size === 0) {
      finalizarCarga();
      return;
    }

    const arr = Array.from(tiposUnicos);
    forkJoin(arr.map(t => this.denunciaService.getParametros(t))).subscribe({
      next: (results) => {
        arr.forEach((tipo, i) => { this.parametros[tipo] = results[i]; });
        finalizarCarga();
      },
      error: () => {
        this.error = 'Error al cargar las opciones de los campos.';
        this.cargando = false;
      }
    });
  }

  private cargarBorrador(id: number): void {
    this.denunciaService.getBorrador(id).subscribe({
      next: (borrador) => {
        this.poblarDesdeBorrador(borrador);
        this.evaluarCondiciones();
        this.denunciaService.getAdjuntos(id).subscribe({
          next: (adj) => {
            this.adjuntosSubidos.clear();
            adj.forEach(a => {
              const lista = this.adjuntosSubidos.get(a.idCampo) ?? [];
              lista.push(a);
              this.adjuntosSubidos.set(a.idCampo, lista);
            });
          },
          error: () => {}
        });
        this.cargando = false;
      },
      error: () => {
        this.idDenuncia = null;
        this.cargando = false;
      }
    });
  }

  private poblarDesdeBorrador(borrador: any): void {
    if (borrador.idEstado === 7) {
      this.estaDevuelto = true;
    }
    if (borrador.camposRechazados?.length) {
      this.camposRechazados = borrador.camposRechazados;
    }
    if (borrador.comentarioAdmin) {
      this.comentarioAdmin = borrador.comentarioAdmin;
    }

    borrador.respuestas.forEach((r: RespuestaCampoDto) => {
      const campo = this.encontrarCampo(r.idCampo);
      if (campo?.tipoCampo === this.TIPO.SELECT_MULTIPLE) {
        if (!this.valoresMulti[r.idCampo]) this.valoresMulti[r.idCampo] = [];
        if (r.idOpcion != null) this.valoresMulti[r.idCampo].push(r.idOpcion);
      } else if (r.idOpcion != null) {
        this.valores[r.idCampo] = r.idOpcion;
      } else {
        this.valores[r.idCampo] = r.textoRespuesta;
      }
    });

    if (borrador.respuestasTabla?.length) {
      const byCampo: { [idCampoTabla: number]: { [grupo: number]: FilaTabla } } = {};
      borrador.respuestasTabla.forEach((r: RespuestaTablaDto) => {
        if (!byCampo[r.idCampoTabla]) byCampo[r.idCampoTabla] = {};
        if (!byCampo[r.idCampoTabla][r.idGrupo]) byCampo[r.idCampoTabla][r.idGrupo] = {};
        byCampo[r.idCampoTabla][r.idGrupo][r.idCampoHijo] = r.textoRespuesta ?? null;
      });

      Object.entries(byCampo).forEach(([idStr, grupos]) => {
        const idCampoTabla = parseInt(idStr);
        const gruposOrdenados = Object.entries(grupos)
          .sort(([a], [b]) => parseInt(a) - parseInt(b))
          .map(([, fila]) => fila);
        this.filasTabla[idCampoTabla] = gruposOrdenados;
      });
    }
  }

  private encontrarCampo(idCampo: number): CampoDto | undefined {
    for (const sec of this.formulario?.secciones ?? []) {
      const found = sec.campos.find(c => c.id === idCampo);
      if (found) return found;
    }
    return undefined;
  }

  guardar(): void {
    if (!this.formulario || this.guardando) return;
    if (!this.idEmpresa) {
      this.error = 'Debe seleccionar la empresa antes de continuar.';
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }
    this.guardando = true;
    this.guardadoOk = false;

    const { respuestas, respuestasTabla } = this.construirRespuestas();

    this.denunciaService.guardarDenuncia({
      idDenuncia: this.idDenuncia,
      idFormulario: this.idFormulario,
      esAnonimo: this.esAnonimo,
      idEmpresa: this.idEmpresa,
      respuestas,
      respuestasTabla
    }).subscribe({
      next: (res) => {
        if (res.exito) {
          this.idDenuncia = res.idDenuncia ?? this.idDenuncia;
          this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { borrador: this.idDenuncia },
            replaceUrl: true
          });
          this.uploadPendingFiles().then(() => {
            this.guardando = false;
            this.guardadoOk = true;
            setTimeout(() => { this.guardadoOk = false; }, 3000);
          });
        } else {
          this.guardando = false;
          this.error = res.mensaje;
        }
      },
      error: () => {
        this.guardando = false;
        this.error = 'Error al guardar el borrador. Intente nuevamente.';
      }
    });
  }

  enviar(): void {
    if (!this.formulario) return;

    const errores = this.validarFormularioCompleto();
    if (!this.idEmpresa) {
      errores.unshift('Debe seleccionar la empresa.');
    }
    if (errores.length > 0) {
      this.erroresValidacion = errores;
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }
    this.erroresValidacion = [];

    const { respuestas, respuestasTabla } = this.construirRespuestas();

    this.enviando = true;

    if (this.modoRegistrador) {
      this.registradorService.montarDenuncia({
        correoDestino: this.correoDestino ?? '',
        canalOrigen: this.canalOrigen ?? '',
        idFormulario: this.idFormulario,
        respuestas,
        respuestasTabla
      }).subscribe({
        next: (res) => {
          this.enviando = false;
          if (res.exito) {
            this.idDenuncia = res.idDenuncia ?? this.idDenuncia;
            this.exito = true;
            this.mensajeExito = res.consecutivo
              ? `Denuncia registrada. Número de radicado: ${res.consecutivo}`
              : `Denuncia registrada exitosamente.`;
          } else {
            this.error = res.mensaje;
          }
        },
        error: () => {
          this.enviando = false;
          this.error = 'Error al registrar la denuncia. Intente nuevamente.';
        }
      });
      return;
    }

    this.denunciaService.crearDenuncia({
      idDenuncia: this.idDenuncia,
      idFormulario: this.idFormulario,
      esAnonimo: this.esAnonimo,
      idEmpresa: this.idEmpresa!,
      respuestas,
      respuestasTabla
    }).subscribe({
      next: (res) => {
        if (res.exito) {
          this.idDenuncia = res.idDenuncia ?? this.idDenuncia;
          this.uploadPendingFiles().then(() => {
            this.enviando = false;
            this.exito = true;
            this.mensajeExito = res.consecutivo
              ? `Denuncia enviada. Número de radicado: ${res.consecutivo}`
              : `Denuncia enviada exitosamente.`;
          });
        } else {
          this.enviando = false;
          this.error = res.mensaje;
        }
      },
      error: () => {
        this.enviando = false;
        this.error = 'Error al enviar la denuncia. Intente nuevamente.';
      }
    });
  }

  private async uploadPendingFiles(): Promise<void> {
    if (!this.idDenuncia || this.archivosSeleccionados.size === 0) return;
    this.subiendoAdjuntos = true;
    for (const [idCampo, files] of this.archivosSeleccionados.entries()) {
      for (const file of files) {
        try {
          const adj = await firstValueFrom(
            this.denunciaService.subirAdjunto(this.idDenuncia, idCampo, file)
          );
          const lista = this.adjuntosSubidos.get(idCampo) ?? [];
          lista.push(adj);
          this.adjuntosSubidos.set(idCampo, lista);
        } catch {  }
      }
      this.archivosSeleccionados.delete(idCampo);
    }
    this.subiendoAdjuntos = false;
  }

  private construirRespuestas(): { respuestas: RespuestaCampoDto[]; respuestasTabla: RespuestaTablaDto[] } {
    const respuestas: RespuestaCampoDto[] = [];
    const respuestasTabla: RespuestaTablaDto[] = [];

    this.formulario!.secciones.forEach(sec => {
      if (this.seccionesOcultas.has(sec.id)) return;
      sec.campos.forEach(campo => {
        if (campo.tipoCampo === this.TIPO.LABEL) return;
        if (this.camposOcultos.has(campo.id)) return;

        if (campo.tipoCampo === this.TIPO.TABLA) {
          (this.filasTabla[campo.id] ?? []).forEach((fila, idx) => {
            campo.camposHijo.forEach(hijo => {
              const val = fila[hijo.id];
              if (val !== null && val !== '') {
                respuestasTabla.push({
                  idCampoTabla: campo.id, idGrupo: idx,
                  idCampoHijo: hijo.id, textoRespuesta: String(val), idOpcion: null
                });
              }
            });
          });
        } else if (campo.tipoCampo === this.TIPO.SELECT_MULTIPLE) {
          (this.valoresMulti[campo.id] ?? []).forEach(idOpcion => {
            respuestas.push({ idCampo: campo.id, idOpcion, textoRespuesta: null });
          });
        } else if (campo.tipoCampo === this.TIPO.SELECT_UNICO) {
          const val = this.valores[campo.id];
          if (val != null && val !== '') {
            respuestas.push({ idCampo: campo.id, idOpcion: Number(val), textoRespuesta: null });
          }
        } else {
          const val = this.valores[campo.id];
          if (val != null && val !== '') {
            respuestas.push({ idCampo: campo.id, textoRespuesta: String(val), idOpcion: null });
          }
        }
      });
    });

    return { respuestas, respuestasTabla };
  }

  private nuevaFila(campo: CampoDto): FilaTabla {
    const fila: FilaTabla = {};
    campo.camposHijo.forEach(hijo => (fila[hijo.id] = null));
    return fila;
  }

  agregarFila(idCampo: number, campo: CampoDto): void {
    if (!this.filasTabla[idCampo]) this.filasTabla[idCampo] = [];
    this.filasTabla[idCampo].push(this.nuevaFila(campo));
  }

  eliminarFila(idCampo: number, index: number): void {
    if (this.filasTabla[idCampo]?.length > 1) {
      this.filasTabla[idCampo].splice(index, 1);
    }
  }

  private validarSeccion(seccion: SeccionDto): string[] {
    const errores: string[] = [];
    for (const campo of seccion.campos) {
      if (campo.tipoCampo === this.TIPO.LABEL) continue;
      if (!campo.obligatorio) continue;
      if (this.camposOcultos.has(campo.id)) continue;

      const nombre = campo.nombre ?? `Campo ${campo.id}`;

      if (campo.tipoCampo === this.TIPO.ADJUNTO) {
        const tieneArchivo = (this.adjuntosSubidos.get(campo.id)?.length ?? 0) > 0
                          || (this.archivosSeleccionados.get(campo.id)?.length ?? 0) > 0;
        if (!tieneArchivo) errores.push(nombre);
      } else if (campo.tipoCampo === this.TIPO.TABLA) {
        const filas = this.filasTabla[campo.id] ?? [];
        const hayContenido = filas.some(fila =>
          campo.camposHijo.some(hijo => fila[hijo.id] != null && fila[hijo.id] !== '')
        );
        if (!hayContenido) errores.push(nombre);
      } else if (campo.tipoCampo === this.TIPO.SELECT_MULTIPLE) {
        if (!(this.valoresMulti[campo.id]?.length)) errores.push(nombre);
      } else {
        const val = this.valores[campo.id];
        if (val == null || String(val).trim() === '') errores.push(nombre);
      }
    }
    return errores;
  }

  private validarFormularioCompleto(): string[] {
    const errores: string[] = [];
    for (const sec of this.seccionesVisibles) {
      errores.push(...this.validarSeccion(sec));
    }
    return errores;
  }

  irAPaso(index: number): void {
    this.erroresValidacion = [];
    this.currentStep = index;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  anteriorPaso(): void {
    if (!this.isFirstStep) {
      this.erroresValidacion = [];
      this.currentStep--;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  siguientePaso(): void {
    if (this.isLastStep) return;
    const errores = this.validarSeccion(this.seccionActual!);
    if (errores.length > 0) {
      this.erroresValidacion = errores;
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }
    this.erroresValidacion = [];
    this.currentStep++;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  getParametrosFiltrados(campo: CampoDto): ParametroDto[] {
    const todos = this.parametros[campo.tipoParametro] ?? [];
    if (campo.tipoParametroHijo === 0) return todos;
    const valorPadre = this.valores[campo.tipoParametroHijo];
    if (!valorPadre) return todos;
    return todos.filter(p => p.dependeDe === Number(valorPadre));
  }

  toggleMultiSelect(idCampo: number, idOpcion: number): void {
    if (!this.valoresMulti[idCampo]) this.valoresMulti[idCampo] = [];
    const arr = this.valoresMulti[idCampo];
    const idx = arr.indexOf(idOpcion);
    if (idx >= 0) arr.splice(idx, 1); else arr.push(idOpcion);
    this.evaluarCondiciones();
  }

  isMultiSelected(idCampo: number, idOpcion: number): boolean {
    return (this.valoresMulti[idCampo] ?? []).includes(idOpcion);
  }

  getColClass(col: number): string { return `col-md-${col}`; }

  getFieldClass(idCampo: number, col: number, esTabla = false): string {
    const base     = esTabla ? 'col-12' : `col-md-${col || 12}`;
    const secId    = this.seccionActual?.id ?? 0;
    const rechazado = this.esCampoRechazado(idCampo, secId);
    const bloqueado = !this.esCampoEditable(idCampo, secId);
    return [base, rechazado ? 'campo-rechazado' : '', bloqueado ? 'campo-bloqueado' : ''].filter(Boolean).join(' ');
  }

  onArchivoSeleccionado(event: Event, idCampo: number): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.agregarArchivos(idCampo, input.files);
    input.value = '';
  }

  private agregarArchivos(idCampo: number, files: FileList | File[]): void {
    const lista = this.archivosSeleccionados.get(idCampo) ?? [];
    Array.from(files).forEach(f => lista.push(f));
    this.archivosSeleccionados.set(idCampo, lista);
  }

  onDragEnterAdjunto(event: DragEvent, idCampo: number): void {
    event.preventDefault();
    this.archivoArrastrandoSobre = idCampo;
  }

  onDragOverAdjunto(event: DragEvent): void {
    event.preventDefault();
  }

  onDragLeaveAdjunto(event: DragEvent, idCampo: number): void {
    const zona = event.currentTarget as HTMLElement;
    const siguiente = event.relatedTarget as Node | null;
    if (!siguiente || !zona.contains(siguiente)) {
      if (this.archivoArrastrandoSobre === idCampo) this.archivoArrastrandoSobre = null;
    }
  }

  onDropAdjunto(event: DragEvent, idCampo: number): void {
    event.preventDefault();
    this.archivoArrastrandoSobre = null;
    if (!this.esCampoEditable(idCampo, this.seccionActual!.id)) return;
    const files = event.dataTransfer?.files;
    if (files?.length) this.agregarArchivos(idCampo, files);
  }

  quitarArchivoPendiente(idCampo: number, idx: number): void {
    const lista = this.archivosSeleccionados.get(idCampo);
    if (lista) { lista.splice(idx, 1); if (lista.length === 0) this.archivosSeleccionados.delete(idCampo); }
  }

  eliminarAdjuntoSubido(adj: AdjuntoDto): void {
    this.denunciaService.eliminarAdjunto(adj.id).subscribe({
      next: () => {
        const lista = this.adjuntosSubidos.get(adj.idCampo);
        if (lista) {
          const idx = lista.findIndex(a => a.id === adj.id);
          if (idx >= 0) lista.splice(idx, 1);
        }
      },
      error: () => { alert('Error al eliminar el archivo.'); }
    });
  }

  descargarAdjunto(idAdjunto: number, nombreOriginal: string): void {
    this.denunciaService.descargarAdjunto(idAdjunto).subscribe({
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

  adjuntosDeCampo(idCampo: number): AdjuntoDto[] {
    return this.adjuntosSubidos.get(idCampo) ?? [];
  }

  archivosPendientesDeCampo(idCampo: number): File[] {
    return this.archivosSeleccionados.get(idCampo) ?? [];
  }

  volver(): void { this.router.navigate(['/pages/inicio']); }
  resetearError(): void { this.error = null; }
}
