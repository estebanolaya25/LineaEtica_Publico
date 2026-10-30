import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../core/config.service';
import { RespuestaCampoDto, RespuestaTablaDto } from '../denuncia/denuncia.service';

export interface BuscarUsuarioResponseDto {
  existe: boolean;
  nombre: string | null;
  correo: string | null;
}

export interface MontarDenunciaRequestDto {
  correoDestino: string;
  nombreDestino?: string | null;
  canalOrigen: string;
  idFormulario: number;
  respuestas: RespuestaCampoDto[];
  respuestasTabla: RespuestaTablaDto[];
}

export interface MontarDenunciaResponseDto {
  exito: boolean;
  mensaje: string;
  idDenuncia?: number;
  consecutivo?: number;
  usuarioCreado: boolean;
}

export interface DenunciaRegistradorResumenDto {
  id: number;
  consecutivo: number | null;
  nombreFormulario: string;
  estado: string;
  idEstado: number;
  esAnonimo: boolean;
  fechaCreacion: string | null;
  origenCanal: string | null;
}

@Injectable({ providedIn: 'root' })
export class RegistradorService {
  private http = inject(HttpClient);
  private configService = inject(ConfigService);

  private get apiUrl(): string {
    return `${this.configService.apiUrl}/registrador`;
  }

  buscarUsuario(correo: string): Observable<BuscarUsuarioResponseDto> {
    return this.http.get<BuscarUsuarioResponseDto>(`${this.apiUrl}/buscar-usuario`, { params: { correo } });
  }

  montarDenuncia(request: MontarDenunciaRequestDto): Observable<MontarDenunciaResponseDto> {
    return this.http.post<MontarDenunciaResponseDto>(`${this.apiUrl}/montar-denuncia`, request);
  }

  getMisDenunciasMontadas(): Observable<DenunciaRegistradorResumenDto[]> {
    return this.http.get<DenunciaRegistradorResumenDto[]>(`${this.apiUrl}/mis-denuncias`);
  }
}
