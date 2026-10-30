import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../core/config.service';

export interface DenunciaInvestigadorResumenDto {
  id: number;
  consecutivo: number | null;
  nombreFormulario: string;
  estado: string;
  idEstado: number;
  esAnonimo: boolean;
  fechaCreacion: string | null;
  fechaAsignacion: string | null;
}

export interface DenunciaInvestigadorDetalleDto {
  id: number;
  idFormulario: number;
  idEstado: number;
  estado: string;
  esAnonimo: boolean;
  fechaCreacion: string | null;
  consecutivo: number | null;
  comentarioAdmin: string | null;
  nombreInvestigador: string | null;
  correoInvestigador: string | null;
  respuestas: RespuestaCampoDto[];
  respuestasTabla: RespuestaTablaDto[];
  seguimientos: SeguimientoDto[];
}

export interface RespuestaCampoDto {
  idCampo: number;
  idOpcion: number | null;
  textoRespuesta: string | null;
}

export interface RespuestaTablaDto {
  idCampoTabla: number;
  idGrupo: number;
  idCampoHijo: number;
  textoRespuesta: string | null;
  idOpcion: number | null;
}

export interface SeguimientoDto {
  id: number;
  comentario: string;
  fecha: string;
  usuario: string;
  adjuntos: AdjuntoInvestigadorDto[];
}

export interface AdjuntoInvestigadorDto {
  id: number;
  nombreOriginal: string;
  nombreArchivo: string;
  tipoMime: string;
  tamanoBytes: number;
  fechaSubida: string;
  usuario: string;
}

export interface InvestigadorResumenDto {
  id: number;
  nombre: string;
  correo: string;
}

export interface InvestigacionAdminResumenDto {
  id: number;
  consecutivo: number | null;
  nombreFormulario: string;
  estado: string;
  idEstado: number;
  esAnonimo: boolean;
  fechaCreacion: string | null;
  fechaAsignacion: string | null;
  nombreInvestigador: string | null;
  correoInvestigador: string | null;
  cantidadSeguimientos: number;
  idEmpresa: number | null;
  nombreEmpresa: string | null;
}

@Injectable({ providedIn: 'root' })
export class InvestigadorService {
  private http = inject(HttpClient);
  private cfg  = inject(ConfigService);

  private get base(): string { return `${this.cfg.apiUrl}/investigador`; }
  private get adminBase(): string { return `${this.cfg.apiUrl}/admin`; }

  getMisDenuncias(): Observable<DenunciaInvestigadorResumenDto[]> {
    return this.http.get<DenunciaInvestigadorResumenDto[]>(`${this.base}/mis-denuncias`);
  }

  getDetalle(idDenuncia: number): Observable<DenunciaInvestigadorDetalleDto> {
    return this.http.get<DenunciaInvestigadorDetalleDto>(`${this.base}/denuncia/${idDenuncia}`);
  }

  getSeguimientos(idDenuncia: number): Observable<SeguimientoDto[]> {
    return this.http.get<SeguimientoDto[]>(`${this.base}/denuncia/${idDenuncia}/seguimientos`);
  }

  getInvestigadoresDisponibles(): Observable<InvestigadorResumenDto[]> {
    return this.http.get<InvestigadorResumenDto[]>(`${this.base}/investigadores`);
  }

  reasignarInvestigacion(idDenuncia: number, idNuevoInvestigador: number, comentario: string): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(
      `${this.base}/denuncia/${idDenuncia}/reasignar`,
      { idNuevoInvestigador, comentario }
    );
  }

  crearSeguimiento(idDenuncia: number, comentario: string, archivos: File[]): Observable<SeguimientoDto> {
    const form = new FormData();
    form.append('comentario', comentario);
    archivos.forEach(f => form.append('archivos', f, f.name));
    return this.http.post<SeguimientoDto>(`${this.base}/denuncia/${idDenuncia}/seguimiento`, form);
  }

  descargarAdjunto(idAdjunto: number): Observable<Blob> {
    return this.http.get(`${this.base}/adjunto/${idAdjunto}`, { responseType: 'blob' });
  }

  cerrarInvestigacion(idDenuncia: number, comentario: string): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${this.base}/denuncia/${idDenuncia}/cerrar`, { comentario });
  }

  solicitarInformacion(idDenuncia: number, comentario: string, archivos: File[]): Observable<SeguimientoDto> {
    const form = new FormData();
    form.append('comentario', comentario);
    archivos.forEach(f => form.append('archivos', f, f.name));
    return this.http.post<SeguimientoDto>(`${this.base}/denuncia/${idDenuncia}/solicitar-informacion`, form);
  }

  responderInvestigador(idDenuncia: number, comentario: string, archivos: File[]): Observable<SeguimientoDto> {
    const form = new FormData();
    form.append('comentario', comentario);
    archivos.forEach(f => form.append('archivos', f, f.name));
    return this.http.post<SeguimientoDto>(`${this.adminBase}/investigacion/${idDenuncia}/responder-investigador`, form);
  }

  getInvestigadores(): Observable<InvestigadorResumenDto[]> {
    return this.http.get<InvestigadorResumenDto[]>(`${this.adminBase}/investigadores`);
  }

  asignarInvestigador(idDenuncia: number, idInvestigador: number, comentario?: string): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(
      `${this.adminBase}/denuncia/${idDenuncia}/asignar-investigador`,
      { idInvestigador, comentario }
    );
  }

  getInvestigacionesAdmin(): Observable<InvestigacionAdminResumenDto[]> {
    return this.http.get<InvestigacionAdminResumenDto[]>(`${this.adminBase}/investigaciones`);
  }

  getDetalleAdmin(idDenuncia: number): Observable<DenunciaInvestigadorDetalleDto> {
    return this.http.get<DenunciaInvestigadorDetalleDto>(`${this.adminBase}/investigacion/${idDenuncia}`);
  }

  descargarAdjuntoAdmin(idAdjunto: number): Observable<Blob> {
    return this.http.get(`${this.adminBase}/investigacion-adjunto/${idAdjunto}`, { responseType: 'blob' });
  }

  agregarComentarioAdmin(idDenuncia: number, comentario: string, archivos: File[]): Observable<SeguimientoDto> {
    const form = new FormData();
    form.append('comentario', comentario);
    archivos.forEach(f => form.append('archivos', f, f.name));
    return this.http.post<SeguimientoDto>(`${this.adminBase}/investigacion/${idDenuncia}/comentario`, form);
  }
}
