import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';

export interface EmpresaDto {
  id: number;
  nombre: string;
  activo: boolean;
}

@Injectable({ providedIn: 'root' })
export class EmpresaService {
  private http = inject(HttpClient);
  private configService = inject(ConfigService);

  private get apiUrl(): string {
    return `${this.configService.apiUrl}/empresa`;
  }

  listar(): Observable<EmpresaDto[]> {
    return this.http.get<EmpresaDto[]>(`${this.apiUrl}/listar`);
  }
}
