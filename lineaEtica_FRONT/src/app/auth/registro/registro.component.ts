import { Component, afterNextRender, NgZone, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../auth.service';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const clave = control.get('clave')?.value;
  const confirmar = control.get('confirmarClave')?.value;
  return clave && confirmar && clave !== confirmar ? { passwordsMismatch: true } : null;
}

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './registro.component.html',
  styleUrl: './registro.component.scss'
})
export class RegistroComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private ngZone = inject(NgZone);

  form = this.fb.group({
    nombre: ['', [Validators.required, Validators.minLength(3)]],
    correo: ['', [Validators.required, Validators.email]],
    clave: ['', [Validators.required, Validators.minLength(8)]],
    confirmarClave: ['', Validators.required],
    captcha: ['', Validators.required]
  }, { validators: passwordsMatch });

  isLoading = false;
  errorMessage = '';
  registroExitoso = false;
  private captchaCode = '';

  constructor() {
    afterNextRender(() => {
      this.generateCaptcha();
    });
  }

  get f() { return this.form.controls; }

  refreshCaptcha(): void {
    this.generateCaptcha();
  }

  registro(): void {
    if (this.form.invalid || this.isLoading) return;

    if (this.form.controls['captcha'].value !== this.captchaCode) {
      this.errorMessage = 'El código de verificación es incorrecto. Por favor, inténtelo de nuevo.';
      this.generateCaptcha();
      this.form.controls['captcha'].reset();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { nombre, correo, clave, confirmarClave } = this.form.value;

    this.authService.registro({ nombre: nombre!, correo: correo!, clave: clave!, confirmarClave: confirmarClave! })
      .subscribe({
        next: (res) => {
          this.isLoading = false;
          if (res.exito) {
            this.registroExitoso = true;
          } else {
            this.errorMessage = res.mensaje || 'No se pudo completar el registro.';
          }
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = typeof err === 'string' ? err : 'Error al registrar. Intente nuevamente.';
        }
      });
  }

  irAlLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  private generateCaptcha(): void {
    this.captchaCode = this.createCaptcha();
    this.ngZone.runOutsideAngular(() => {
      setTimeout(() => this.renderCaptcha(), 50);
    });
  }

  private createCaptcha(): string {
    const chars = 'ABCDEFGHJKMNOPQRSTUVWXYZabcdefghjklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < 6; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
  }

  private renderCaptcha(): void {
    const canvas = document.getElementById('captchaCanvasRegistro') as HTMLCanvasElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.font = '30px Arial';
    ctx.textBaseline = 'middle';
    ctx.textAlign = 'center';
    for (let i = 0; i < 5; i++) {
      ctx.strokeStyle = this.randomColor();
      ctx.beginPath();
      ctx.moveTo(Math.random() * canvas.width, Math.random() * canvas.height);
      ctx.lineTo(Math.random() * canvas.width, Math.random() * canvas.height);
      ctx.stroke();
    }
    ctx.fillStyle = this.randomColor();
    ctx.fillText(this.captchaCode, canvas.width / 2, canvas.height / 2);
    for (let i = 0; i < 150; i++) {
      ctx.fillStyle = this.randomColor();
      ctx.fillRect(Math.random() * canvas.width, Math.random() * canvas.height, 1, 1);
    }
  }

  private randomColor(): string {
    const h = '0123456789ABCDEF';
    return '#' + Array.from({ length: 6 }, () => h[Math.floor(Math.random() * 16)]).join('');
  }
}
