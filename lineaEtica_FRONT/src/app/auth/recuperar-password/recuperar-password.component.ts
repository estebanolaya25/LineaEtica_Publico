import { Component, OnInit, inject, afterNextRender } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { Dialog } from '@angular/cdk/dialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../auth.service';
import { MensajeModalComponent } from '../mensaje-modal/mensaje-modal.component';

@Component({
  selector: 'app-recuperar-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslateModule],
  templateUrl: './recuperar-password.component.html',
  styleUrl: './recuperar-password.component.scss',
})
export class RecuperarPasswordComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private dialog = inject(Dialog);
  private translate = inject(TranslateService);

  isLoading = false;
  Lang = 'es';
  private captchaCode = '';

  form: FormGroup = new FormGroup({
    correoElectronico: new FormControl('', [Validators.required, Validators.email]),
    captcha: new FormControl('', [Validators.required]),
  });

  constructor() {
    afterNextRender(() => {
      this.generateCaptcha();
    });
  }

  ngOnInit(): void {
    this.Lang = localStorage.getItem('language') || 'es';
    this.translate.use(this.Lang);
  }

  enviar(): void {
    if (this.form.controls['captcha'].value !== this.captchaCode) {
      this.openModal(
        this.Lang === 'es' ? 'El CAPTCHA ingresado es incorrecto.' : 'The CAPTCHA entered is incorrect.',
        '',
        true
      );
      this.generateCaptcha();
      return;
    }

    this.isLoading = true;
    const { correoElectronico } = this.form.value;

    this.authService.recuperacion(correoElectronico).subscribe({
      next: () => {
        this.isLoading = false;
        const dialogRef = this.dialog.open(MensajeModalComponent, {
          data: {
            name: this.Lang === 'es'
              ? 'Se ha enviado un correo con las instrucciones para recuperar su contraseña.'
              : 'An email with instructions to recover your password has been sent.',
            title: '',
            isError: false,
          },
          panelClass: 'modal-overlay',
        });
        dialogRef.closed.subscribe(() => this.router.navigate(['/auth/login']));
      },
      error: () => {
        this.isLoading = false;
        this.openModal(
          this.Lang === 'es' ? 'Error al enviar. Verifique los datos.' : 'Error sending. Please check your data.',
          'Error',
          true
        );
      },
    });
  }

  refreshCaptcha(): void {
    this.generateCaptcha();
  }

  private openModal(name: string, title: string, isError: boolean): void {
    this.dialog.open(MensajeModalComponent, {
      data: { name, title, isError },
      panelClass: 'modal-overlay',
    });
  }

  private generateCaptcha(): void {
    this.captchaCode = this.createCaptcha();
    setTimeout(() => this.renderCaptcha(), 50);
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
    const canvas = document.getElementById('captchaCanvasR') as HTMLCanvasElement;
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
