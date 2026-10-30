import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuariosService, UsuarioListDto, UsuarioCreateDto } from './usuarios.service';
import { AuthService } from '../../../auth/auth.service';
import { EmpresaService, EmpresaDto } from '../../../core/empresa.service';

type Vista = 'lista' | 'nuevo' | 'editar';

@Component({
  selector: 'app-gestion-usuarios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './gestion-usuarios.component.html',
  styleUrl: './gestion-usuarios.component.scss'
})
export class GestionUsuariosComponent implements OnInit {
  private svc      = inject(UsuariosService);
  private auth     = inject(AuthService);
  private empresaSvc = inject(EmpresaService);

  readonly esAdmin        = this.auth.getCurrentUserRole() === 'Consultor';
  readonly esDuenoProceso = this.auth.getCurrentUserRole() === 'Jefe del proceso';
  readonly miEmpresa      = this.auth.getCurrentUserEmpresa();

  empresas: EmpresaDto[] = [];
  usuarios: UsuarioListDto[] = [];
  cargando = true;
  error: string | null = null;
  mensaje: string | null = null;
  procesando = false;
  vista: Vista = 'lista';

  form: UsuarioCreateDto = this.formVacio();

  readonly tiposUsuario = [
    { id: 1, label: 'Consultor' },
    { id: 3, label: 'Investigador' },
    { id: 4, label: 'Jefe del proceso' },
    { id: 5, label: 'Registrador' },
  ];

  readonly tiposDocumento = [
    { id: 1,  label: 'Cédula de ciudadanía' },
    { id: 2,  label: 'Tarjeta de identidad' },
    { id: 3,  label: 'Cédula de extranjería' },
    { id: 4,  label: 'Pasaporte' },
  ];

  ngOnInit(): void {
    this.cargar();
    this.empresaSvc.listar().subscribe({
      next: (lista) => { this.empresas = lista; },
      error: () => {  }
    });
  }

  private cargar(): void {
    this.cargando = true;
    this.svc.listar().subscribe({
      next:  (lista) => { this.usuarios = lista; this.cargando = false; },
      error: ()      => { this.error = 'Error al cargar usuarios.'; this.cargando = false; }
    });
  }

  iniciarNuevo(): void {
    this.form  = this.formVacio();
    this.vista = 'nuevo';
    this.error = null;
    this.mensaje = null;
  }

  iniciarEditar(u: UsuarioListDto): void {
    this.form = {
      id:                 u.id,
      nombres:            u.nombres,
      apellidos:          u.apellidos,
      tipoIdentificacion: u.tipoIdentificacion || 1,
      identificacion:     u.identificacion,
      email:              u.email,
      idTipoUsuario:      u.idTipoUsuario,
      activo:             u.activo,
      lang:               'es',
      idEmpresa:          u.idEmpresa,
    };
    this.vista = 'editar';
    this.error = null;
    this.mensaje = null;
  }

  cancelar(): void {
    this.vista = 'lista';
    this.error = null;
    this.mensaje = null;
  }

  guardar(): void {
    if (this.procesando) return;
    if (!this.form.nombres.trim() || !this.form.email.trim()) {
      this.error = 'Nombre y correo son obligatorios.';
      return;
    }
    if (this.form.idTipoUsuario !== 4 && !this.form.idEmpresa) {
      this.error = 'Debe seleccionar la empresa para este tipo de usuario.';
      return;
    }

    this.procesando = true;
    this.error = null;

    const op = this.vista === 'nuevo'
      ? this.svc.crear(this.form)
      : this.svc.editar(this.form);

    op.subscribe({
      next: () => {
        this.procesando = false;
        this.mensaje    = this.vista === 'nuevo'
          ? 'Usuario creado. Se envió la contraseña temporal por correo.'
          : 'Usuario actualizado. Se envió nueva contraseña por correo.';
        this.vista = 'lista';
        this.cargar();
      },
      error: (err) => {
        this.procesando = false;
        this.error      = err?.error?.mensaje ?? err?.message ?? 'Error al guardar usuario.';
      }
    });
  }

  private formVacio(): UsuarioCreateDto {
    return {
      id: 0, nombres: '', apellidos: '', tipoIdentificacion: 1, identificacion: '', email: '',
      idTipoUsuario: 1, activo: true, lang: 'es',
      idEmpresa: this.esAdmin ? this.miEmpresa : null,
    };
  }

  empresaDeshabilitada(): boolean {
    return this.form.idTipoUsuario === 4 || this.esAdmin;
  }

  onTipoUsuarioChange(): void {
    if (this.form.idTipoUsuario === 4) {
      this.form.idEmpresa = null;
    } else if (this.esAdmin) {
      this.form.idEmpresa = this.miEmpresa;
    }
  }

  badgeTipo(idTipo: number): string {
    const map: Record<number, string> = { 1: 'badge-admin', 3: 'badge-investigador', 4: 'badge-dueno', 5: 'badge-registrador' };
    return map[idTipo] ?? 'bg-secondary';
  }

  nombreTipo(idTipo: number): string {
    return this.tiposUsuario.find(t => t.id === idTipo)?.label
        ?? this.tiposDocumento.find(t => t.id === idTipo)?.label
        ?? `Tipo ${idTipo}`;
  }
}
