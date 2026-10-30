import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RegistradorService, DenunciaRegistradorResumenDto } from '../registrador.service';
import { AuthService } from '../../../auth/auth.service';

interface CanalRow {
  canal: string;
  cantidad: number;
  widthPct: number;
}

const CANAL_ICONS: Record<string, string> = {
  Correo:      'bi-envelope-fill',
  WhatsApp:    'bi-whatsapp',
  Presencial:  'bi-person-fill',
  Telefono:    'bi-telephone-fill',
  Otro:        'bi-three-dots',
};

@Component({
  selector: 'app-registrador-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './registrador-dashboard.component.html',
  styleUrl: './registrador-dashboard.component.scss'
})
export class RegistradorDashboardComponent implements OnInit {
  private svc    = inject(RegistradorService);
  private router = inject(Router);
  private auth   = inject(AuthService);

  readonly empresaNombre = this.auth.getCurrentUserEmpresaNombre();

  cargando = true;
  error: string | null = null;
  denuncias: DenunciaRegistradorResumenDto[] = [];
  canalRows: CanalRow[] = [];

  ngOnInit(): void {
    this.svc.getMisDenunciasMontadas().subscribe({
      next: (d) => {
        this.denuncias = d;
        this.buildCanalRows(d);
        this.cargando = false;
      },
      error: () => {
        this.error = 'No se pudieron cargar sus denuncias montadas.';
        this.cargando = false;
      }
    });
  }

  get total(): number { return this.denuncias.length; }

  get anonimas(): number {
    return this.denuncias.filter(d => d.esAnonimo).length;
  }

  get noAnonimas(): number {
    return this.denuncias.filter(d => !d.esAnonimo).length;
  }

  get enProceso(): number {
    return this.denuncias.filter(d => d.idEstado !== 9).length;
  }

  get cerradas(): number {
    return this.denuncias.filter(d => d.idEstado === 9).length;
  }

  private buildCanalRows(denuncias: DenunciaRegistradorResumenDto[]): void {
    const porCanal = new Map<string, number>();
    for (const d of denuncias) {
      const canal = d.origenCanal || 'Otro';
      porCanal.set(canal, (porCanal.get(canal) ?? 0) + 1);
    }
    const maxVal = Math.max(...Array.from(porCanal.values()), 1);
    this.canalRows = Array.from(porCanal.entries())
      .map(([canal, cantidad]) => ({ canal, cantidad, widthPct: (cantidad / maxVal) * 100 }))
      .sort((a, b) => b.cantidad - a.cantidad);
  }

  getCanalIcon(canal: string): string {
    return CANAL_ICONS[canal] ?? 'bi-question-circle';
  }

  irMontarDenuncia(): void {
    this.router.navigate(['/pages/registrador/montar-denuncia']);
  }
}
