import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../../core/config.service';

export interface UsuarioListDto {
  id: number;
  nombres: string;
  apellidos: string;
  email: string;
  identificacion: string;
  tipoIdentificacion: number;
  usuario: string;
  idTipoUsuario: number;
  nombreTipoUsuario: string;
  activo: boolean;
  idEmpresa: number | null;
  nombreEmpresa: string | null;
}

export interface UsuarioCreateDto {
  id: number;
  nombres: string;
  apellidos: string;
  tipoIdentificacion: number;
  identificacion: string;
  email: string;
  idTipoUsuario: number;
  activo: boolean;
  lang: string;
  idEmpresa: number | null;
}

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private http          = inject(HttpClient);
  private configService = inject(ConfigService);

  private get apiUrl(): string {
    return `${this.configService.apiUrl}/auth`;
  }

  listar(): Observable<UsuarioListDto[]> {
    return this.http.get<UsuarioListDto[]>(`${this.apiUrl}/listaUsuariosAPP`);
  }

  crear(dto: UsuarioCreateDto): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/addnewuser`, dto);
  }

  editar(dto: UsuarioCreateDto): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/edituser`, dto);
  }
}
