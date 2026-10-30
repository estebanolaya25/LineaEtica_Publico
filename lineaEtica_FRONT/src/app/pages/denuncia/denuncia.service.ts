import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../core/config.service';

export interface CampoDto {
  id: number;
  nombre: string | null;
  descripcion: string | null;
  ayuda: string | null;
  obligatorio: boolean;
  orden: number;
  tipoCampo: number;
  tipoParametro: number;
  tipoParametroHijo: number;
  col: number;
  idCampo: number;
  cantAdjuntos: number;
  camposHijo: CampoDto[];
}

export interface SeccionDto {
  id: number;
  nombre: string;
  descripcion: string | null;
  mostrarNombre: boolean;
  orden: number;
  campos: CampoDto[];
}

export interface CondicionDto {
  id: number;
  idCampoOrigen: number | null;
  condicion: string;
  valorCondicion: string | null;
  idSeccionDestino: number;
  idCampoDestino: number | null;
  accion: string;
}

export interface FormularioDto {
  id: number;
  nombre: string;
  secciones: SeccionDto[];
  condiciones: CondicionDto[];
}

export interface ParametroDto {
  id: number;
  nombre: string;
  codigo: string | null;
  dependeDe: number;
}

export interface RespuestaCampoDto {
  idCampo: number;
  idOpcion?: number | null;
  textoRespuesta?: string | null;
}

export interface RespuestaTablaDto {
  idCampoTabla: number;
  idGrupo: number;
  idCampoHijo: number;
  textoRespuesta?: string | null;
  idOpcion?: number | null;
}

export interface DenunciaRequestDto {
  idDenuncia?: number | null;
  idFormulario: number;
  esAnonimo: boolean;
  idEmpresa: number;
  respuestas: RespuestaCampoDto[];
  respuestasTabla: RespuestaTablaDto[];
}

export interface CampoRechazadoDto {
  idCampo: number;
  idSeccion: number;
  idFila?: number | null;
}

export interface BorradorDto {
  id: number;
  idFormulario: number;
  idEstado: number;
  respuestas: RespuestaCampoDto[];
  respuestasTabla: RespuestaTablaDto[];
  camposRechazados: CampoRechazadoDto[];
  comentarioAdmin: string | null;
}

export interface DenunciaResponseDto {
  exito: boolean;
  mensaje: string;
  idDenuncia?: number;
  consecutivo?: number;
}

export interface MisDenunciaDto {
  id: number;
  consecutivo: number | null;
  nombreFormulario: string;
  estado: string;
  fechaCreacion: string | null;
  esAnonimo: boolean;
}

export interface DenunciaAdminResumenDto {
  id: number;
  consecutivo: number | null;
  nombreFormulario: string;
  estado: string;
  idEstado: number;
  fechaCreacion: string | null;
  esAnonimo: boolean;
  usuarioReportante: string | null;
  idEmpresa: number | null;
  nombreEmpresa: string | null;
}

export interface DenunciaDetalleAdminDto {
  id: number;
  idFormulario: number;
  idEstado: number;
  estado: string;
  esAnonimo: boolean;
  usuarioReportante: string | null;
  fechaCreacion: string | null;
  consecutivo: number | null;
  respuestas: RespuestaCampoDto[];
  respuestasTabla: RespuestaTablaDto[];
  camposRechazados: CampoRechazadoDto[];
  comentarioAdmin: string | null;
  idEmpresa: number | null;
  nombreEmpresa: string | null;
}

export interface DecisionAdminRequestDto {
  comentario?: string | null;
  camposRechazados: CampoRechazadoDto[];
}

export interface EstMesDto {
  mes: string;
  cantidad: number;
}

export interface EstEstadoDto {
  idEstado: number;
  estado: string;
  cantidad: number;
}

export interface EstEmpresaDto {
  idEmpresa: number | null;
  nombreEmpresa: string;
  cantidad: number;
}

export interface EstCanalDto {
  canal: string;
  cantidad: number;
}

export interface EstadisticasDto {
  total: number;
  pendientes: number;
  aprobadas: number;
  rechazadas: number;
  devueltas: number;
  enInvestigacion: number;
  cerradas: number;
  anonimas: number;
  noAnonimas: number;
  porMes: EstMesDto[];
  porEstado: EstEstadoDto[];
  porEmpresa: EstEmpresaDto[];
  porCanal: EstCanalDto[];
}

export interface AdjuntoDto {
  id: number;
  idCampo: number;
  nombreOriginal: string;
  tipoMime: string;
  tamanoBytes: number;
  fechaSubida: string;
}

export const TIPO_CAMPO = {
  TEXTO: 1,
  OBSERVACION: 2,
  FECHA: 3,
  SELECT_UNICO: 4,
  SELECT_MULTIPLE: 5,
  NUMERO: 6,
  TABLA: 7,
  ADJUNTO: 8,
  EMAIL: 9,
  LABEL: 10,
  TIME: 11
} as const;

@Injectable({ providedIn: 'root' })
export class DenunciaService {
  private http = inject(HttpClient);
  private configService = inject(ConfigService);

  private get apiUrl(): string {
    return `${this.configService.apiUrl}/denuncia`;
  }

  private get adminUrl(): string {
    return `${this.configService.apiUrl}/admin/denuncia`;
  }

  getFormulario(idFormulario: number): Observable<FormularioDto> {
    return this.http.get<FormularioDto>(`${this.apiUrl}/formulario/${idFormulario}`);
  }

  getParametros(idTipo: number): Observable<ParametroDto[]> {
    return this.http.get<ParametroDto[]>(`${this.apiUrl}/parametros/${idTipo}`);
  }

  crearDenuncia(request: DenunciaRequestDto): Observable<DenunciaResponseDto> {
    return this.http.post<DenunciaResponseDto>(`${this.apiUrl}/crear`, request);
  }

  guardarDenuncia(request: DenunciaRequestDto): Observable<DenunciaResponseDto> {
    return this.http.post<DenunciaResponseDto>(`${this.apiUrl}/guardar`, request);
  }

  getBorrador(idDenuncia: number): Observable<BorradorDto> {
    return this.http.get<BorradorDto>(`${this.apiUrl}/borrador/${idDenuncia}`);
  }

  getMisDenuncias(): Observable<MisDenunciaDto[]> {
    return this.http.get<MisDenunciaDto[]>(`${this.apiUrl}/mis-denuncias`);
  }

  getEstadisticas(idEmpresa?: number | null): Observable<EstadisticasDto> {
    const params: Record<string, string> = {};
    if (idEmpresa != null) params['idEmpresa'] = idEmpresa.toString();
    return this.http.get<EstadisticasDto>(`${this.adminUrl}/estadisticas`, { params });
  }

  getTodasDenuncias(): Observable<DenunciaAdminResumenDto[]> {
    return this.http.get<DenunciaAdminResumenDto[]>(`${this.adminUrl}/todas`);
  }

  getDenunciaDetalleAdmin(idDenuncia: number): Observable<DenunciaDetalleAdminDto> {
    return this.http.get<DenunciaDetalleAdminDto>(`${this.adminUrl}/${idDenuncia}`);
  }

  aprobarDenuncia(idDenuncia: number, dto: DecisionAdminRequestDto): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${this.adminUrl}/${idDenuncia}/aprobar`, dto);
  }

  devolverDenuncia(idDenuncia: number, dto: DecisionAdminRequestDto): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${this.adminUrl}/${idDenuncia}/devolver`, dto);
  }

  rechazarDenuncia(idDenuncia: number, dto: DecisionAdminRequestDto): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${this.adminUrl}/${idDenuncia}/rechazar`, dto);
  }

  subirAdjunto(idDenuncia: number, idCampo: number, archivo: File): Observable<AdjuntoDto> {
    const form = new FormData();
    form.append('archivo', archivo, archivo.name);
    return this.http.post<AdjuntoDto>(`${this.apiUrl}/${idDenuncia}/adjunto/${idCampo}`, form);
  }

  getAdjuntos(idDenuncia: number): Observable<AdjuntoDto[]> {
    return this.http.get<AdjuntoDto[]>(`${this.apiUrl}/${idDenuncia}/adjuntos`);
  }

  getAdjuntosAdmin(idDenuncia: number): Observable<AdjuntoDto[]> {
    return this.http.get<AdjuntoDto[]>(`${this.adminUrl}/${idDenuncia}/adjuntos`);
  }

  eliminarAdjunto(idAdjunto: number): Observable<{ mensaje: string }> {
    return this.http.delete<{ mensaje: string }>(`${this.apiUrl}/adjunto/${idAdjunto}`);
  }

  downloadUrlAdjunto(idAdjunto: number): string {
    return `${this.apiUrl}/adjunto/${idAdjunto}`;
  }

  descargarAdjunto(idAdjunto: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/adjunto/${idAdjunto}`, { responseType: 'blob' });
  }

  downloadUrlAdjuntoAdmin(idAdjunto: number): string {
    return `${this.adminUrl}/adjunto/${idAdjunto}`;
  }

  descargarAdjuntoAdmin(idAdjunto: number): Observable<Blob> {
    return this.http.get(`${this.adminUrl}/adjunto/${idAdjunto}`, { responseType: 'blob' });
  }
}
